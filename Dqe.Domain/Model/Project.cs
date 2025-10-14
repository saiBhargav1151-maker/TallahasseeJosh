using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Fdot;
using Dqe.Domain.Repositories;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class Project : Entity<Transformers.Project>
    {
        private readonly ICollection<ProjectVersion> _projectVersions;
        private readonly ICollection<Proposal> _proposals;
        private readonly ICollection<DqeUser> _assignedUsers; 
        private readonly IProjectRepository _projectRepository;
        private readonly ICommandRepository _commandRepository;
        private readonly IWebTransportService _webTransportService;
        
        public Project(IProjectRepository projectRepository, ICommandRepository commandRepository, IWebTransportService webTransportService)
        {
            _projectVersions = new Collection<ProjectVersion>();
            _proposals = new Collection<Proposal>();
            _assignedUsers = new Collection<DqeUser>();
            _projectRepository = projectRepository;
            _commandRepository = commandRepository;
            _webTransportService = webTransportService;
        }

        [Required]
        public virtual MasterFile MyMasterFile { get; protected internal set; }

        [StringLength(15)]
        public virtual string ProjectNumber { get; protected internal set; }

        [StringLength(256)]
        public virtual string District { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        public virtual long WtLsDbId { get; protected internal set; }

        [StringLength(2)]
        public virtual string LsDbCode { get; protected internal set; }

        public virtual County MyCounty { get; protected internal set; }

        /// <summary>
        /// Pulling from Transport as either a 'C' or a 'M' for maintenance
        /// </summary>
        public virtual string ProjectType { get; set; }

        //public virtual DateTime? LettingDate { get; protected internal set; }

        public virtual DqeUser CustodyOwner { get; protected internal set; }

        public virtual string Description { get; protected internal set; }

        [StringLength(256)]
        public virtual string DesignerName { get; protected internal set; }

        public virtual int? PseeContactSrsId { get; protected internal set; }

        /// <summary>
        /// holds the estimate amounts grabbed from LRE snapshots
        /// </summary>
        public virtual decimal? EstimateInitial { get; protected internal set; }

        public virtual decimal? EstimateScope { get; protected internal set; }

        public virtual decimal? EstimatePhase1 { get; protected internal set; }

        public virtual decimal? EstimatePhase2 { get; protected internal set; }

        public virtual decimal? EstimatePhase3 { get; protected internal set; }

        public virtual decimal? EstimatePhase4 { get; protected internal set; }

        /// <summary>
        /// LRE Column - Dictates to user if they want DQE as the primary program instead of LRE
        /// It is in the DB as a single char byte
        /// </summary>
        public virtual string QuantityComplete { get; protected internal set; }

        public virtual IEnumerable<DqeUser> AssignedUsers
        {
            get { return _assignedUsers.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<ProjectVersion> ProjectVersions 
        {
            get { return _projectVersions.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<Proposal> Proposals
        {
            get { return _proposals.ToList().AsReadOnly(); }
        } 

        public virtual void SetCounty(County county)
        {
            MyCounty = county;
        }

        public virtual void SetSeedEstimateValues(decimal? initial, decimal? scope, decimal? phase1, decimal? phase2, decimal? phase3, decimal? phase4, string Qt)
        {
            EstimateInitial = initial;
            EstimateScope = scope;
            EstimatePhase1 = phase1;
            EstimatePhase2 = phase2;
            EstimatePhase3 = phase3;
            EstimatePhase4 = phase4;
            QuantityComplete = Qt;
        }

        public virtual void AssignToUser(DqeUser assignmentUser, DqeUser account)
        {
            if (assignmentUser == null) throw new ArgumentNullException("assignmentUser");
            if (account == null) throw new ArgumentNullException("account");
            if (assignmentUser.IsInDqeDistrict(District)) return;
            if (_assignedUsers.Contains(assignmentUser)) return;
            if (account.Role == DqeRole.Administrator
                || (account.Role == DqeRole.DistrictAdministrator && account.IsInDqeDistrict(District))
                || (account.Role == DqeRole.MaintenanceDistrictAdmin && account.IsInDqeDistrict(District)))
            {
                _assignedUsers.Add(assignmentUser);    
                return;
            }
            throw new SecurityException("Only system admins or district admins for this project's district can assign access rights to a user outside this project's district.");
        }

        public virtual void AddProposal(Proposal proposal, DqeUser account)
        {
            if (proposal == null) throw new ArgumentNullException("proposal");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System 
                && account.Role != DqeRole.Administrator 
                && account.Role != DqeRole.AdminReadOnly
                && account.Role != DqeRole.DistrictAdministrator 
                && account.Role != DqeRole.Estimator
                && account.Role != DqeRole.Coder
                && account.Role != DqeRole.MaintenanceDistrictAdmin
                && account.Role != DqeRole.MaintenanceEstimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (!_proposals.Contains(proposal))
            {
                _proposals.Add(proposal);    
            }
            proposal.AddProject(this);
        }

        public override Transformers.Project GetTransformer()
        {
            return new Transformers.Project
            {
                Description = Description,
                DesignerName = DesignerName,
                District = District,
                Id = Id,
                //LettingDate = LettingDate,
                ProjectNumber = ProjectNumber,
                PseeContactSrsId = PseeContactSrsId,
                WtId = WtId,
                WtLsDbId = WtLsDbId,
                LsDbCode = LsDbCode,
                QuantityComplete = QuantityComplete
            };
        }

        public virtual void RemoveProposal(Proposal proposal)
        {
            var p = _proposals.FirstOrDefault(i => i.Id == proposal.Id);
            if (p != null)
            {
                _proposals.Remove(p);    
            }
        }

        public virtual ProjectEstimate CreateNewVersionFromLre(string comment, object source, DqeUser account)
        {
            return null;
        }

        public virtual ProjectEstimate CreateNewVersionFromWt(string comment, Wt.Project source, bool initializePrices, DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System
                && account.Role != DqeRole.Administrator 
                && account.Role != DqeRole.DistrictAdministrator 
                && account.Role != DqeRole.Estimator
                && account.Role != DqeRole.MaintenanceEstimator
                && account.Role != DqeRole.MaintenanceDistrictAdmin
                && account.Role != DqeRole.Coder)
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
            var prop = Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            if (prop != null)
            {
                prop.CurrentEstimator = null;
            }
            //reset any current working estimates
            var version = ProjectVersions.FirstOrDefault(i => i.VersionOwner == account && i.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            if (version != null)
            {
                var ss = version.ProjectEstimates.Where(i => i.IsWorkingEstimate).ToList();
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
            var s = new ProjectEstimate(_webTransportService)
            {
                Created = DateTime.Now,
                Label = account.Role != DqeRole.Coder ? SnapshotLabel.Estimator : SnapshotLabel.Coder,
                LastUpdated = DateTime.Now,
                Estimate = _projectRepository.GetMaxEstimate(ProjectNumber, v.Version) + 1,
                EstimateComment = comment,
                IsWorkingEstimate = true
            };
            v.AddEstimate(s);
            //var categories = source.ProjectItems.Select(i => i.MyCategory).Distinct().ToList();
            foreach (var eg in source.Categories)
            {
                //create new estimate group
                var e = new EstimateGroup
                {
                    Name = eg.Name,
                    Description = eg.Description,
                    CombineWithLikeItems = eg.CombineLikeItems,
                    AlternateSet = eg.MyCategoryAlternate == null
                        ? string.Empty
                        : eg.MyCategoryAlternate.Name,
                    FederalConstructionClass = string.IsNullOrWhiteSpace(eg.FederalConstructionClass) ? string.Empty : eg.FederalConstructionClass,
                    WtId = eg.Id,
                    IsLsDbSummary = source.LsDbId != 0 && !eg.IsLsDbDetail,
                    AlternateMember = string.IsNullOrWhiteSpace(eg.AlternateMember) ? string.Empty : eg.AlternateMember
                };
                s.AddEstimateGroup(e);
                //var cat = eg;
                //var items = source.ProjectItems.Where(i => i.MyCategory == cat).Distinct();
                foreach (var pi in eg.ProjectItems)
                {
                    //create new project item
                    //var refItem = refItems.First(i => i.Name == pi.ItemCode);
                    var p = new ProjectItem
                    {
                        PayItemDescription = pi.MyRefItem.Description,
                        PayItemNumber = pi.MyRefItem.Name,
                        Price = initializePrices ? pi.Price : 0,
                        PriceSet = initializePrices ? pi.Price > 0 ? PriceSetType.EstimatorOverride : PriceSetType.NotSet : PriceSetType.NotSet,
                        AlternateMember = string.IsNullOrWhiteSpace(pi.AlternateMember) ? string.Empty : pi.AlternateMember,
                        //LineNumber = pi.LineNumber,
                        CalculatedUnit =
                            string.IsNullOrWhiteSpace(pi.MyRefItem.CalculatedUnit)
                                ? string.Empty
                                : pi.MyRefItem.CalculatedUnit,
                        Unit = string.IsNullOrWhiteSpace(pi.MyRefItem.Unit) ? string.Empty : pi.MyRefItem.Unit,
                        //IsLumpSum = pi.MyRefItem.LumpSum,
                        CombineWithLikeItems = pi.CombineLikeItems,
                        Quantity = pi.Quantity,
                        WtId = pi.Id,
                        SupplementalDescription = string.IsNullOrWhiteSpace(pi.SupplementalDescription) ? string.Empty : pi.SupplementalDescription,
                        Fund = pi.MyFundPackage == null ? string.Empty : pi.MyFundPackage.Name,
                        AlternateSet = pi.MyAlternate == null
                            ? string.Empty
                            : pi.MyAlternate.Name
                    };
                    e.AddProjectItem(p);
                }
            }
            _commandRepository.Flush();
            account.MyRecentProjectEstimate = s;
            return s;
        }

        public virtual ProjectEstimate CreateNewVersionFromSnapshot(string comment, ProjectEstimate source, DqeUser account)
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
            var prop = Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            if (prop != null)
            {
                prop.CurrentEstimator = null;
            }
            //reset any current working estimates
            var version = ProjectVersions.FirstOrDefault(i => i.VersionOwner == account && i.ProjectEstimates.FirstOrDefault(ii => ii.IsWorkingEstimate) != null);
            if (version != null)
            {
                var ss = version.ProjectEstimates.Where(i => i.IsWorkingEstimate).ToList();
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
                EstimateSource = source
            };
            _projectVersions.Add(v);
            var s = new ProjectEstimate(_webTransportService)
            {
                Created = DateTime.Now,
                Label = SnapshotLabel.Estimator,
                LastUpdated = DateTime.Now,
                Estimate = _projectRepository.GetMaxEstimate(ProjectNumber, v.Version) + 1,
                EstimateComment = comment,
                IsWorkingEstimate = true
            };
            v.AddEstimate(s);
            foreach (var eg in source.EstimateGroups)
            {
                var e = new EstimateGroup
                {
                    Name = eg.Name,
                    Description = eg.Description,
                    AlternateSet = eg.AlternateSet,
                    FederalConstructionClass = eg.FederalConstructionClass,
                    CombineWithLikeItems = eg.CombineWithLikeItems,
                    WtId = eg.WtId,
                    IsLsDbSummary = eg.IsLsDbSummary,
                    AlternateMember = eg.AlternateMember
                };
                s.AddEstimateGroup(e);
                foreach (var pi in eg.ProjectItems)
                {
                    var p = new ProjectItem
                    {
                        PayItemDescription = pi.PayItemDescription,
                        AlternateMember = pi.AlternateMember,
                        //LineNumber = pi.LineNumber,
                        PayItemNumber = pi.PayItemNumber,
                        Price = pi.Price,
                        PreviousPrice = pi.PreviousPrice,
                        PriceSet = pi.PriceSet,
                        Quantity = pi.Quantity,
                        CalculatedUnit = pi.CalculatedUnit,
                        Unit = pi.Unit,
                        //IsLumpSum = pi.IsLumpSum,
                        CombineWithLikeItems = pi.CombineWithLikeItems,
                        AlternateSet = pi.AlternateSet,
                        SupplementalDescription = pi.SupplementalDescription,
                        Fund = pi.Fund,
                        WtId = pi.WtId
                    };
                    e.AddProjectItem(p);
                }
            }
            _commandRepository.Flush();
            account.MyRecentProjectEstimate = s;
            return s;
        }

        /// <summary>
        /// This creates a new ReadOnly Version with a single estimate of type Review 'R' and 
        /// clones all estimate groups and items from the source estimate.
        /// Only System Admins (CO Admins) can create a review.
        /// This does NOT update info to LRE, this is intended to be read only (except the notes). MB. 
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="source"></param>
        /// <param name="account"></param>
        /// <returns><see cref="ProjectEstimate"/></returns>
        public virtual ProjectEstimate CreateNewReviewVersionFromSnapshot(string comment, ProjectEstimate source, DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator
                && account.Role != DqeRole.StateReviewer
                && account.Role != DqeRole.DistrictReviewer)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
          
            var v = new ProjectVersion
            {
                MyProject = this,
                ProjectSource = ProjectSourceType.Review,
                Version = _projectRepository.GetMaxVersion(ProjectNumber) + 1,
                VersionOwner = account,
                EstimateSource = source
            };
            _projectVersions.Add(v);
            var s = new ProjectEstimate(_webTransportService)
            {
                Created = DateTime.Now,
                Label = SnapshotLabel.Review,
                LastUpdated = source.LastUpdated,
                Estimate = 1,
                EstimateComment = comment,
                IsWorkingEstimate = false,
                
            };
            v.AddEstimate(s);
            if(source.EstimateGroups != null)
            {
                foreach (var eg in source.EstimateGroups)
                {
                    var e = new EstimateGroup
                    {
                        Name = eg.Name,
                        Description = eg.Description,
                        AlternateSet = eg.AlternateSet,
                        FederalConstructionClass = eg.FederalConstructionClass,
                        CombineWithLikeItems = eg.CombineWithLikeItems,
                        WtId = eg.WtId,
                        IsLsDbSummary = eg.IsLsDbSummary,
                        AlternateMember = eg.AlternateMember
                    };
                    s.AddEstimateGroup(e);
                    foreach (var pi in eg.ProjectItems)
                    {
                        var p = new ProjectItem
                        {
                            PayItemDescription = pi.PayItemDescription,
                            AlternateMember = pi.AlternateMember,
                            //LineNumber = pi.LineNumber,
                            PayItemNumber = pi.PayItemNumber,
                            Price = pi.Price,
                            PreviousPrice = pi.PreviousPrice,
                            PriceSet = pi.PriceSet,
                            Quantity = pi.Quantity,
                            CalculatedUnit = pi.CalculatedUnit,
                            Unit = pi.Unit,
                            //IsLumpSum = pi.IsLumpSum,
                            CombineWithLikeItems = pi.CombineWithLikeItems,
                            AlternateSet = pi.AlternateSet,
                            SupplementalDescription = pi.SupplementalDescription,
                            Fund = pi.Fund,
                            WtId = pi.WtId
                        };
                        e.AddProjectItem(p);
                    }
                }
            }
            
            _commandRepository.Flush();
            account.MyRecentProjectEstimate = s;
            return s;
        }

        public virtual void RemoveLabel(SnapshotLabel label, string comment)
        {
            foreach (var projectVersion in ProjectVersions)
            {
                foreach (var estimate in projectVersion.ProjectEstimates)
                {
                    if (estimate.Label == label)
                    {
                        if (label == SnapshotLabel.Phase4)
                        {
                            EstimatePhase4 = null;
                        }
                        if (label == SnapshotLabel.Phase3)
                        {
                            EstimatePhase3 = null;
                        }
                        if (label == SnapshotLabel.Phase2)
                        {
                            EstimatePhase2 = null;
                        }
                        if (label == SnapshotLabel.Phase1)
                        {
                            EstimatePhase1= null;
                        }
                        if (label == SnapshotLabel.Scope)
                        {
                            EstimateScope = null;
                        }
                        if (label == SnapshotLabel.Initial)
                        {
                            EstimateInitial= null;
                        }
                        estimate.Label = SnapshotLabel.Estimator;
                        estimate.LabelRemovedOn = DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            estimate.LabelRemovedComment = comment.Trim();
                        }
                        return;
                    }
                }
            }
        }

        protected internal virtual bool HasAuthorizationEstimate()
        {
            var next = GetNextProposalSnapshotLabel();
            return next == SnapshotLabel.Official || next == SnapshotLabel.Estimator;
        }

        public virtual void SnapshotWorkingEstimate(DqeUser account, bool labelSnapshot, string comment, bool proposalOverride, ILreService lreService)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator && account.Role != DqeRole.Coder)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (CustodyOwner == account)
            {
                if (!proposalOverride)
                {
                    if (account.MyRecentProjectEstimate == null || account.MyRecentProjectEstimate.MyProjectVersion.MyProject != this)
                    {
                        throw new InvalidOperationException("Working Estimate could not be determined");
                    }    
                }
                ProjectEstimate source = null;
                foreach (var v in _projectVersions.Where(i => i.VersionOwner == account))
                {
                    foreach (var ps in v.ProjectEstimates)
                    {
                        if (ps.IsWorkingEstimate)
                        {
                            source = ps;
                        }
                        ps.IsWorkingEstimate = false;
                    }
                }
                //var source = account.MyRecentProjectEstimate;
                if (source == null)
                {
                    throw new InvalidOperationException(string.Format("Working estimate could not be determined for project {0} and user {1}", ProjectNumber, account.Name));
                }
                if (labelSnapshot)
                {
                    source.LastUpdated = DateTime.Now;
                }
                if (proposalOverride)
                {
                    source.Label = labelSnapshot ? GetNextProposalSnapshotLabel() : SnapshotLabel.Estimator;
                    if (!string.IsNullOrWhiteSpace(comment)) source.SetComment(comment, account);
                    if (source.Label == SnapshotLabel.Official)
                    {
                        source.IsWorkingEstimate = true;
                        return;
                    }
                    if (source.Label == SnapshotLabel.Authorization)
                    {
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Authorization, source.GetEstimateTotal().Total);
                    }
                }
                else
                {
                    source.Label = labelSnapshot ? GetNextSnapshotLabel() : SnapshotLabel.Estimator;
                    if (!string.IsNullOrWhiteSpace(comment)) source.SetComment(comment, account);
                }
                if (labelSnapshot)
                {
                    if (source.Label == SnapshotLabel.Initial)
                    {
                        EstimateInitial = source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Initial, EstimateInitial.Value);
                    }
                    if (source.Label == SnapshotLabel.Scope)
                    {
                        EstimateScope= source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Scope, EstimateScope.Value);
                    }
                    if (source.Label == SnapshotLabel.Phase1)
                    {
                        EstimatePhase1 = source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Phase1, EstimatePhase1.Value);
                    }

                    if (source.Label == SnapshotLabel.Phase2)
                    {
                        EstimatePhase2 = source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Phase2, EstimatePhase2.Value);
                    }
                    if (source.Label == SnapshotLabel.Phase3)
                    {
                        EstimatePhase3 = source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Phase3, EstimatePhase3.Value);
                    }
                    if (source.Label == SnapshotLabel.Phase4)
                    {
                        EstimatePhase4 = source.GetEstimateTotal().Total;
                        lreService.SetDqeSnapshotInLre(this, account, SnapshotLabel.Phase4, EstimatePhase4.Value);
                    }
                    lreService.UpdateLreProjectSetDQEDefaultPlatform(this.ProjectNumber);

                }
                var prop = Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
                if (prop != null)
                {
                    prop.CurrentEstimator = null;
                }
                var s = new ProjectEstimate(_webTransportService)
                {
                    Created = DateTime.Now,
                    Label = SnapshotLabel.Estimator,
                    LastUpdated = DateTime.Now,
                    Estimate = _projectRepository.GetMaxEstimate(ProjectNumber, source.MyProjectVersion.Version) + 1,
                    EstimateComment = string.Empty,
                    IsWorkingEstimate = true
                };
                source.MyProjectVersion.AddEstimate(s);
                foreach (var eg in source.EstimateGroups)
                {
                    var e = new EstimateGroup
                    {
                        Name = eg.Name,
                        Description = eg.Description,
                        AlternateSet = eg.AlternateSet,
                        FederalConstructionClass = eg.FederalConstructionClass,
                        CombineWithLikeItems = eg.CombineWithLikeItems,
                        WtId = eg.WtId,
                        IsLsDbSummary = eg.IsLsDbSummary,
                        AlternateMember = eg.AlternateMember
                    };
                    s.AddEstimateGroup(e);
                    foreach (var pi in eg.ProjectItems)
                    {
                        var p = new ProjectItem
                        {
                            PayItemDescription = pi.PayItemDescription,
                            PayItemNumber = pi.PayItemNumber,
                            AlternateMember = pi.AlternateMember,
                            //LineNumber = pi.LineNumber,
                            Price = pi.Price,
                            PreviousPrice = pi.PreviousPrice,
                            PriceSet = pi.PriceSet,
                            Quantity = pi.Quantity,
                            CalculatedUnit = pi.CalculatedUnit,
                            Unit = pi.Unit,
                            //IsLumpSum = pi.IsLumpSum,
                            CombineWithLikeItems = pi.CombineWithLikeItems,
                            AlternateSet = pi.AlternateSet,
                            SupplementalDescription = pi.SupplementalDescription,
                            Fund = pi.Fund,
                            WtId = pi.WtId
                        };
                        e.AddProjectItem(p);
                    }
                }
                //source.MyProjectVersion.AddEstimate(s);
                if (!proposalOverride)
                {
                    account.MyRecentProjectEstimate = s;    
                }
            }
            else
            {
                throw new InvalidOperationException("Estimator can only release custody on a project he has custody on.");
            }
        }

        public virtual void CoderSnapshotWorkingEstimate(DqeUser account, ILreService lreService)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator
                && account.Role != DqeRole.Coder)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            ProjectEstimate source = null;
            var projectVersions = _projectVersions.Where(i => i.VersionOwner == account);
            if (projectVersions != null)
            {
                foreach (var pv in projectVersions)
                {
                    foreach (var ps in pv.ProjectEstimates)
                    {
                        if (ps.IsWorkingEstimate)
                        {
                            source = ps;
                        }
                    }
                }
            }
          
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("Working estimate could not be determined for project {0} and user {1}", ProjectNumber, account.Name));
            }
           

            var v = new ProjectVersion
            {
                MyProject = this,
                ProjectSource = ProjectSourceType.Snapshot,
                Version = _projectRepository.GetMaxVersion(ProjectNumber) + 1,
                VersionOwner = account,
                EstimateSource = source
            };
            _projectVersions.Add(v);
            var s = new ProjectEstimate(_webTransportService)
            {
                Created = DateTime.Now,
                Label = SnapshotLabel.Coder,
                LastUpdated = source.LastUpdated,
                Estimate = 1,
                EstimateComment = string.Empty,
                IsWorkingEstimate = false,
            };
            v.AddEstimate(s);
            if (source.EstimateGroups != null)
            {
                foreach (var eg in source.EstimateGroups)
                {
                    var e = new EstimateGroup
                    {
                        Name = eg.Name,
                        Description = eg.Description,
                        AlternateSet = eg.AlternateSet,
                        FederalConstructionClass = eg.FederalConstructionClass,
                        CombineWithLikeItems = eg.CombineWithLikeItems,
                        WtId = eg.WtId,
                        IsLsDbSummary = eg.IsLsDbSummary,
                        AlternateMember = eg.AlternateMember
                    };
                    s.AddEstimateGroup(e);
                    foreach (var pi in eg.ProjectItems)
                    {
                        var p = new ProjectItem
                        {
                            PayItemDescription = pi.PayItemDescription,
                            AlternateMember = pi.AlternateMember,
                            //LineNumber = pi.LineNumber,
                            PayItemNumber = pi.PayItemNumber,
                            //Price = pi.Price,
                            //PreviousPrice = pi.PreviousPrice,
                            PriceSet = pi.PriceSet,
                            Quantity = pi.Quantity,
                            CalculatedUnit = pi.CalculatedUnit,
                            Unit = pi.Unit,
                            //IsLumpSum = pi.IsLumpSum,
                            CombineWithLikeItems = pi.CombineWithLikeItems,
                            AlternateSet = pi.AlternateSet,
                            SupplementalDescription = pi.SupplementalDescription,
                            Fund = pi.Fund,
                            WtId = pi.WtId
                        };
                        e.AddProjectItem(p);
                    }
                }
            }

            _commandRepository.Flush();
            account.MyRecentProjectEstimate = s;
            //return s;
        }

        public virtual void SnapshotWorkingEstimate(DqeUser account, bool labelSnapshot, ILreService lreService)
        {
            SnapshotWorkingEstimate(account, labelSnapshot, string.Empty, false, lreService);
        }

        public virtual SnapshotLabel GetNextSnapshotLabel()
        {
            //checks existing milestone have price totals and sets variable true if so
            var hasInitial = EstimateInitial.HasValue;
            var hasScope = EstimateScope.HasValue;
            var hasPhase1 = EstimatePhase1.HasValue;

            var hasPhase2 = EstimatePhase2.HasValue;
            var hasPhase3 = EstimatePhase3.HasValue;
            var hasPhase4 = EstimatePhase4.HasValue;

            //checks all version estimates for milestones and sets variables true if found
            foreach (var v in _projectVersions)
            {
                foreach (var ps in v.ProjectEstimates)
                {
                    if (ps.Label == SnapshotLabel.Initial) hasInitial = true;
                    if (ps.Label == SnapshotLabel.Scope) hasScope = true;
                    if (ps.Label == SnapshotLabel.Phase1) hasPhase1 = true;

                    if (ps.Label == SnapshotLabel.Phase2) hasPhase2 = true;
                    if (ps.Label == SnapshotLabel.Phase3) hasPhase3 = true;
                    if (ps.Label == SnapshotLabel.Phase4) hasPhase4 = true;
                }
            }

            //From highest milestone bool down we check to see if that milestone exhist then return the next milestone.
            if (hasPhase4) return SnapshotLabel.Estimator;
            if (hasPhase3) return SnapshotLabel.Phase4;
            if (hasPhase2) return SnapshotLabel.Phase3;

            if (hasPhase1) return SnapshotLabel.Phase2;
            if (hasScope) return SnapshotLabel.Phase1;
            if (hasInitial) return SnapshotLabel.Scope;
            return SnapshotLabel.Initial;
        }

        protected internal virtual SnapshotLabel GetNextProposalSnapshotLabel()
        {
            //checks existing milestone flags and sets variables if flags have value
            var hasInitial = EstimateInitial.HasValue;
            var hasScope = EstimateScope.HasValue;
            var hasPhase1 = EstimatePhase1.HasValue;

            var hasPhase2 = EstimatePhase2.HasValue;
            var hasPhase3 = EstimatePhase3.HasValue;
            var hasPhase4 = EstimatePhase4.HasValue;
            var hasAuthorization = false;
            var hasOfficial = false;
            //checks all version estimates for milestones and sets variables true if found
            foreach (var v in _projectVersions)
            {
                foreach (var ps in v.ProjectEstimates)
                {
                    if (ps.Label == SnapshotLabel.Initial) hasInitial = true;
                    if (ps.Label == SnapshotLabel.Scope) hasScope = true;
                    if (ps.Label == SnapshotLabel.Phase1) hasPhase1 = true;

                    if (ps.Label == SnapshotLabel.Phase2) hasPhase2 = true;
                    if (ps.Label == SnapshotLabel.Phase3) hasPhase3 = true;
                    if (ps.Label == SnapshotLabel.Phase4) hasPhase4 = true;
                    if (ps.Label == SnapshotLabel.Authorization) hasAuthorization = true;
                    if (ps.Label == SnapshotLabel.Official) hasOfficial = true;
                }
            }
            //From highest milestone bool down we check to see if that milestone exist then return the next milestone.
            if (hasOfficial) return SnapshotLabel.Estimator;
            if (hasAuthorization) return SnapshotLabel.Official;
            if (hasPhase4) return SnapshotLabel.Authorization;
            if (hasPhase3) return SnapshotLabel.Phase4;
            if (hasPhase2) return SnapshotLabel.Phase3;

            if (hasPhase1) return SnapshotLabel.Phase2;
            if (hasScope) return SnapshotLabel.Phase1;
            if (hasInitial) return SnapshotLabel.Scope;
            return SnapshotLabel.Initial;

        }

        public virtual SnapshotLabel GetCurrentSnapshotLabel()
        {
            //checks existing milestone flags and sets variables if flags have value
            var hasInitial = EstimateInitial.HasValue;
            var hasScope = EstimateScope.HasValue;
            var hasPhase1 = EstimatePhase1.HasValue;

            var hasPhase2 = EstimatePhase2.HasValue;
            var hasPhase3 = EstimatePhase3.HasValue;
            var hasPhase4 = EstimatePhase4.HasValue;
            var hasAuthorization = false;
            var hasOfficial = false;
            //From highest milestone bool down we check to see if that milestone exist then return the next milestone.
            foreach (var v in _projectVersions)
            {
                foreach (var ps in v.ProjectEstimates)
                {
                    if (ps.Label == SnapshotLabel.Initial) hasInitial = true;
                    if (ps.Label == SnapshotLabel.Scope) hasScope = true;
                    if (ps.Label == SnapshotLabel.Phase1) hasPhase1 = true;

                    if (ps.Label == SnapshotLabel.Phase2) hasPhase2 = true;
                    if (ps.Label == SnapshotLabel.Phase3) hasPhase3 = true;
                    if (ps.Label == SnapshotLabel.Phase4) hasPhase4 = true;
                    if (ps.Label == SnapshotLabel.Authorization) hasAuthorization = true;
                    if (ps.Label == SnapshotLabel.Official) hasOfficial = true;
                }
            }
            //From highest milestone bool down we check to see if that milestone exist then return THAT milestone.
            if (hasOfficial) return SnapshotLabel.Official;
            if (hasAuthorization) return SnapshotLabel.Authorization;
            if (hasPhase4) return SnapshotLabel.Phase4;
            if (hasPhase3) return SnapshotLabel.Phase3;
            if (hasPhase2) return SnapshotLabel.Phase2;

            if (hasPhase1) return SnapshotLabel.Phase1;
            if (hasScope) return SnapshotLabel.Scope;
            if (hasInitial) return SnapshotLabel.Initial;
            return SnapshotLabel.Estimator;
        }

        public virtual string GetSnapshotLabelString(SnapshotLabel label)
        {
            return label == SnapshotLabel.Official ? "Official"
                    : label == SnapshotLabel.Authorization ? "Authorization"
                    : label == SnapshotLabel.Phase4 ? "Phase IV"
                    : label == SnapshotLabel.Phase3 ? "Phase III"
                    : label == SnapshotLabel.Phase2 ? "Phase II"
                    : label == SnapshotLabel.Phase1 ? "Phase I"
                    : label == SnapshotLabel.Scope ? "Scope"
                    : label == SnapshotLabel.Initial ? "Initial"
                    : string.Empty;
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
            if (account.MyRecentProjectEstimate.MyProjectVersion.MyProject != this)
            {
                throw new InvalidOperationException(string.Format("Working Estimate is for Project {0}", account.MyRecentProjectEstimate.MyProjectVersion.MyProject.ProjectNumber));
            }
            var prop = Proposals.FirstOrDefault(i => i.ProposalSource == ProposalSourceType.Wt);
            if (prop != null)
            {
                prop.CurrentEstimator = null;
            }
            foreach (var v in _projectVersions.Where(i => i.VersionOwner == account))
            {
                foreach (var ps in v.ProjectEstimates)
                {
                    ps.IsWorkingEstimate = false;
                }
            }
            var we = version.ProjectEstimates.SingleOrDefault(i => i.Estimate == version.ProjectEstimates.Max(ii => ii.Estimate));
            if (we == null)
            {
                throw new InvalidOperationException(string.Format("Max estimate was not found for Project {0} Version {1}", version.MyProject.ProjectNumber, version.Version));
            }
            we.IsWorkingEstimate = true;
            account.MyRecentProjectEstimate = we;
        }

        public virtual void ReleaseCustody(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator && account.Role != DqeRole.Coder)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (account.Role == DqeRole.System || account.Role == DqeRole.Administrator || account.Role == DqeRole.DistrictAdministrator || account.Role == DqeRole.Coder)
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
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator && account.Role != DqeRole.Coder)
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

        public virtual bool ProjectHasWorkingEstimateForUser(DqeUser dqeUser)
        {
            var userVersions = ProjectVersions.Where(i => i.VersionOwner == dqeUser).ToList();
            return userVersions.Count != 0;
        }

        public override void Transform(Transformers.Project transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator 
                && account.Role != DqeRole.DistrictAdministrator 
                && account.Role != DqeRole.Estimator 
                && account.Role != DqeRole.DistrictReviewer
                && account.Role != DqeRole.StateReviewer
                && account.Role != DqeRole.Coder
                && account.Role != DqeRole.MaintenanceDistrictAdmin
                && account.Role != DqeRole.MaintenanceEstimator
                && account.Role != DqeRole.AdminReadOnly

                )
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            ProjectNumber = transformer.ProjectNumber;
            Description = transformer.Description;
            DesignerName = transformer.DesignerName;
            District = transformer.District;
            //LettingDate = transformer.LettingDate;
            PseeContactSrsId = transformer.PseeContactSrsId;
            WtId = transformer.WtId;
            WtLsDbId = transformer.WtLsDbId;
            LsDbCode = transformer.LsDbCode;
            QuantityComplete = transformer.QuantityComplete;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //if (Proposals.Count(i => i.ProposalSource == ProposalSourceType.Wt) > 1)
            //{
            //    yield return new ValidationResult(string.Format("Only one Proposal associated to Project {0} can have a type of Web Trns*Port.", ProjectNumber));
            //}
            return new List<ValidationResult>();
        }

    }
}