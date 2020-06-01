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
        /// Initial reference contracts' address;
        /// create CARD token and issue to contract itself.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        
        
        
        
        public override Empty Initial(Empty input)  // ZDY：初始化
                                                    // ……初始化两个internal合约，用他的方法把本业务的Token信息创建
                                                    // ……
        {
            Assert(!State.Initialized.Value, "Already initialized.");  // 检查数据库一个SingleMap属性应该，表示未初始化
            //设置数据库里两个internal（两个要用的合约）的地址
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.ConsensusContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);
            // 用其中一个internal合约（TokenContract）的方法，创建和发行属于本合约的Token,记录到
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
            State.Initialized.Value = true;  // 修改数据库SingleMap属性应该，表示已经初始化 
            return new Empty();
        }

        /// <summary>
        /// 免费给用户一定数额的Token
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override Empty Register(Empty input)  // ZDY:玩家注册
                                                     // ……用户地址记录到数据库，为其分配第一笔Token
                                                     // ……
        
        {
            Assert(State.PlayerInformation[Context.Sender] == null, $"User {Context.Sender} already registered.");  // 检查数据库一个Map，应该没有对当前用户（交易发起者）的地址记录，表示该用户未注册
            //创建一个玩家（用户）信息并记录到数据库
            var information = new PlayerInformation
            {
                // The value of seed will influence user's game result in some aspects.
                Seed = Context.TransactionId
            };
            State.PlayerInformation[Context.Sender] = information;
            // 按照Constance设置，给这个玩家（用户）一定数额的Token来使用
            State.TokenContract.Transfer.Send(new TransferInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = BingoGameContractConstants.InitialCards,
                To = Context.Sender,
                Memo = "Initial Bingo Cards for player."
            });

            return new Empty();
        }

        public override Empty Deposit(SInt64Value input)  // ZDY：玩家充值
        {
            // 从库中拿到玩家（用户，交易发起者）的信息
            var playerInformation = State.PlayerInformation[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");  // 检查用户已注册
            Assert(input.Value > 0, "At least you should buy 1 CARD.");  //充值金额大于0
            // 每次交易需要用户付的费用
            var elfAmount = input.Value.Mul(1_0000_0000);
            // 从玩家（用户，交易发起者)地址提钱到当前正在执行（被调用）的合约的地址，即"付费"，这个费是合约要接受的，不是自定义类型的Token
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                Symbol = Context.Variables.NativeSymbol,
                Amount = elfAmount,
                From = Context.Sender,
                To = Context.Self,
                Memo = "Thanks for recharging:)"
            });
            // 本合约执行给玩家充值一定的Token（用于本业务，之前自定义的Token）
            State.TokenContract.Transfer.Send(new TransferInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = input.Value,
                To = Context.Sender,
                Memo = "Now you are stronger:)"
            });

            return new Empty();
        }

        public override SInt64Value Play(SInt64Value input)  // ZDY：玩家进行下注
                                                             // 把这次下注信息记录到当前回合信息
                                                             // ……
        {
            Assert(input.Value > 1, "Invalid bet amount.");  //  检查玩（下注）的数额大于1
            var playerInformation = State.PlayerInformation[Context.Sender];  //  读玩家信息并检查已经注册
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");

            Context.LogDebug(() => $"Playing with amount {input.Value}");  // 记录到游戏日志
            // 把这笔下注数额的Token打给当前正在执行（被调用）的合约的地址（就是本合约）
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                From = Context.Sender,
                To = Context.Self,
                Amount = input.Value,
                Symbol = BingoGameContractConstants.CardSymbol,
                Memo = "Enjoy!"
            });
            
            // 获取当前回合信息？？？？？
            var currentRound = State.ConsensusContract.GetCurrentRoundInformation.Call(new Empty());
            // 玩家信息的"下注记录"这个repeated BoutInformation属性加入新一轮的BoutInformation下注信息
            playerInformation.Bouts.Add(new BoutInformation
            {
                // Record current round number.
                PlayRoundNumber = currentRound.RoundNumber,  // 当前回合号
                Amount = input.Value,  // 玩了多少钱
                PlayId = Context.TransactionId  // 交易id，每笔交易有一个id ！！！
            });
            // 更新数据库的玩家信息
            State.PlayerInformation[Context.Sender] = playerInformation;
            // 返回本回合等待时间
            return new SInt64Value {Value = CalculateWaitingMilliseconds(currentRound)};
        }

        private long CalculateWaitingMilliseconds(Round round)  // Play函数专用，计算该回合游戏时间
        {
            // 生成区块的时间
            var extraBlockProducerExpectedTime = round.RealTimeMinersInformation.Values.OrderByDescending(i => i.Order)
                .First().ExpectedMiningTime.AddMilliseconds(4000);
            // 计算随机数的时间
            var expectedTimeOfGettingRandomNumber = extraBlockProducerExpectedTime.AddMilliseconds(8000);
            return (expectedTimeOfGettingRandomNumber - Context.CurrentBlockTime).Milliseconds();
        }

        public override BoolValue Bingo(Hash input)  // ZDY：一个回合，Bingo游戏逻辑
        {
            Context.LogDebug(() => $"Getting game result of play id: {input.ToHex()}");
            //读取玩家信息，检查已注册，检查
            var playerInformation = State.PlayerInformation[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");
            Assert(playerInformation.Bouts.Count > 0, $"User {Context.Sender} seems never join this game.");
            // 构建新的回合信息，检查hash是空
            var boutInformation = input == Hash.Empty
                ? playerInformation.Bouts.First(i => i.BingoRoundNumber == 0)
                : playerInformation.Bouts.FirstOrDefault(i => i.PlayId == input);
            // 检查本回合信息不为空，检查本游戏未结束
            Assert(boutInformation != null, "Bouts not found.");
            Assert(!boutInformation.IsComplete, "Bout already finished.");
            
            // 构建新一轮回合信息
            var targetRound = State.ConsensusContract.GetRoundInformation.Call(new Int64Value
            {
                Value = boutInformation.PlayRoundNumber.Add(1)
            });
            // 检查
            Assert(targetRound != null, "Still preparing your game result, please wait for a while :)");

            var randomHash = targetRound.RealTimeMinersInformation.Values.First(i => i.PreviousInValue != null).PreviousInValue;
            // 赢没赢
            var isWin = ConvertHashToBool(randomHash);
            // 使用随机哈希数和用户种子计算出一个新的hash
            var usefulHash = Hash.FromTwoHashes(randomHash, playerInformation.Seed);
            // 根据这个hash的稀有性种类，与玩家用户的下注数额计算可能赢得的奖金/赔掉的钱
            var award = CalculateAward(boutInformation.Amount, GetKindFromHash(usefulHash));
            // 根据赢没赢判断这笔钱是赚还是赔
            award = isWin ? award : -award;
            // 把这笔赢得的钱（游戏币）加和底注相加
            var transferAmount = boutInformation.Amount.Add(award);
            // 相加之后（底注变多/没赔光）的结果作为新的底注打给玩家
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

            boutInformation.Award = award;  //记录本回合的利润（多赚的游戏币）
            boutInformation.IsComplete = true;   // 本回合游戏结束
            State.PlayerInformation[Context.Sender] = playerInformation;  // 更新玩家信息

            return new BoolValue {Value = true};  //告诉其他方法本回合Bingo完成
        }

        public override SInt64Value GetAward(Hash input)  // ZDY：获几等奖
                                                          //  ……

        {
            var playerInformation = State.PlayerInformation[Context.Sender];  // 读取玩家信息，检查已经注册
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");
            // 查找玩家使用输入的hash参数玩游戏的回合信息
            var boutInformation = playerInformation.Bouts.FirstOrDefault(i => i.PlayId == input);
            // 如果有的话，返回奖金数额
            return boutInformation == null
                ? new SInt64Value {Value = 0}
                : new SInt64Value {Value = boutInformation.Award};
        }

        public override Empty Quit(Empty input)  // ZDY：退出
                                                 // 如果赚了，结算成aelf返还奖励
                                                 // 扣除本次游戏玩家所有游戏币
        {
            var playerInformation = State.PlayerInformation[Context.Sender];  // 读取玩家信息，检查已经注册
            Assert(playerInformation != null, "Not registered.");
            State.PlayerInformation[Context.Sender] = new PlayerInformation();
            
            //读取玩家账户的游戏自定义Token的数额
            var balance = State.TokenContract.GetBalance.Call(new GetBalanceInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Owner = Context.Sender
            }).Balance;
            //如果游戏专用Token数额大于初始分配值,给玩家（用户，交易发起者）返还交易费用（是把多赚的游戏币换成了elf tokens这个token）
            if (balance > BingoGameContractConstants.InitialCards)
            {
                State.TokenContract.Transfer.Send(new TransferInput
                {
                    Symbol = Context.Variables.NativeSymbol,
                    To = Context.Sender,
                    Amount = balance - BingoGameContractConstants.InitialCards,
                    Memo = "Give elf tokens back."
                });
            }

            State.TokenContract.TransferFrom.Send(new TransferFromInput  // 扣除玩家所有游戏币
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                From = Context.Sender,
                To = Context.Self,
                Amount = balance,
                Memo = "Return cards back."
            });
            return new Empty();
        }

        public override PlayerInformation GetPlayerInformation(Address input)  // ZDY：获取玩家信息
                                                                               //
        {
            return State.PlayerInformation[input] ?? new PlayerInformation();
        }
        
        
        
        /// 以下为private方法，合约内部使用：
        /// 
        /// <summary>
        /// 100%: 0...15, 240...256
        /// 70%: 16...47, 208...239
        /// 40%: 48...95, 160...207
        /// 10%: 96...159
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        
        
        private int GetKindFromHash(Hash hash)  // hash的特点（1的分布）即稀有性，设定种类
        {
            var sum = SumHash(hash);
            if (sum <= 15 || sum >= 240)  // 1位的下标分布在集中在某一侧
                return 4;

            if (sum <= 47 || sum >= 208)
                return 3;

            if (sum <= 95 || sum >= 160)
                return 2;

            return 1;  //1位的下标分布两边比较均匀
        }

        private long CalculateAward(long amount, int kind)  // 计算奖励
        {
            switch (kind)
            {
                case 1:
                    return amount.Div(10);  // 底注*0.1
                case 2:
                    return amount.Mul(4).Div(10);  // 底注*0.4
                case 3:
                    return amount.Mul(7).Div(10);  // 底注*0.7
                case 4:
                    return amount;  // 底注*1
                default:
                    return 0;
            }
        }

        private int SumHash(Hash hash)  // 给hash求和
        {
            var bitArray = new BitArray(hash.Value.ToByteArray());  // hash转二进制字串（0或1的串）
            var value = 0;  
            for (var i = 0; i < bitArray.Count; i++)  // 遍历，查找是1的位
            {
                if (bitArray[i])
                    value += i; // 把这些位的下标求和
            }

            return value;  // 结果是个int
        }

        private bool ConvertHashToBool(Hash hash)  // hash转bool，表示赢没赢
        {
            return SumHash(hash) % 2 == 0;  // 哈希和的奇偶性作为bool值
        }
    }
}