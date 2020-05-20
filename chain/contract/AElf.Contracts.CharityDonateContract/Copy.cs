using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Linq;
using AElf.Contracts.Consensus.AEDPoS;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Sdk.CSharp;
using AElf.Types;

namespace AElf.Contracts.BingoGameContract
{
    public class BingoGameContract : BingoGameContractContainer.BingoGameContractBase
    {
        /// <summary>
        /// Initial reference contracts' address; 初始化合约地址
        /// create CARD token and issue to contract itself. 创建CARD名字的token发行给合约自己
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty Initial(Empty input) // 初始化这份合约
        {
            Assert(!State.Initialized.Value, "Already initialized."); //每一次掉合约，检查初始化属性，应该未初始化
            State.TokenContract.Value = // 拿到这两个合约的地址，设为本数据库的属性，从而使用其方法
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.ConsensusContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);

            // Create and issue token of this contract.
            State.TokenContract.Create.Send(new CreateInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                TokenName = "Bingo Card",
                Decimals = 0,
                Issuer = Context.Self,
                IsBurnable = true,
                TotalSupply = long.MaxValue,
                LockWhiteList = {Context.Self}
            });

            State.TokenContract.Issue.Send(new IssueInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = long.MaxValue,
                To = Context.Self,
                Memo = "All to issuer."
            });
            State.Initialized.Value = true; // 已初始化过
            return new Empty();
        }

        /// <summary>
        /// Give user a certain amount of CARD tokens for free. 免费给玩家一些card
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty Register(Empty input) // 注册新玩家
        {
            Assert(State.PlayerInformation[Context.Sender] == null, $"User {Context.Sender} already registered."); // 表里有用户信息，则不能再注册
            var information = new PlayerInformation //初始化玩家信息
            {
                // The value of seed will influence user's game result in some aspects.
                Seed = Context.TransactionId //种子：
            };
            State.PlayerInformation[Context.Sender] = information; // 存库

            State.TokenContract.Transfer.Send(new TransferInput // 给该玩家转一笔钱，按照默认常量的金额
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = BingoGameContractConstants.InitialCards,
                To = Context.Sender,
                Memo = "Initial Bingo Cards for player."
            });

            return new Empty();
        }

        public override Empty Deposit(SInt64Value input)
        {
            var playerInformation = State.PlayerInformation[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");
            Assert(input.Value > 0, "At least you should buy 1 CARD.");
            var elfAmount = input.Value.Mul(1_0000_0000);
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                Symbol = Context.Variables.NativeSymbol,
                Amount = elfAmount,
                From = Context.Sender,
                To = Context.Self,
                Memo = "Thanks for recharging:)"
            });
            State.TokenContract.Transfer.Send(new TransferInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = input.Value,
                To = Context.Sender,
                Memo = "Now you are stronger:)"
            });

            return new Empty();
        }

        public override SInt64Value Play(SInt64Value input) // 玩，入一个数额，出一个数额
        {
            Assert(input.Value > 1, "Invalid bet amount."); //下注必须大于1
            var playerInformation = State.PlayerInformation[Context.Sender]; // 读玩家信息
            Assert(playerInformation != null, $"User {Context.Sender} not registered before."); //没登记

            Context.LogDebug(() => $"Playing with amount {input.Value}"); //debug日志里添加当前状态

            State.TokenContract.TransferFrom.Send(new TransferFromInput 
            {
                From = Context.Sender,
                To = Context.Self,
                Amount = input.Value,
                Symbol = BingoGameContractConstants.CardSymbol,
                Memo = "Enjoy!"
            });

            var currentRound = State.ConsensusContract.GetCurrentRoundInformation.Call(new Empty());

            playerInformation.Bouts.Add(new BoutInformation
            {
                // Record current round number.
                PlayRoundNumber = currentRound.RoundNumber,
                Amount = input.Value,
                PlayId = Context.TransactionId
            });

            State.PlayerInformation[Context.Sender] = playerInformation;

            return new SInt64Value {Value = CalculateWaitingMilliseconds(currentRound)};
        }

        private long CalculateWaitingMilliseconds(Round round)
        {
            var extraBlockProducerExpectedTime = round.RealTimeMinersInformation.Values.OrderByDescending(i => i.Order)
                .First().ExpectedMiningTime.AddMilliseconds(4000);
            var expectedTimeOfGettingRandomNumber = extraBlockProducerExpectedTime.AddMilliseconds(8000);
            return (expectedTimeOfGettingRandomNumber - Context.CurrentBlockTime).Milliseconds();
        }

        public override BoolValue Bingo(Hash input) // 
        {
            Context.LogDebug(() => $"Getting game result of play id: {input.ToHex()}");

            var playerInformation = State.PlayerInformation[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");
            Assert(playerInformation.Bouts.Count > 0, $"User {Context.Sender} seems never join this game.");

            var boutInformation = input == Hash.Empty
                ? playerInformation.Bouts.First(i => i.BingoRoundNumber == 0)
                : playerInformation.Bouts.FirstOrDefault(i => i.PlayId == input);

            Assert(boutInformation != null, "Bouts not found.");
            Assert(!boutInformation.IsComplete, "Bout already finished.");

            var targetRound = State.ConsensusContract.GetRoundInformation.Call(new Int64Value
            {
                Value = boutInformation.PlayRoundNumber.Add(1)
            });
            Assert(targetRound != null, "Still preparing your game result, please wait for a while :)");

            var randomHash = targetRound.RealTimeMinersInformation.Values.First(i => i.PreviousInValue != null).PreviousInValue;
            var isWin = ConvertHashToBool(randomHash);

            var usefulHash = Hash.FromTwoHashes(randomHash, playerInformation.Seed);
            var award = CalculateAward(boutInformation.Amount, GetKindFromHash(usefulHash));
            award = isWin ? award : -award;

            var transferAmount = boutInformation.Amount.Add(award);
            if (transferAmount > 0)
            {
                State.TokenContract.Transfer.Send(new TransferInput
                {
                    Symbol = BingoGameContractConstants.CardSymbol,
                    Amount = transferAmount,
                    To = Context.Sender,
                    Memo = "Thx for playing my game."
                });
            }

            boutInformation.Award = award;
            boutInformation.IsComplete = true;
            State.PlayerInformation[Context.Sender] = playerInformation;

            return new BoolValue {Value = true};
        }

        public override SInt64Value GetAward(Hash input) //传入hash类型的ID，得到奖金数额
        {
            var playerInformation = State.PlayerInformation[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");

            var boutInformation = playerInformation.Bouts.FirstOrDefault(i => i.PlayId == input);

            return boutInformation == null
                ? new SInt64Value {Value = 0}
                : new SInt64Value {Value = boutInformation.Award};
        }

        public override Empty Quit(Empty input) // 退出
        {
            var playerInformation = State.PlayerInformation[Context.Sender]; // 按消息发送方（操作的人的地址）的地址查询
            Assert(playerInformation != null, "Not registered."); //若查不到玩家信息
            State.PlayerInformation[Context.Sender] = new PlayerInformation();

            var balance = State.TokenContract.GetBalance.Call(new GetBalanceInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Owner = Context.Sender
            }).Balance;
            if (balance > BingoGameContractConstants.InitialCards) // 比最初的卡多
            {
                State.TokenContract.Transfer.Send(new TransferInput //向tokencontract的transfer里send一个转账输入
                {
                    Symbol = Context.Variables.NativeSymbol,
                    To = Context.Sender,
                    Amount = balance - BingoGameContractConstants.InitialCards,
                    Memo = "Give elf tokens back."
                });
            }

            State.TokenContract.TransferFrom.Send(new TransferFromInput // //向tokencontract的transferfrom里send一个转账输入
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                From = Context.Sender, // 多一个from属性
                To = Context.Self,
                Amount = balance,
                Memo = "Return cards back."
            });
            return new Empty();
        }

        public override PlayerInformation GetPlayerInformation(Address input) // 根据地址查玩家信息
        {
            return State.PlayerInformation[input] ?? new PlayerInformation();
        }

        /// <summary>
        /// 100%: 0...15, 240...256
        /// 70%: 16...47, 208...239
        /// 40%: 48...95, 160...207
        /// 10%: 96...159
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        private int GetKindFromHash(Hash hash) // 从hash大小判断计算种类
        {
            var sum = SumHash(hash);
            if (sum <= 15 || sum >= 240)
                return 4;

            if (sum <= 47 || sum >= 208)
                return 3;

            if (sum <= 95 || sum >= 160)
                return 2;

            return 1;
        }

        private long CalculateAward(long amount, int kind) //计算：第一个参数乘以0.1，0.4，0.7，1
        {
            switch (kind)
            {
                case 1:
                    return amount.Div(10);
                case 2:
                    return amount.Mul(4).Div(10);
                case 3:
                    return amount.Mul(7).Div(10);
                case 4:
                    return amount;
                default:
                    return 0;
            }
        }

        private int SumHash(Hash hash) // 把hash转成的byte数组元素求和
        {
            var bitArray = new BitArray(hash.Value.ToByteArray());
            var value = 0;
            for (var i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                    value += i;
            }

            return value;
        }

        private bool ConvertHashToBool(Hash hash) //hash串求和后为偶
        {
            return SumHash(hash) % 2 == 0;
        }
        
        
        public override Empty TransferFrom(TransferFromInput input) //转账操作
        {
            AssertValidSymbolAndAmount(input.Symbol, input.Amount); // 检查symbol合法，账户正常
            // First check allowance.
            var allowance = State.Allowances[input.From][Context.Sender][input.Symbol];
            if (allowance < input.Amount) // 
            {
                if (IsInWhiteList(new IsInWhiteListInput {Symbol = input.Symbol, Address = Context.Sender}).Value ||
                    IsContributingProfits(input))
                {
                    DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
                    return new Empty();
                }

                Assert(false,
                    $"[TransferFrom]Insufficient allowance. Token: {input.Symbol}; {allowance}/{input.Amount}.\n" +
                    $"From:{input.From}\tSpender:{Context.Sender}\tTo:{input.To}");
            }

            DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
            State.Allowances[input.From][Context.Sender][input.Symbol] = allowance.Sub(input.Amount);
            return new Empty();
        }
    }
