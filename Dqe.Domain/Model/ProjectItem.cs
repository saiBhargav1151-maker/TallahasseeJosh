using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace Dqe.Domain.Model
{
    public class ProjectItem : Entity<Transformers.ProjectItem>
    {
        //private readonly ICollection<ProposalHistory> _proposalHistories;

        public ProjectItem()
        {
            //_proposalHistories = new Collection<ProposalHistory>();
        }
        
        [Required]
        public virtual EstimateGroup MyEstimateGroup { get; protected internal set; }

        public virtual ProposalItem MyProposalItem { get; protected internal set; }

        //public virtual IEnumerable<ProposalHistory> ProposalHistories
        //{
        //    get { return _proposalHistories.ToList().AsReadOnly(); }
        //} 

        public virtual long? ParentProposalId { get; protected internal set; }

        public virtual decimal Quantity { get; protected internal set; }

        public virtual decimal PublicPrice { get; protected internal set; }

        public virtual decimal Price { get; protected internal set; }

        public virtual decimal? PreviousPrice { get; protected internal set; }

        public virtual PriceSetType PriceSet { get; protected internal set; }

        public virtual int? ProposalItemId { get; protected internal set; }

        [StringLength(200)]
        public virtual string CalculatedUnit { get; protected internal set; }

        [StringLength(10)]
        public virtual string Unit { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        [StringLength(12)]
        public virtual string PayItemNumber { get; protected internal set; }

        [StringLength(256)]
        public virtual string PayItemDescription { get; protected internal set; }

        //public virtual bool IsLumpSum { get; protected internal set; }

        public virtual bool CombineWithLikeItems { get; protected internal set; }

        public virtual string AlternateSet { get; protected internal set; }

        public virtual string AlternateMember { get; protected internal set; }

        //public virtual string LineNumber { get; protected internal set; }

        public virtual string Fund { get; protected internal set; }

        public virtual string SupplementalDescription { get; protected internal set; }

        public virtual int BidHistoryMonthRange { get; protected internal set; }

        //public virtual void ClearHistory()
        //{
        //    _proposalHistories.Clear();
        //}

        //public virtual ProposalHistory AddProposalHistory(dynamic proposal)
        //{
        //    var ph = new ProposalHistory
        //    {
        //        LettingDate = proposal.letting,
        //        ProposalNumber = proposal.proposal,
        //        MyProjectItem = this
        //    };
        //    _proposalHistories.Add(ph);
        //    return ph;
        //}

        public virtual void ResetPublicPrice()
        {
            PublicPrice = Price;
        }

        public override Transformers.ProjectItem GetTransformer()
        {
            return new Transformers.ProjectItem
            {
                Id = Id,
                Quantity = Quantity,
                Price = Price,
                PreviousPrice = PreviousPrice,
                PayItemNumber = PayItemNumber,
                PayItemDescription = PayItemDescription,
                CalculatedUnit = CalculatedUnit,
                Unit = Unit,
                //IsLumpSum = IsLumpSum,
                CombineWithLikeItems = CombineWithLikeItems
            };
        }

        public override void Transform(Transformers.ProjectItem transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.AdminReadOnly && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator && account.Role != DqeRole.Coder)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (Math.Round(Price, 2) != Math.Round(transformer.Price, 2))
            {
                PreviousPrice = Math.Round(Price, 2);
                Price = Math.Round(transformer.Price, 2);
                PriceSet = transformer.PriceSet;
                MyEstimateGroup.MyProjectEstimate.LastUpdated = DateTime.Now;    
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MyEstimateGroup.MyProjectEstimate.Label == SnapshotLabel.Authorization)
            {
                PublicPrice = Price;
            }
            else
            {
                PublicPrice = !MyEstimateGroup.MyProjectEstimate.MyProjectVersion.MyProject.HasAuthorizationEstimate() ? Price : 0;    
            }
            return base.Validate(validationContext);
        }
    }
}