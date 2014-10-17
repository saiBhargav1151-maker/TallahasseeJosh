using System;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class Proposal : Transformer
    {
        public string ProposalNumber { get; set; }
        public ProposalSourceType ProposalSource { get; set; }
        public long WtId { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; protected internal set; }
        public DateTime LastUpdated { get; protected internal set; }
        public string Description { get; set; }
    }
}