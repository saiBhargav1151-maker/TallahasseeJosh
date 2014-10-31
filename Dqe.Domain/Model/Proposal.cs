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
        private readonly ICollection<Project> _projects;
        private readonly ICollection<SectionGroup> _sectionGroups; 
        private readonly IProposalRepository _proposalRepository;

        public Proposal(IProposalRepository proposalRepository)
        {
            _projectVersions = new Collection<ProjectVersion>();
            _projects = new Collection<Project>();
            _sectionGroups = new Collection<SectionGroup>();
            _proposalRepository = proposalRepository;
        }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual ProposalSourceType ProposalSource { get; protected internal set; }

        [StringLength(500)]
        public virtual string Comment { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        public virtual DateTime LettingDate { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; protected internal set; }

        [StringLength(256)]
        public virtual string District { get; protected internal set; }

        public virtual County County { get; protected internal set; }

        [StringLength(256)]
        public virtual string Description { get; protected internal set; }

        public virtual IEnumerable<Project> Projects
        {
            get { return _projects.ToList().AsReadOnly(); }
        } 

        public virtual IEnumerable<ProjectVersion> ProjectVersions
        {
            get { return _projectVersions.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<SectionGroup> SectionGroups
        {
            get { return _sectionGroups.ToList().AsReadOnly(); }
        } 

        protected internal virtual void AddProject(Project project)
        {
            if (_projects.Contains(project)) return;
            _projects.Add(project);
        }

        public virtual void ConvertToGaming()
        {
            ProposalSource = ProposalSourceType.Gaming;
            Comment = "A new Proposal was associated to one or more Projects in Web Trns*Port that have DQE estimates.  This Proposal was converted to a Gaming Proposal.";
            LastUpdated = DateTime.Now;
        }

        public virtual IEnumerable<EstimateGroup> GetEstimateGroups(DqeUser dqeUser)
        {
            var versions = Projects.Select(i => i.ProjectVersions.First(ii => ii.VersionOwner == dqeUser)).Distinct();
            var estimates = versions.Select(i => i.ProjectEstimates.First(ii => ii.IsWorkingEstimate)).Distinct();
            var groups = new List<EstimateGroup>();
            foreach (var estimate in estimates)
            {
                groups.AddRange(estimate.EstimateGroups);
            }
            return groups.Distinct();
        }

        public virtual void SetCounty(County county)
        {
            County = county;
        }

        public override Transformers.Proposal GetTransformer()
        {
            return new Transformers.Proposal
            {
                Id = Id,
                ProposalNumber = ProposalNumber,
                ProposalSource = ProposalSource,
                Created = Created,
                LastUpdated = LastUpdated,
                Description = Description,
                LettingDate = LettingDate,
                District = District
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
                WtId = transformer.WtId;
            }
            LastUpdated = DateTime.Now;
            District = transformer.District;
            Description = transformer.Description;
            LettingDate = transformer.LettingDate;
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