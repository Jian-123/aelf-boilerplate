// IEnumerable需要这3个
using System.Collections.Generic;
using System.Linq; // Single需要这个
using AElf.OS.Node.Application;
using AElf.Types;
// 
using Acs0;

namespace AElf.Blockchains.MainChain // 注意这有s，否则_code会读不到
{
    public partial class GenesisSmartContractDtoProvider
    {
        public IEnumerable<GenesisSmartContractDto> GetGenesisSmartContractDtosForCharityTest()
        {
            var l = new List<GenesisSmartContractDto>(); // 智能合约列表，添加
            l.AddGenesisSmartContract(
                _codes.Single(kv => kv.Key.Contains("CharityTest")).Value,
                Hash.FromString("AElf.ContractNames.CharityTest"),
                GenerateCharityTestInitializationCallList()); // 下面实现
            return l;
        }
        
        private SystemContractDeploymentInput.Types.SystemTransactionMethodCallList GenerateCharityTestInitializationCallList() //需要Acs0;
        {
            var charityTestContractMethodCallList = new SystemContractDeploymentInput.Types.SystemTransactionMethodCallList();

            return charityTestContractMethodCallList;
        }
    }
}