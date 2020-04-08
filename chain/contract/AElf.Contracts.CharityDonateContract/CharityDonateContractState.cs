using AElf.Sdk.CSharp.State;
using AElf.Types;

namespace AElf.Contracts.CharityDonateContract
{
    public class CharityDonateContractState : ContractState
    {
        // public SingletonState<Address> PID { get; set; } 
        // public SingletonState<Donat> {}
        // public SingletonState<decimal> Account { get; set; }
        // public MappedState<,>{}
        public MappedState<Address, User> Users { get; set; }
        
        public MappedState<Address, ProjectInformation> ProjectInformations { get; set; }

    }
}