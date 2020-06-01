using Google.Protobuf.WellKnownTypes;
using AElf.Contracts.MultiToken;

using AElf.Types;

namespace AElf.Contracts.Charity
{
    public partial class Charity : CharityContainer.CharityBase // Container名字来自service，Base类在.c.cs
    {
        // public override Empty Initial(Empty input)
        // {
        //     var a = new TokensInfo();
        //     return new Empty();
        // }

        public int totalProjects =  0;

        public override Empty Donate(Donation input)
        {
            var transferFromInput = new TransferFromsInput()
            {
                From = input.Donor,
                To = Context.Self,
                Amount = input.Amount,
                Symbol = CharityConstants.DonateSymbol,
                Memo = $"{input.Amount} donation from {input.Donor}"
            };
            var charityAccount = new CharityAccount() ;
            charityAccount.TransferFrom(transferFromInput);
            return new Empty();
        }

        public override Empty SetProject(Project input) // 增加项目
        {
            var proID = new Int32Value
            {
                Value = totalProjects
            };
            totalProjects++; 
            State.Projects.Set(proID,input);
            return new Empty();
        } 
        
        public override  Project GetProject(Int32Value input)  // 查询项目
        {
            return State.Projects[input] ?? new Project();
        }
        
        
        
        public override Donations GetDonations(Int32Value input)  // 获取项目的捐赠信息
        {
            var donations = State.Projects[input].DonationList;
            return donations;
            
        }
        
        
        
        
        
        
        
        
        
        
        // public override Project GetProjects (Address input)
        // {
        //     var project = State.Projects[input];
        //     var donations = new Projects;
        //     return project;
        // }
    }
    
}