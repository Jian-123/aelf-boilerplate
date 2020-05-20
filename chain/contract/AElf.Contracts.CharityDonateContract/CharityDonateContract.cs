using System;
using System.Linq;
using System.Text;
using System.Threading;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;

using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;

namespace AElf.Contracts.CharityDonateContract
{
    public class CharityDonateContract : CharityDonateContractContainer.CharityDonateContractBase
    {
        public override Empty Donate(Donation input)
        {
            Assert(Accessible(input),"Cannot donate.");
            State.ProjectInformations[input.To].Amount += input.Money;
            State.Users[input.From].Account -= input.Money;
            State.Users[input.From].DonationIds.Add(input.Id); //
            
            return null; 
        }

        public override ProjectInformation GetProjectInformation(Address input) // 展示某项目的具体信息
        {
            var projectInformation = State.ProjectInformations[input];
            return projectInformation;
        }
        
        
        public override Empty SetUser(User input) // 存用户
        {
            State.Users.Set(input.Id,input);
            return null;
        }

        public override Empty SetProject(ProjectInformation  input) //存项目
        {
            State.ProjectInformations.Set(input.Id, input);
            return null;
        }

        public override RepeatedField<Hash> CheckMyDonate(Address input) // 查看自己的捐赠记录
        {
           
            var d = State.Users[input].DonationIds;
            var s = new DonationIds(); 
            return d;
        }

        public bool Accessible(Donation input) // 判断捐款可进行
        {
            
            var toThisProject = State.ProjectInformations[input.To];
            var fromThisUser = State.Users[input.From];
            var endTime = toThisProject.EndTime;
            // var submitTime = input.SubmitTime;
            var currentTime = Context.CurrentBlockTime;
            // var c = Timestamp.FromDateTime(DateTime.Now); 被禁
            return (input.Money <= fromThisUser.Account) && (currentTime < endTime);
            
        }
        
       
        
        
        
        
        // 以下处理转账transfer.proto的实现
        
        public override Empty Create(CreateInput input) //生成token
        {
            AssertValidCreateInput(input);
            RegisterTokenInfo(new TokenInfo
            {
                Symbol = input.Symbol,
                TokenName = input.TokenName,
                TotalSupply = input.TotalSupply,
                Decimals = input.Decimals,
                Issuer = input.Issuer,
                IsBurnable = input.IsBurnable,
                IsProfitable = input.IsProfitable,
                IssueChainId = input.IssueChainId == 0 ? Context.ChainId : input.IssueChainId
            });
            if (string.IsNullOrEmpty(State.NativeTokenSymbol.Value))
            {
                Assert(Context.Variables.NativeSymbol == input.Symbol, "Invalid input.");
                State.NativeTokenSymbol.Value = input.Symbol; // NativeTokenSymbol入库
            }

            var systemContractAddresses = Context.GetSystemContractNameToAddressMapping().Select(m => m.Value);
            var isSystemContractAddress = input.LockWhiteList.All(l => systemContractAddresses.Contains(l));
            Assert(isSystemContractAddress, "Addresses in lock white list should be system contract addresses");
            foreach (var address in input.LockWhiteList)
            {
                State.LockWhiteLists[input.Symbol][address] = true;
            }

            Context.LogDebug(() => $"Token created: {input.Symbol}"); 
            

            return new Empty();
        }

        public override Empty Issue(IssueInput input) //发行token
        {
            Assert(input.To != null, "To address not filled."); //发到的地址是否存在
            AssertValidMemo(input.Memo); //验证memo合法
            var tokenInfo = AssertValidToken(input.Symbol, input.Amount); //验证提取token信息
            Assert(tokenInfo.IssueChainId == Context.ChainId, "Unable to issue token with wrong chainId."); 
            Assert(tokenInfo.Issuer == Context.Sender || Context.Sender == Context.GetZeroSmartContractAddress(),
                $"Sender is not allowed to issue token {input.Symbol}."); // 这个正在执行的事务的发送方(签名者)的地址应为token的发行方或零合约的地址
            tokenInfo.Supply = tokenInfo.Supply.Add(input.Amount);
            Assert(tokenInfo.Supply.Add(tokenInfo.Burned) <= tokenInfo.TotalSupply, "Total supply exceeded"); //tokeninfo的supply不超额
            State.TokenInfos[input.Symbol] = tokenInfo; // 
            ModifyBalance(input.To, input.Symbol, input.Amount); // 发款到该地址
            return new Empty();
        }

