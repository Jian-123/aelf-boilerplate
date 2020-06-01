using AElf.Contracts.Consensus.AEDPoS;
using AElf.Contracts.MultiToken;

namespace AElf.Contracts.CharityTest
{
    public partial class CharityTestState
    {
        internal TokenContractContainer.TokenContractReferenceState TokenContract { get; set; }
        internal AEDPoSContractContainer.AEDPoSContractReferenceState ConsensusContract { get; set; }
    }
}