using System;

using System.Threading;
using Google.Protobuf.WellKnownTypes;

using AElf.Sdk.CSharp;
using AElf.Types;

namespace AElf.Contracts.CharityDonateContract
{
    public class CharityDonateContract : CharityDonateContractContainer.CharityDonateContractBase
    {
        public override Empty Donate(Donation input)
        {
            Assert(Accessable(input),"Cannot donate.");
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

        public override DonationIds CheckMyDonate(Address input) // 查看自己的捐赠记录
        {
           
            var d = State.Users[input].DonationIds;
            var s = new DonationIds(); 
            return s;
        }

        public bool Accessable(Donation input) // 判断捐款可进行
        {
            var toThisProject = State.ProjectInformations[input.To];
            var fromThisUser = State.Users[input.From];
            var endTime = toThisProject.EndTime;
            var submitTime = input.SubmitTime;
            var c = System.DateTime.Now;
            // var c = Timestamp.FromDateTime(DateTime.Now);
            return (input.Money <= fromThisUser.Account) && (submitTime < endTime);
        }
    }
}