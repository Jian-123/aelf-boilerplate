using AElf.Contracts.TestKit;
using AElf.Kernel.SmartContract;
using AElf.Kernel.SmartContract.Infrastructure;
using AElf.Runtime.CSharp; //UnitTestCSharpSmartContractRunner需要
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Modularity;

namespace AElf.Contracts.CharityTest
{
    [DependsOn(typeof(ContractTestModule))]
    public class CharityTestTestModule : ContractTestModule //代码模块化管理单元
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<ContractOptions>(o => o.ContractDeploymentAuthorityRequired = false);
            context.Services.AddSingleton<ISmartContractRunner, UnitTestCSharpSmartContractRunner>(provider =>
            {
                var option = provider.GetService<IOptions<RunnerOptions>>();
                return new UnitTestCSharpSmartContractRunner(
                    option.Value.SdkDir);
            });
        }
    }
}