        public override Empty Transfer(TransferInput input) //转账
        {
            AssertValidSymbolAndAmount(input.Symbol, input.Amount); 
            DoTransfer(Context.Sender, input.To, input.Symbol, input.Amount, input.Memo);
            return new Empty();
        }

         
        public override Empty TransferFrom(TransferFromInput input)
        {
            AssertValidSymbolAndAmount(input.Symbol, input.Amount);
            // First check allowance.
            var allowance = State.Allowances[input.From][Context.Sender][input.Symbol];
            if (allowance < input.Amount)
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


        

        private void AssertValidCreateInput(CreateInput input) //验证Create输入的合法性
        {
            var isValid = input.TokenName.Length <= TokenContractConstants.TokenNameLength
                          && input.Symbol.Length > 0
                          && input.Symbol.Length <= TokenContractConstants.SymbolMaxLength
                          && input.Decimals >= 0
                          && input.Decimals <= TokenContractConstants.MaxDecimals; // 《=规定的常量
            Assert(isValid, "Invalid input.");
        }

        
        private void RegisterTokenInfo(TokenInfo tokenInfo) //登记token信息
        {
            var existing = State.TokenInfos[tokenInfo.Symbol]; //从库中读取已有的tokeninfo
            Assert(existing == null || existing.Equals(new TokenInfo()), "Token already exists."); //
            Assert(!string.IsNullOrEmpty(tokenInfo.Symbol) && tokenInfo.Symbol.All(IsValidSymbolChar),
                "Invalid symbol."); //token symbol不为空
            Assert(!string.IsNullOrEmpty(tokenInfo.TokenName), $"Invalid token name. {tokenInfo.Symbol}"); //toke名字不为空
            Assert(tokenInfo.TotalSupply > 0, "Invalid total supply."); //total supply > 0  ?
            Assert(tokenInfo.Issuer != null, "Invalid issuer address."); //发行地址不为空
            State.TokenInfos[tokenInfo.Symbol] = tokenInfo;
        }
        
        private TokenInfo AssertValidToken(string symbol, long amount) // 验证token合法性
        {
            AssertValidSymbolAndAmount(symbol, amount);
            var tokenInfo = State.TokenInfos[symbol];
            Assert(tokenInfo != null && !string.IsNullOrEmpty(tokenInfo.Symbol), $"Token is not found. {symbol}");
            return tokenInfo;
        }
        
        private void AssertValidSymbolAndAmount(string symbol, long amount)
        {
            Assert(!string.IsNullOrEmpty(symbol) && symbol.All(IsValidSymbolChar), //symbol不为空且都是合法字符
                "Invalid symbol.");
            Assert(amount > 0, "Invalid amount."); //amount为正
        }

        private void AssertValidMemo(string memo) // 验证memo备忘录的合法性
        {
            Assert(memo == null || Encoding.UTF8.GetByteCount(memo) <= TokenContractConstants.MemoMaxLength,
                "Invalid memo size."); //字节数不超过一个常量
        }

        
        
        private void DoTransfer(Address from, Address to, string symbol, long amount, string memo = null) // 处理转账操作
        {
            Assert(from != to, "Can't do transfer to sender itself.");
            AssertValidMemo(memo); 
            ModifyBalance(from, symbol, -amount);
            ModifyBalance(to, symbol, amount);
            Context.Fire(new Transferred //触发转账？
            {
                From = from,
                To = to,
                Symbol = symbol,
                Amount = amount,
                Memo = memo ?? string.Empty
            });
        }

        private void ModifyBalance(Address address, string symbol, long addAmount) //修改支付某方的amount 
        {
            var before = GetBalance(address, symbol); ////读取Balance状态，获得支付前的amount
            if (addAmount < 0 && before < -addAmount) // 为支出方，且余额不足
            {
                Assert(false, $"Insufficient balance. {symbol}: {before} / {-addAmount}"); //
            }
            var target = before.Add(addAmount); // 支付后的的amount
            State.Balances[address][symbol] = target; // 保存到数据库
        }
        
        private long GetBalance(Address address, string symbol) //从数据库Balances字典里获取amount
        {
            return State.Balances[address][symbol];
        }



        
        
        
    }
}