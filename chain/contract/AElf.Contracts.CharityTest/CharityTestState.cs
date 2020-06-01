using AElf.Sdk.CSharp.State;
using AElf.Types; // Address类所在

namespace AElf.Contracts.CharityTest
{
    public partial class CharityTestState: ContractState
    {
 
        
        
        public SingletonState<bool> Initialized { get; set; }  // 是否初始化，未必有用
        public SingletonState<int> NumberOfProjects { get; set; }  // 系统上的项目总数
        
        public MappedState<Address, UserInformation> UserInformations{ get; set; }
        public MappedState<int, ProjectInformation> ProjectInformations{ get; set; }

    }
}