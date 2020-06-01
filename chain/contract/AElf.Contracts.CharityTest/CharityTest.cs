using System;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using AElf.Sdk.CSharp;
using AElf.Contracts.MultiToken;

using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Linq;
using AElf.Contracts.Consensus.AEDPoS;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Google.Protobuf.Collections;


namespace AElf.Contracts.CharityTest
{
    public class CharityTest : CharityTestContainer.CharityTestBase
    {
        
        public override Empty Initial(Empty input)  //ZDY：创币发行
        {
           
           Assert(!State.Initialized.Value, "Already initialized.");  // 检查数据库一个SingleMap属性应该，表示未初始化
           //设置数据库里两个internal（两个要用的合约）的地址
           State.TokenContract.Value =
               Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
           State.ConsensusContract.Value =
               Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);
           // 用其中一个internal合约（TokenContract）的方法，创建专用的虚拟币(Token)并发行给本合约使用,记录上链
           State.TokenContract.Create.Send(new CreateInput
           {
               Symbol = CharityTestConstants.CardSymbol,
               TokenName = "Kindness",
               Decimals = 0,
               Issuer = Context.Self,
               IsBurnable = true,
               TotalSupply = long.MaxValue,
               LockWhiteList = {Context.Self}
           });
           State.TokenContract.Issue.Send(new IssueInput
           {
               Symbol = CharityTestConstants.CardSymbol,
               Amount = long.MaxValue,
               To = Context.Self,
               Memo = "All to issuer."
           });
           State.Initialized.Value = true;  // 修改数据库SingleMap属性应该，表示已经初始化 
           State.NumberOfProjects.Value = 0;
           return new Empty();
        }
       
        public override Empty Register(Empty input)  // ZDY：为用户注册钱包
        {

            Assert(State.UserInformations[Context.Sender] == null, $"User {Context.Sender} already registered.");  // 检查数据库一个Map，应该没有对当前用户（交易发起者）的地址记录，表示该用户未注册
            //创建一个（用户）信息并记录到数据库
            var information = new UserInformation
            {
                // 可以删
                Seed = Context.TransactionId
            };
            State.UserInformations[Context.Sender] = information;
            // 按照Constance设置，给这个用户一定数额的Token来使用
            State.TokenContract.Transfer.Send(new TransferInput
            {
                Symbol = CharityTestConstants.CardSymbol,
                Amount = CharityTestConstants.InitialCards,
                To = Context.Sender,
                Memo = "Initial  some Kindness for user."
            });
            return new Empty(); 
        }
        
        public override Empty Deposit(SInt64Value input) // ZDY：购买虚拟代币
        {
            var playerInformation = State.UserInformations[Context.Sender];
            Assert(playerInformation != null, $"User {Context.Sender} not registered before.");  // 检查用户已注册
            Assert(input.Value > 0, "At least you should buy 1 CARD.");  //充值金额大于0
            // 每次交易需要用户付的费用
            var elfAmount = input.Value.Mul(1_0000_0000);
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                Symbol = Context.Variables.NativeSymbol,
                Amount = elfAmount,
                From = Context.Sender,
                To = Context.Self,
                Memo = "Payment has been."
            });
            State.TokenContract.Transfer.Send(new TransferInput
            {
                Symbol = CharityTestConstants.CardSymbol,
                Amount = input.Value,
                To = Context.Sender,
                Memo = "The token has arrived."
            });

            return new Empty();
        }
        
        
        
        
        public override Int32Value SetProject(NewProject input)  // ZDY：发布慈善项目
        {
            
            var numberOfProjectsNow = ++State.NumberOfProjects.Value;
            var information = new ProjectInformation
            {
                
                Id = numberOfProjectsNow, // 使用Add后自己会变吗？？？
                Title = input.Title,
                Description = input.Description,
                Creator = Context.Sender,
                // DonationList = new RepeatedField<Donation>()
            };
            // todo: key的选取;
            // 这里能不能用的交易hash作为项目的key？？？
            State.ProjectInformations[numberOfProjectsNow]= information;
            return new Int32Value {Value = numberOfProjectsNow};
        }

        public override ProjectInformation GetProject(Int32Value input)
        {
            // ZDY：这里存取方式不一致 Hash.FromString(input.ToString())
            return State.ProjectInformations[input.Value] ?? new ProjectInformation();
        }

        // public override ProjectInformation GetProject(Hash input)  
        // {
        //     return State.ProjectInformations[input] ?? new ProjectInformation();
        // }

        public override Empty Donate(NewDonation input)
        {
            Assert(input!= null,"Not valid DonationInput.");
            Assert(input != null && input.Amount > 0, "At least you should donate 1 Kindness.");
            //读取用户账户的的数额
            var balance = State.TokenContract.GetBalance.Call(new GetBalanceInput
            {
                Symbol = CharityTestConstants.CardSymbol,
                Owner = Context.Sender
            }).Balance;
            Assert(input != null && balance >= input.Amount, "Your Kindness is not enough.");

            if (input != null)
            {
                
                State.TokenContract.TransferFrom.Send(new TransferFromInput // 扣除用户的虚拟币
                {
                    Symbol = CharityTestConstants.CardSymbol,
                    From = Context.Sender,
                    To = State.ProjectInformations[input.ToID].Creator,
                    Amount = input.Amount,
                    Memo = "Thanks for your Kindness."
                });
                //将转账记录加到该项目下
                
                var donation = new Donation
                {
                    // Record current round number.
                    Id = Context.TransactionId,
                    From = Context.Sender,
                    To = State.ProjectInformations[input.ToID].Creator,
                    Amount = input.Amount,
                    Memo = input.Memo
                };

                // 更新项目的捐赠列表
                State.ProjectInformations[input.ToID].DonationList.Add(donation);

               
            }

            return new Empty();
        }


        
        // 测试前后端交互
        public override Int32Value TestAdd(AddInput input)
        {
            var sum = input.A + input.B;
            var returnSumValue = new Int32Value(){Value =sum};
            return returnSumValue;
        }
    }
}