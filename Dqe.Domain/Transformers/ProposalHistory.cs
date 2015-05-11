using System;

namespace Dqe.Domain.Transformers
{
    public class ProposalHistory : Transformer
    {
        public string ProposalNumber { get; set; }

        public string County { get; set; }

        public DateTime LettingDate { get; set; }

        public decimal Quantity { get; set; }

        public string ContractType { get; set; }

        public string ProposalType { get; set; }

        public string ContractWorkType { get; set; }

        public long Duration { get; set; }
    }
}