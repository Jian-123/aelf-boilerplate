using System.Collections.Generic;
using System.Linq;
using AElf.Boilerplate.MainChain;
using AElf.Contracts.Deployer;
using AElf.Kernel.Account.Application;
using AElf.Kernel.Consensus.AEDPoS;
using AElf.OS.Node.Application;
using AElf.Types;
using Microsoft.Extensions.Options;

namespace AElf.Blockchains.MainChain
{
    public partial class GenesisSmartContractDtoProvider : IGenesisSmartContractDtoProvider
    {
        private readonly IAccountService _accountService;
        private readonly IReadOnlyDictionary<string, byte[]> _codes;
        
        private readonly ConsensusOptions _consensusOptions;
        private readonly EconomicOptions _economicOptions;

        public GenesisSmartContractDtoProvider(IOptionsSnapshot<ConsensusOptions> dposOptions,
            IOptionsSnapshot<EconomicOptions> economicOptions, ContractsDeployer contractsDeployer,
            IAccountService accountService)
        {
            _accountService = accountService;
            _consensusOptions = dposOptions.Value;
            _economicOptions = economicOptions.Value;
            
            _codes = contractsDeployer.GetContractCodes<GenesisSmartContractDtoProvider>();
        }

        public IEnumerable<GenesisSmartContractDto> GetGenesisSmartContractDtos(Address zeroContractAddress)
        {
            // The order matters !!!
            return new[]
            {
                GetGenesisSmartContractDtosForVote(zeroContractAddress),
                GetGenesisSmartContractDtosForProfit(zeroContractAddress),
                GetGenesisSmartContractDtosForElection(zeroContractAddress),
                GetGenesisSmartContractDtosForToken(zeroContractAddress),
                GetGenesisSmartContractDtosForConsensus(zeroContractAddress),
                GetGenesisSmartContractDtosForHelloWorld(zeroContractAddress),
                GetGenesisSmartContractDtosForBingoGame(zeroContractAddress)
            }.SelectMany(x => x);
        }
    }
}