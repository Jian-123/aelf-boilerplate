using AElf.Sdk.CSharp.State;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using AElf.Contracts.MultiToken;
namespace AElf.Contracts.Charity
{
    public class CharityState : ContractState
    {
        private MappedState<string, TokensInfo> TokenInfos { get; set; } //（symbol，tokeninfo） 
        
        public MappedState<Address, string, long> Balances { get; set; } //每个钱包的symbol余额（address，symbol，amount）
        // public MappedState<Address, Project> Projects { get; set; } // 每个项目拥有一个地址
        public MappedState<Address, User> Users { get; set; } // 每个用户有一个地址
        public MappedState<Int32Value, Project> Projects { get; set; } // 每个项目拥有一个id
        
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
    }
}