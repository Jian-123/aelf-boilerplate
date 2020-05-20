using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.CharityDonateContract
{
    public class CharityAccount : 
    {
        public override Empty Initial(Empty input)
        {
            Assert(!State.Initialized.Value, "Already initialized.");
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.ConsensusContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.ConsensusContractSystemName);

            // Create and issue token of this contract.
            State.TokenContract.Create.Send(new CreateInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                TokenName = "Bingo Card",
                Decimals = 0,
                Issuer = Context.Self,
                IsBurnable = true,
                TotalSupply = long.MaxValue,
                LockWhiteList = {Context.Self}
            });

            State.TokenContract.Issue.Send(new IssueInput
            {
                Symbol = BingoGameContractConstants.CardSymbol,
                Amount = long.MaxValue,
                To = Context.Self,
                Memo = "All to issuer."
            });
            State.Initialized.Value = true;
            return new Empty();
        }

       
        
        
        message TransferFromInput {
            aelf.Address from = 1;
            aelf.Address to = 2;
        string symbol = 3;
        sint64 amount = 4;
        string memo = 5;
    }

        public override Empty TransferFrom(TransferFrom input){
            AssertValidSymbolAndAmount(input.Symbol, input.Amount);
        }
        
        
        
        public override Empty TransferFrom(TransferFromInput input)
        {
            AssertValidSymbolAndAmount(input.Symbol, input.Amount);
            // First check allowance.
            var allowance = State.Allowances[input.From][Context.Sender][input.Symbol];
            if (allowance < input.Amount)
            {
                if (IsInWhiteList(new IsInWhiteListInput {Symbol = input.Symbol, Address = Context.Sender}).Value ||
                    IsContributingProfits(input))
                {
                    DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
                    return new Empty();
                }

                Assert(false,
                    $"[TransferFrom]Insufficient allowance. Token: {input.Symbol}; {allowance}/{input.Amount}.\n" +
                    $"From:{input.From}\tSpender:{Context.Sender}\tTo:{input.To}");
            }

            DoTransfer(input.From, input.To, input.Symbol, input.Amount, input.Memo);
            State.Allowances[input.From][Context.Sender][input.Symbol] = allowance.Sub(input.Amount);
            return new Empty();
        }


    }
}


