using System.IO;
using System.Linq;
using Acs0;
using AElf.Blockchains.BasicBaseChain.ContractNames; //
using AElf.Contracts.TestKit;
using AElf.Cryptography.ECDSA;
using AElf.Kernel;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.Threading;

namespace AElf.Contracts.CharityTest
{
    public class CharityTestTestBase : ContractTestBase<CharityTestTestModule>
    {
        internal CharityTestContainer.CharityTestStub CharityTestStub { get; set; }
        private ACS0Container.ACS0Stub ZeroContractStub { get; set; }

        private Address CharityTestAddress { get; set; }

        protected CharityTestTestBase() //用来初始化测试用例的变量
        {
            InitializeContracts();
        }

        private void InitializeContracts()
        {
            ZeroContractStub = GetZeroContractStub(SampleECKeyPairs.KeyPairs.First());

            CharityTestAddress = AsyncHelper.RunSync(() =>
                ZeroContractStub.DeploySystemSmartContract.SendAsync(
                    new SystemContractDeploymentInput
                    {
                        Category = KernelConstants.DefaultRunnerCategory,
                        Code = ByteString.CopyFrom(File.ReadAllBytes(typeof(CharityTest).Assembly.Location)),
                        Name = ProfitSmartContractAddressNameProvider.Name,
                        TransactionMethodCallList =
                            new SystemContractDeploymentInput.Types.SystemTransactionMethodCallList()
                    })).Output;
            CharityTestStub = GetCharityTestStub(SampleECKeyPairs.KeyPairs.First());
        }

        private ACS0Container.ACS0Stub GetZeroContractStub(ECKeyPair keyPair)
        {
            return GetTester<ACS0Container.ACS0Stub>(ContractZeroAddress, keyPair);
        }

        private CharityTestContainer.CharityTestStub GetCharityTestStub(ECKeyPair keyPair)
        {
            return GetTester<CharityTestContainer.CharityTestStub>(CharityTestAddress, keyPair);
        }
    }
}