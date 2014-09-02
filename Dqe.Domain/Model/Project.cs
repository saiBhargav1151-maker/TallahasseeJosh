using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;

namespace Dqe.Domain.Model
{
    public class Project : Entity<Transformers.Project>
    {
        private readonly ICollection<EstimateGroup> _estimateGroups;

        public Project()
        {
            _estimateGroups = new Collection<EstimateGroup>();
        }

        public virtual Proposal MyProposal { get; protected internal set; }

        public virtual MasterFile MyMasterFile { get; protected internal set; }

        [StringLength(11)]
        public virtual string ProjectNumber { get; protected internal set; }

        [StringLength(256)]
        public virtual string District { get; protected internal set; }

        [StringLength(256)]
        public virtual string County { get; protected internal set; }

        public virtual DateTime? LettingDate { get; protected internal set; }

        public virtual string Description { get; protected internal set; }

        public virtual int? DesignerSrsId { get; protected internal set; }

        public virtual int? PseeContactSrsId { get; protected internal set; }

        public virtual IEnumerable<EstimateGroup> EstimateGroups
        {
            get { return _estimateGroups.ToList().AsReadOnly(); }
        }

        public virtual void AddEstimateGroup(EstimateGroup estimateGroup, DqeUser account)
        {
            if (estimateGroup == null) throw new ArgumentNullException("estimateGroup");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            _estimateGroups.Add(estimateGroup);
            estimateGroup.MyProject = this;
        }

        public override Transformers.Project GetTransformer()
        {
            return new Transformers.Project
            {
                County = County,
                Description = Description,
                DesignerSrsId = DesignerSrsId,
                District = District,
                Id = Id,
                LettingDate = LettingDate,
                ProjectNumber = ProjectNumber,
                PseeContactSrsId = PseeContactSrsId
            };
        }

        public override void Transform(Transformers.Project transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            County = transformer.County;
            Description = transformer.Description;
            DesignerSrsId = transformer.DesignerSrsId;
            District = transformer.District;
            LettingDate = transformer.LettingDate;
            ProjectNumber = transformer.ProjectNumber;
            PseeContactSrsId = transformer.PseeContactSrsId;
        }
    }
}