using System;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CharityTest
{
    public class CharityTest : CharityContainer.CharityBase
    {
        public override Empty Initial(Empty input)
        {
            // return new Empty(); // 否则自动生成代码：返回base.函数自己的调用。保证编译通过，无意义。
            State.Projects.Value = new Project()
            {
                Id = 1,
                Creator = new Address(),
                Donee = "3",
                DueDay = new Timestamp()
            };
           return new Empty(); 
        }

        public override Empty Set(Project input)
        {
            State.Projects.Value = input;
            return new Empty();
        }

        public override Project Get(Empty input)
        {
            Project pj = State.Projects.Value;
            return pj;
        }

        public override Testmessage1 Test1(Testmessage1 input)
        {
            State.Tests2[input.IntKey] = input.StringValue;
            State.Tests1[input.IntKey] = input;

            var result = State.Tests1[input.IntKey];
            return result;
        }
    }
}