using System.IO;
using AElf.Contracts.TestKit;
using AElf.Kernel.SmartContract;
using AElf.Runtime.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace OrderContract.Test
{
    [DependsOn(typeof(ContractTestModule))]
    public class OrderContractTestModule : ContractTestModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<ContractOptions>(o => o.ContractDeploymentAuthorityRequired = false);
        }
    }
}