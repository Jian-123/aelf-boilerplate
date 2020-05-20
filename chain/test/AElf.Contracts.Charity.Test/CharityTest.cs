using System;
using System.Threading.Tasks;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.Charity
{
    public class CharityTest
    {
        // [Fact]
        // public void Test1()
        // {
        // }
        [Fact]
        public async Task HelloCall_ReturnsHelloWorldMessage()
        {
            var txResult = await CharityContainer.CharityStub.Hello.SendAsync(new Empty());
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var text = new HelloReturn();
            text.MergeFrom(txResult.TransactionResult.ReturnValue);
            text.Value.ShouldBe("Hello World!");
        }
    }
}

