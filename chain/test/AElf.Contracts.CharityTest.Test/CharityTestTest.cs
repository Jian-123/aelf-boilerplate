using System;
using System.Threading.Tasks;
using AElf.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.CharityTest
{
    public class CharityTestTest : CharityTestTestBase
    {
        // [Fact]
        // public void Test1()
        // {
        // }
        // [Fact]
        // public async Task HelloCall_ReturnsHelloWorldMessage()
        // {
        //     var txResult = await CharityTestContainer.CharityTestStub.Hello.SendAsync(new Empty());
        //     txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        //     var text = new HelloReturn();
        //     text.MergeFrom(txResult.TransactionResult.ReturnValue);
        //     text.Value.ShouldBe("Hello World!");
        // }
        //
        [Fact]
        public async Task SetProjectCall_ReturnInt32Value()
        {
             var newproject = new NewProject{ Title = "慈善项目1号", Description = "测试使用"};
             // var a = new CharityTestContainer.CharityTestStub stub; 
            var txResult = await CharityTestContainer.CharityTestStub.SetProject(new NewProject{});
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            
            var text = new SetProject(new NewProject{"a","b"});
            text.MergeFrom(txResult.TransactionResult.ReturnValue);
            text.Value.ShouldBe("Hello World!");
        }
        //
        // [Theory]
        // [InlineData("Ean")]
        // [InlineData("Sam")]
        // public async Task GreetToTests(string name)
        // {
        //    // var txResult = await CharityTestStub.SetProject.SendAsync(new NewProject() {Title = "d", Description = "x"});
        //     txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        //     var output = new GreetToOutput();
        //     output.MergeFrom(txResult.TransactionResult.ReturnValue);
        //     output.Name.ShouldBe(name);
        //     output.GreetTime.ShouldNotBeNull();
        // }
        
        [Fact]
        public async Task FilesToHashCall_ReturnsHashMessage()
        {
            var txResult = await CharityTestStub.SetProject.SendAsync(new NewProject{Title="v",Description = "a"});
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var text = new Empty();
            text.MergeFrom(txResult.TransactionResult.ReturnValue);
            text.ShouldNotBeNull();
        }
        
        [Fact]
        public async Task VerifyFilesCall_ReturnsBytesValueMessage()
        {
            var txResult = await EvidenceContractStub.VerifyFiles.SendAsync(new Hash());
            txResult.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
            var text = new BytesValue();
            text.MergeFrom(txResult.TransactionResult.ReturnValue);
            text.ShouldNotBeNull();
        }
        
    }
}

