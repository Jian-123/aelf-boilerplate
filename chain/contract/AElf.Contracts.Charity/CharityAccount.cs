using System.Text;
using AElf.CSharp.Core;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Charity
{ 
    public partial class CharityAccount : CharityContainer.CharityBase // Container名字来自service，Base类在.c.cs
    {
        public override Empty Initial(Empty input)
        {
            var s = new TokensInfo(); 
            return new Empty();
        }
        
        public Empty TransferFrom (TransferFromsInput input) //转账操作
        {
            AssertValidSymbolAndAmount(input.Symbol, input.Amount); // 检查symbol合法，账户正常
            //
            DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
            return new Empty();
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
       
        
        private void AssertValidSymbolAndAmount(string symbol, long amount)
        {
            Assert(!string.IsNullOrEmpty(symbol), //symbol不为空
                "Invalid symbol.");
            Assert(amount > 0, "Invalid amount."); //amount为正
        }
        
        private void AssertValidMemo(string memo)
        {
            Assert(memo == null || Encoding.UTF8.GetByteCount(memo) <= CharityConstants.MemoMaxLength,
                "Invalid memo size.");
        }

    }
    
}