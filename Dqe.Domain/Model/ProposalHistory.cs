using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model
{
    public class ProposalHistory : Entity<Transformers.ProposalHistory>
    {
        private readonly ICollection<BidHistory> _bidHistories;

        public ProposalHistory()
        {
            _bidHistories = new Collection<BidHistory>();
        }

        public virtual PayItemMaster MyPayItemMaster { get; protected internal set; }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual string County { get; protected internal set; }

        public virtual DateTime LettingDate { get; protected internal set; }

        public virtual decimal Quantity { get; protected internal set; }

        public virtual long Duration { get; protected internal set; }

        public virtual string ContractType { get; protected internal set; }

        public virtual string ProposalType { get; protected internal set; }

        public virtual string ContractWorkType { get; protected internal set; }

        public virtual IEnumerable<BidHistory> BidHistories
        {
            get { return _bidHistories.ToList().AsReadOnly(); }
        }

        public virtual void AddBidHistory(BidHistory bidHistory)
        {
            bidHistory.MyProposalHistory = this;
            _bidHistories.Add(bidHistory);
        }

        public override Transformers.ProposalHistory GetTransformer()
        {
            return new Transformers.ProposalHistory
            {
                ProposalNumber = ProposalNumber,
                County = County,
                LettingDate = LettingDate,
                Quantity = Quantity,
                ContractType = ContractType,
                ContractWorkType = ContractWorkType,
                ProposalType = ProposalType,
                Duration = Duration
            };
        }

        public override void Transform(Transformers.ProposalHistory transformer, DqeUser account)
        {
            ProposalNumber = transformer.ProposalNumber;
            County = transformer.County;
            LettingDate = transformer.LettingDate;
            Quantity = transformer.Quantity;
            ContractType = transformer.ContractType;
            ContractWorkType = transformer.ContractWorkType;
            ProposalType = transformer.ProposalType;
            Duration = transformer.Duration;
        }
    }
}