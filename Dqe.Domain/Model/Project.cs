using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using System.Xml;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class Project : Entity<Transformers.Project>
    {
        private readonly ICollection<ProjectVersion> _projectVersions;
        private readonly IProjectRepository _projectRepository;
        private readonly ICommandRepository _commandRepository;
        
        public Project(IProjectRepository projectRepository, ICommandRepository commandRepository)
        {
            _projectVersions = new Collection<ProjectVersion>();
            _projectRepository = projectRepository;
            _commandRepository = commandRepository;
        }

        [Required]
        public virtual MasterFile MyMasterFile { get; protected internal set; }

        [StringLength(15)]
        public virtual string ProjectNumber { get; protected internal set; }

        [StringLength(256)]
        public virtual string District { get; protected internal set; }

        public virtual County MyCounty { get; protected internal set; }

        public virtual DateTime? LettingDate { get; protected internal set; }

        public virtual DqeUser CustodyOwner { get; protected internal set; }

        public virtual string Description { get; protected internal set; }

        [StringLength(256)]
        public virtual string DesignerName { get; protected internal set; }

        public virtual int? PseeContactSrsId { get; protected internal set; }

        public virtual IEnumerable<ProjectVersion> ProjectVersions 
        {
            get { return _projectVersions.ToList().AsReadOnly(); }
        }

        public virtual void SetCounty(County county)
        {
            MyCounty = county;
        }

        public override Transformers.Project GetTransformer()
        {
            return new Transformers.Project
            {
                Description = Description,
                DesignerName = DesignerName,
                District = District,
                Id = Id,
                LettingDate = LettingDate,
                ProjectNumber = ProjectNumber,
                PseeContactSrsId = PseeContactSrsId
            };
        }

        public virtual ProjectSnapshot CreateNewVersionFromLre(string comment, object source, DqeUser account)
        {
            return null;
        }

        public virtual ProjectSnapshot CreateNewVersionFromWt(string comment, Wt.Estimate source, DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            //make sure custody is by this account or available
            if (CustodyOwner == null)
            {
                CustodyOwner = account;
            }
            if (CustodyOwner != account)
            {
                throw new InvalidOperationException(string.Format("A new version is not allowed because User {0} does not have custody of Project {1}", account.Name, ProjectNumber));
            }
            //reset any current working estimates
            var version = ProjectVersions.FirstOrDefault(i => i.VersionOwner == account && i.ProjectSnapshots.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            if (version != null)
            {
                var ss = version.ProjectSnapshots.Where(i => i.IsWorkingEstimate).ToList();
                foreach (var snap in ss)
                {
                    snap.IsWorkingEstimate = false;
                }
            }
            //create new version
            var v = new ProjectVersion
            {
                MyProject = this,
                ProjectSource = ProjectSourceType.Wt,
                Version = _projectRepository.GetMaxVersion(ProjectNumber) + 1,
                VersionOwner = account,
            };
            _projectVersions.Add(v);
            //create new snapshot
            var s = new ProjectSnapshot
            {
                Created = DateTime.Now,
                Label = SnapshotLabel.Estimator,
                LastUpdated = DateTime.Now,
                Snapshot = _projectRepository.GetMaxSnapshot(ProjectNumber, v.Version) + 1,
                SnapshotComment = comment,
                IsWorkingEstimate = true
            };
            v.AddSnapshot(s);
            foreach (var eg in source.EstimateGroup)
            {
                //create new estimate group
                var e = new EstimateGroup { Description = eg.Description };
                s.AddEstimateGroup(e);
                foreach (var pi in eg.EstimateItem)
                {
                    //create new project item
                    var p = new ProjectItem
                    {
                        PayItemDescription = pi.Description,
                        PayItemNumber = pi.ItemCode,
                        Price = 0,
                    };
                    var q = pi.Quantity as XmlNode[];
                    if (q != null && q.Length == 1)
                    {
                        var qq = q[0] as XmlText;
                        if (qq != null)
                        {
                            decimal val;
                            if (decimal.TryParse(qq.Value, out val))
                            {
                                p.Quantity = val;
                            }
                        }
                    }
                    e.AddProjectItem(p);
                }
            }
            _commandRepository.Flush();
            account.MyRecentProjectSnapshot = s;
            return s;
        }

        public virtual ProjectSnapshot CreateNewVersionFromSnapshot(string comment, ProjectSnapshot source, DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (CustodyOwner == null)
            {
                CustodyOwner = account;
            }
            if (CustodyOwner != account)
            {
                throw new InvalidOperationException(string.Format("A new version is not allowed because User {0} does not have custody of Project {1}", account.Name, ProjectNumber));
            }
            //reset any current working estimates
            var version = ProjectVersions.FirstOrDefault(i => i.VersionOwner == account && i.ProjectSnapshots.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            if (version != null)
            {
                var ss = version.ProjectSnapshots.Where(i => i.IsWorkingEstimate).ToList();
                foreach (var snap in ss)
                {
                    snap.IsWorkingEstimate = false;
                }
            }
            var v = new ProjectVersion
            {
                MyProject = this,
                ProjectSource = ProjectSourceType.Snapshot,
                Version = _projectRepository.GetMaxVersion(ProjectNumber) + 1,
                VersionOwner = account,
                SnapshotSource = source
            };
            _projectVersions.Add(v);
            var s = new ProjectSnapshot
            {
                Created = DateTime.Now,
                Label = SnapshotLabel.Estimator,
                LastUpdated = DateTime.Now,
                Snapshot = _projectRepository.GetMaxSnapshot(ProjectNumber, v.Version) + 1,
                SnapshotComment = comment,
                IsWorkingEstimate = true
            };
            v.AddSnapshot(s);
            foreach (var eg in source.EstimateGroups)
            {
                var e = new EstimateGroup {Description = eg.Description};
                s.AddEstimateGroup(e);
                foreach (var pi in eg.ProjectItems)
                {
                    var p = new ProjectItem
                    {
                        PayItemDescription = pi.PayItemDescription,
                        PayItemNumber = pi.PayItemNumber,
                        Price = pi.Price,
                        Quantity = pi.Quantity
                    };
                    e.AddProjectItem(p);
                }
            }
            _commandRepository.Flush();
            account.MyRecentProjectSnapshot = s;
            return s;
        }

        public virtual void SnapshotWorkingEstimate(DqeUser account, bool labelSnapshot)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (CustodyOwner == account)
            {
                if (account.MyRecentProjectSnapshot == null || account.MyRecentProjectSnapshot.MyProjectVersion.MyProject != this)
                {
                    throw new InvalidOperationException("Working Estimate snapshot could not be determined");
                }
                foreach (var v in _projectVersions.Where(i => i.VersionOwner == account))
                {
                    foreach (var ps in v.ProjectSnapshots)
                    {
                        ps.IsWorkingEstimate = false;
                    }
                }
                var source = account.MyRecentProjectSnapshot;
                source.Label = labelSnapshot ? GetNextSnapshotLabel() : SnapshotLabel.Estimator;
                var s = new ProjectSnapshot
                {
                    Created = DateTime.Now,
                    Label = SnapshotLabel.Estimator,
                    LastUpdated = DateTime.Now,
                    Snapshot = _projectRepository.GetMaxSnapshot(ProjectNumber, source.MyProjectVersion.Version) + 1,
                    SnapshotComment = string.Empty,
                    IsWorkingEstimate = true
                };
                source.MyProjectVersion.AddSnapshot(s);
                foreach (var eg in source.EstimateGroups)
                {
                    var e = new EstimateGroup { Description = eg.Description };
                    s.AddEstimateGroup(e);
                    foreach (var pi in eg.ProjectItems)
                    {
                        var p = new ProjectItem
                        {
                            PayItemDescription = pi.PayItemDescription,
                            PayItemNumber = pi.PayItemNumber,
                            Price = pi.Price,
                            Quantity = pi.Quantity
                        };
                        e.AddProjectItem(p);
                    }
                }
                source.MyProjectVersion.AddSnapshot(s);
                account.MyRecentProjectSnapshot = s;
            }
            else
            {
                throw new InvalidOperationException("Estimator can only release custody on a project he has custody on.");
            }
        }

        public virtual SnapshotLabel GetNextSnapshotLabel()
        {
            var hasPhase2 = false;
            var hasPhase3 = false;
            foreach (var v in _projectVersions)
            {
                foreach (var ps in v.ProjectSnapshots)
                {
                    if (ps.Label == SnapshotLabel.Phase2) hasPhase2 = true;
                    if (ps.Label == SnapshotLabel.Phase3) hasPhase3 = true;
                }
            }
            if (hasPhase3) return SnapshotLabel.Estimator;
            if (hasPhase2) return SnapshotLabel.Phase3;
            return SnapshotLabel.Phase2;
        } 

        public virtual void AssignWorkingEstimate(ProjectVersion version, DqeUser account)
        {
            if (version == null) throw new ArgumentNullException("version");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (CustodyOwner != account)
            {
                throw new InvalidOperationException(string.Format("A new version is not allowed because User {0} does not have custody of Project {1}", account.Name, ProjectNumber));
            }
            if (version.MyProject != this)
            {
                throw new InvalidOperationException(string.Format("Version is for Project {0}", version.MyProject.ProjectNumber));
            }
            if (account.MyRecentProjectSnapshot.MyProjectVersion.MyProject != this)
            {
                throw new InvalidOperationException(string.Format("Working Estimate Snapshot is for Project {0}", account.MyRecentProjectSnapshot.MyProjectVersion.MyProject.ProjectNumber));
            }
            foreach (var v in _projectVersions.Where(i => i.VersionOwner == account))
            {
                foreach (var ps in v.ProjectSnapshots)
                {
                    ps.IsWorkingEstimate = false;
                }
            }
            var we = version.ProjectSnapshots.SingleOrDefault(i => i.Snapshot == version.ProjectSnapshots.Max(ii => ii.Snapshot));
            if (we == null)
            {
                throw new InvalidOperationException(string.Format("Max Snapshot was not found for Project {0} Version {1}", version.MyProject.ProjectNumber, version.Version));
            }
            we.IsWorkingEstimate = true;
            account.MyRecentProjectSnapshot = we;
        }

        public virtual void ReleaseCustody(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (account.Role == DqeRole.System || account.Role == DqeRole.Administrator || account.Role == DqeRole.DistrictAdministrator)
            {
                CustodyOwner = null;
            }
            if (account.Role == DqeRole.Estimator)
            {
                if (CustodyOwner == account)
                {
                    CustodyOwner = null;
                }
                else
                {
                    throw new InvalidOperationException("Estimator can only release custody on a project he has custody on.");
                }
            }
        }

        public virtual void AquireCustody(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (CustodyOwner == null)
            {
                CustodyOwner = account;
            }
            else
            {
                throw new InvalidOperationException("Estimator can only release custody on a project he has custody on.");
            }
        }

        public override void Transform(Transformers.Project transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            ProjectNumber = transformer.ProjectNumber;
            Description = transformer.Description;
            DesignerName = transformer.DesignerName;
            District = transformer.District;
            LettingDate = transformer.LettingDate;
            PseeContactSrsId = transformer.PseeContactSrsId;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}