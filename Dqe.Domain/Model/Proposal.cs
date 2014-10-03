using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class Proposal : Entity<Transformers.Proposal>
    {
        private readonly ICollection<ProjectVersion> _projectVersions;
        private readonly IProposalRepository _proposalRepository;

        public Proposal(IProposalRepository proposalRepository)
        {
            _projectVersions = new Collection<ProjectVersion>();
            _proposalRepository = proposalRepository;
        }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual ProposalSourceType ProposalSource { get; protected internal set; }

        [StringLength(500)]
        public virtual string Comment { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; protected internal set; }

        public virtual Project MyProject { get; protected internal set; }

        public virtual IEnumerable<ProjectVersion> ProjectVersions
        {
            get { return _projectVersions.ToList().AsReadOnly(); }
        }

        public virtual void ConvertToGaming()
        {
            ProposalSource = ProposalSourceType.Gaming;
            Comment = "A new Proposal was associated to one or more Projects in Web Trans*Port that have DQE estimates.  This Proposal was converted to a Gaming Proposal.";
            LastUpdated = DateTime.Now;
        }

        public override Transformers.Proposal GetTransformer()
        {
            return new Transformers.Proposal
            {
                Id = Id,
                ProposalNumber = ProposalNumber,
                ProposalSource = ProposalSource,
                Created = Created,
                LastUpdated = LastUpdated
            };
        }

        public override void Transform(Transformers.Proposal transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            ProposalNumber = transformer.ProposalNumber;
            ProposalSource = transformer.ProposalSource;
            if (Id == 0)
            {
                Created = DateTime.Now;
            }
            LastUpdated = DateTime.Now;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //var propFamily = _proposalRepository.GetByNumber(ProposalNumber);
            //if (propFamily.Count(i => i.ProposalSource == ProposalSourceType.Wt) > 1)
            //{
            //    yield return new ValidationResult(string.Format("Only one Proposal with number {0} can have a source of Web Trns*Port", ProposalNumber));
            //}
            return new List<ValidationResult>();
        }
    }
}