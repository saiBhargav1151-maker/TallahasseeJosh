using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class Project : Entity<Transformers.Project>
    {
        private readonly ICollection<EstimateGroup> _estimateGroups;
        private readonly IProjectRepository _projectRepository;
        private readonly ICommandRepository _commandRepository;

        public Project(ICommandRepository commandRepository, IProjectRepository projectRepository)
        {
            _estimateGroups = new Collection<EstimateGroup>();
            _projectRepository = projectRepository;
            _commandRepository = commandRepository;
        }

        public virtual Proposal MyProposal { get; protected internal set; }

        public virtual Project MySourceProject { get; protected internal set; }

        public virtual MasterFile MyMasterFile { get; protected internal set; }

        public virtual DateTime? LoadedFromWtOn { get; protected internal set; }

        [StringLength(11)]
        public virtual string ProjectNumber { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual int Version { get; protected internal set; }

        public virtual DqeUser Owner { get; protected internal set; }

        [StringLength(256)]
        public virtual string District { get; protected internal set; }

        [StringLength(256)]
        public virtual string County { get; protected internal set; }

        public virtual DateTime? LettingDate { get; protected internal set; }

        public virtual string Description { get; protected internal set; }

        public virtual int? DesignerSrsId { get; protected internal set; }

        public virtual int? PseeContactSrsId { get; protected internal set; }

        public virtual DateTime LastUpdated { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual SnapshotType Snapshot { get; protected internal set; }

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

        public virtual Project VersionProject(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            var currentVersion = _projectRepository.GetMaxVersionForOwner(ProjectNumber, account);
            if (currentVersion == 0 && account.Id == Owner.Id) throw new InvalidOperationException(string.Format("Project {0} does not have an existing version for Owner {1}", ProjectNumber, account.Name));
            //TODO: copy - project, estimate group, and pay items - this will need to include parameters in the future as well...
            var p = new Project(_commandRepository, _projectRepository);
            var t = GetTransformer();
            p.Transform(t, account);
            p.LoadedFromWtOn = null;
            p.Version = currentVersion + 1;
            p.Created = DateTime.Now;
            p.Snapshot = SnapshotType.None;
            p.MySourceProject = this;
            p.MyMasterFile = MyMasterFile;
            _commandRepository.Add(p);
            //for each - estimate group copy
            foreach (var eg in EstimateGroups)
            {
                var neg = new EstimateGroup();
                var teg = eg.GetTransformer();
                neg.Transform(teg, account);
                p.AddEstimateGroup(neg, account);
                foreach (var pi in eg.ProjectItems)
                {
                    var npi = new ProjectItem();
                    var tpi = pi.GetTransformer();
                    npi.Transform(tpi, account);
                    neg.AddProjectItem(npi, account);
                }
            }
            return p;
        }

        public virtual void TakeSnapshot(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            var projects = _projectRepository.GetAllByNumber(ProjectNumber).ToList();
            var hasPhase2 = projects.Any(i => i.Snapshot == SnapshotType.Phase2);
            var hasPhase3 = projects.Any(i => i.Snapshot == SnapshotType.Phase3);
            if (!hasPhase2)
            {
                Snapshot = SnapshotType.Phase2;
            }
            else if (!hasPhase3)
            {
                Snapshot = SnapshotType.Phase3;
            }
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
                Version = Version,
                PseeContactSrsId = PseeContactSrsId,
                LastUpdated = LastUpdated,
                Snapshot = Snapshot
            };
        }

        public virtual void TransformForSync(Transformers.Project transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            ProjectNumber = transformer.ProjectNumber;
            if (Id == 0)
            {
                Owner = account;
                //Version = 1;
                var currentVersion = _projectRepository.GetMaxVersionForOwner(ProjectNumber, account);
                Version = currentVersion + 1;
                Created = DateTime.Now;
            }
            County = transformer.County;
            Description = transformer.Description;
            DesignerSrsId = transformer.DesignerSrsId;
            District = transformer.District;
            LettingDate = transformer.LettingDate;
            PseeContactSrsId = transformer.PseeContactSrsId;
            if (transformer.Snapshot == SnapshotType.Authorization || transformer.Snapshot == SnapshotType.Official)
            {
                throw new InvalidOperationException("Projects may only have Phase II and Phase III snapshots.");
            }
        }

        public override void Transform(Transformers.Project transformer, DqeUser account)
        {
            TransformForSync(transformer, account);
            LastUpdated = DateTime.Now;
            if (!LoadedFromWtOn.HasValue && MySourceProject == null)
            {
                LoadedFromWtOn = DateTime.Now;    
            }
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Snapshot == SnapshotType.Phase2 || Snapshot == SnapshotType.Phase3)
            {
                //TODO: add validate to verify that this project does not already contain a Phase II or Phase III estimate

            }
            if (LoadedFromWtOn.HasValue && MySourceProject != null)
            {
                yield return new ValidationResult(string.Format("Project {0} Version {1} cannot have a WT loaded date and source project reference", ProjectNumber, Version));
            }
            if (!LoadedFromWtOn.HasValue && MySourceProject == null)
            {
                yield return new ValidationResult(string.Format("Project {0} Version {1} must have a WT loaded date or source project reference", ProjectNumber, Version));
            }
        }
    }
}