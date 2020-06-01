using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using AElf.Contracts.MultiToken;

namespace AElf.Contracts.CharityManagement
{
    public class CharityManagementState : ContractState
    {
        public SingletonState<bool> Initialized { get; set; }

        public MappedState<Address, ProjectInformation> PlayerInformation { get; set; }
 
    }
}