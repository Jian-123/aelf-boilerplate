using AElf.Sdk.CSharp.State;
using AElf.Types; // Address类所在

namespace AElf.Contracts.CharityTest
{
    public class CharityTestState: ContractState
    {
        public SingletonState<Project> Projects { set; get; } // 必须public
        public MappedState<int,Testmessage1> Tests1 { get; set; } // 第一项是Key，后面是Value
        public MappedState<int,string> Tests2 { get; set; } // 第一项是Key，后面是Value
 
    }
}