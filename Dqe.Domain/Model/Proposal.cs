using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class Proposal : Entity<Transformers.Proposal>
    {
        private readonly ICollection<ProjectVersion> _projectVersions;
        private readonly ICollection<Project> _projects;
        private readonly ICollection<SectionGroup> _sectionGroups;
        private readonly ICollection<DqeUser> _users; 
        private readonly IProposalRepository _proposalRepository;

        public Proposal(IProposalRepository proposalRepository)
        {
            _projectVersions = new Collection<ProjectVersion>();
            _projects = new Collection<Project>();
            _sectionGroups = new Collection<SectionGroup>();
            _users = new Collection<DqeUser>();
            _proposalRepository = proposalRepository;
        }

        [Required]
        public virtual string ProposalNumber { get; protected internal set; }

        public virtual ProposalSourceType ProposalSource { get; protected internal set; }

        [StringLength(500)]
        public virtual string Comment { get; protected internal set; }

        [Range(1, int.MaxValue)]
        public virtual long WtId { get; protected internal set; }

        public virtual DateTime? LettingDate { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; protected internal set; }

        public virtual DqeUser CurrentEstimator { get; protected internal set; }

        public virtual void SetCurrentEstimator(DqeUser dqeUser)
        {
            CurrentEstimator = dqeUser;
        }

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

        public virtual IEnumerable<DqeUser> Users
        {
            get { return _users.ToList().AsReadOnly(); }
        } 

        protected internal virtual void AddProject(Project project)
        {
            if (_projects.Contains(project)) return;
            _projects.Add(project);
        }

        public virtual SnapshotLabel GetCurrentSnapshotLabel()
        {
            if (ProposalSource != ProposalSourceType.Wt) return SnapshotLabel.Estimator;
            if (!Projects.Any()) return SnapshotLabel.Estimator;
            var dict = Projects.ToDictionary(project => project, project => project.GetCurrentSnapshotLabel());
            var review = dict.All(i => i.Value == SnapshotLabel.Review);
            var initial = dict.All(i => i.Value == SnapshotLabel.Initial);
            var scope = dict.All(i => i.Value == SnapshotLabel.Scope);
            var phase1 = dict.All(i => i.Value == SnapshotLabel.Phase1);
            var phase2 = dict.All(i => i.Value == SnapshotLabel.Phase2);
            var phase3 = dict.All(i => i.Value == SnapshotLabel.Phase3);
            var phase4 = dict.All(i => i.Value == SnapshotLabel.Phase4);
            var authorization = dict.All(i => i.Value == SnapshotLabel.Authorization);
            var official = dict.All(i => i.Value == SnapshotLabel.Official);
            if (official)
            {
                return SnapshotLabel.Official;
            }
            if (authorization)
            {
                return SnapshotLabel.Authorization;
            }
            if (phase4)
            {
                return SnapshotLabel.Phase4;
            }
            if (phase3)
            {
                return SnapshotLabel.Phase3;
            }
            if (phase2)
            {
                return SnapshotLabel.Phase2;
            }
            if (phase1)
            {
                return SnapshotLabel.Phase1;
            }
            if (scope)
            {
                return SnapshotLabel.Scope;
            }
            if (initial)
            {
                return SnapshotLabel.Initial;
            }
            if (review)
            {
                return SnapshotLabel.Review;
            }

            return SnapshotLabel.Estimator;
            //var inSync = dict.All(i => i.Value == dict.First().Value);
            //return inSync ? SnapshotLabel.Estimator : SnapshotLabel.Estimator;
        }

        public virtual SnapshotLabel GetGreatesUnleveledCurrentSnapshotLabel()
        {
            if (ProposalSource != ProposalSourceType.Wt) return SnapshotLabel.Estimator;
            var dict = Projects.ToDictionary(project => project, project => project.GetCurrentSnapshotLabel());
            var official = dict.Any(i => i.Value == SnapshotLabel.Official);
            var authorization = dict.Any(i => i.Value == SnapshotLabel.Authorization);
            var phase4 = dict.Any(i => i.Value == SnapshotLabel.Phase4);
            var phase3 = dict.Any(i => i.Value == SnapshotLabel.Phase3);
            var phase2 = dict.Any(i => i.Value == SnapshotLabel.Phase2);
            var phase1 = dict.All(i => i.Value == SnapshotLabel.Phase1);
            var scope = dict.All(i => i.Value == SnapshotLabel.Scope);
            var initial = dict.All(i => i.Value == SnapshotLabel.Initial);
            var review = dict.All(i => i.Value == SnapshotLabel.Review);
            if (official)
            {
                return SnapshotLabel.Official;
            }
            if (authorization)
            {
                return SnapshotLabel.Authorization;
            }
            if (phase4)
            {
                return SnapshotLabel.Phase4;
            }
            if (phase3)
            {
                return SnapshotLabel.Phase3;
            }
            if (phase2)
            {
                return SnapshotLabel.Phase2;
            }
            if (phase1)
            {
                return SnapshotLabel.Phase1;
            }
            if (scope)
            {
                return SnapshotLabel.Scope;
            }
            if (initial)
            {
                return SnapshotLabel.Initial;
            }
            if (review)
            {
                return SnapshotLabel.Review;
            }
            return SnapshotLabel.Estimator;
            //var inSync = dict.All(i => i.Value == dict.First().Value);
            //return inSync ? SnapshotLabel.Estimator : SnapshotLabel.Estimator;
        }

        public virtual SnapshotLabel GetNextSnapshotLabel()
        {
            if (ProposalSource != ProposalSourceType.Wt) return SnapshotLabel.Estimator;
            var dict = Projects.ToDictionary(project => project, project => project.GetCurrentSnapshotLabel());
            var review = dict.All(i => i.Value == SnapshotLabel.Review);
            var initial = dict.All(i => i.Value == SnapshotLabel.Initial);
            var scope = dict.All(i => i.Value == SnapshotLabel.Scope);
            var phase1 = dict.All(i => i.Value == SnapshotLabel.Phase1);
            var phase2 = dict.All(i => i.Value == SnapshotLabel.Phase2);
            var phase3 = dict.All(i => i.Value == SnapshotLabel.Phase3);
            var phase4 = dict.All(i => i.Value == SnapshotLabel.Phase4);
            var authorization = dict.All(i => i.Value == SnapshotLabel.Authorization);
            var official = dict.All(i => i.Value == SnapshotLabel.Official);
            if (official)
            {
                return SnapshotLabel.Estimator;
            }
            if (authorization)
            {
                return SnapshotLabel.Official;
            }
            if (phase4)
            {
                return SnapshotLabel.Authorization;
            }
            if (phase3)
            {
                return SnapshotLabel.Phase4;
            }
            if (phase2)
            {
                return SnapshotLabel.Phase3;
            }
            if (phase1)
            {
                return SnapshotLabel.Phase1;
            }
            if (scope)
            {
                return SnapshotLabel.Scope;
            }
            if (initial)
            {
                return SnapshotLabel.Initial;
            }
            if (review)
            {
                return SnapshotLabel.Review;
            }
            //return SnapshotLabel.Phase2;
            var inSync = dict.All(i => i.Value == dict.First().Value);
            return inSync ? SnapshotLabel.Phase2 : SnapshotLabel.Estimator;
        }

        //public virtual void SnapshotWorkingEstimate(DqeUser account, bool labelSnapshot)
        //{
        //    if (account == null) throw new ArgumentNullException("account");
        //    if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
        //    {
        //        throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
        //    }
        //    //var next = GetNextSnapshotLabel();
        //    //foreach (var project in Projects)
        //    //{
        //    //    project.RemoveLabel(next);
        //    //}
        //    foreach (var project in Projects)
        //    {
        //        project.SnapshotWorkingEstimate(account, labelSnapshot, true);
        //    }
        //}

        public virtual void RemoveProject(Project project)
        {
            //clear proposal items
            foreach (var sectionGroup in _sectionGroups)
            {
                foreach (var proposalItem in sectionGroup.ProposalItems)
                {
                    proposalItem.ClearProjectItems();
                }
            }
            _projects.Remove(project);
            project.RemoveProposal(this);
        }

        public virtual void RemoveProjects()
        {
            //clear proposal items
            foreach (var project in _projects)
            {
                foreach (var sectionGroup in _sectionGroups)
                {
                    foreach (var proposalItem in sectionGroup.ProposalItems)
                    {
                        proposalItem.ClearProjectItems();
                    }
                }
                project.RemoveProposal(this);
            }
            _projects.Clear();
        }

        public virtual void ConvertToGaming()
        {
            ProposalSource = ProposalSourceType.Gaming;
            Comment = "A new Proposal was associated to one or more Projects in Project Preconstruction that have DQE estimates.  This Proposal was converted to a Gaming Proposal.";
            LastUpdated = DateTime.Now;
        }

        public virtual bool SynchronizeStructure(Wt.Proposal wtp, DqeUser custodyUser, bool isOfficialTransfer)
        {
            //_proposalRepository.DeleteProposalStructure(Id);

            
            WtId = wtp.Id;
            

            var l = SectionGroups.ToList();
            foreach (var sectionGroup in l)
            {
                foreach (var proposalItem in sectionGroup.ProposalItems)
                {
                    foreach (var projectItem in proposalItem.ProjectItems)
                    {
                        projectItem.MyProposalItem = null;
                    }
                }
            }
            foreach (var sectionGroup in l)
            {
                _sectionGroups.Remove(sectionGroup);
            }
            var projects = wtp.Projects.Select(i => i.ProjectNumber).Distinct().ToArray();
            var projectItems = isOfficialTransfer 
                ? _proposalRepository.GetDqeProjectItemsForOfficialProposal(projects).ToList() 
                : _proposalRepository.GetDqeProjectItemsForProposal(custodyUser, projects).ToList();
            foreach (var s in wtp.Sections)
            {
                var section = new SectionGroup
                {
                    AlternateMember = string.IsNullOrWhiteSpace(s.AlternateMember) ? string.Empty : s.AlternateMember,
                    AlternateSet = string.IsNullOrWhiteSpace(s.AlternateSet) ? string.Empty : s.AlternateSet,
                    Description = s.Description,
                    MyProposal = this,
                    Name = s.Name,
                    WtId = s.Id
                };
                _sectionGroups.Add(section);
                foreach (var i in s.ProposalItems)
                {
                    var proposalItem = new ProposalItem
                    {
                        AlternateMember = string.IsNullOrWhiteSpace(i.AlternateMember) ? string.Empty : i.AlternateMember,
                        AlternateSet = string.IsNullOrWhiteSpace(i.AlternateSet) ? string.Empty : i.AlternateSet,
                        CalculatedUnit = i.MyRefItem.CalculatedUnit,
                        MySectionGroup = section,
                        PayItemDescription = i.MyRefItem.Description,
                        PayItemNumber = i.MyRefItem.Name,
                        Quantity = i.Quantity,
                        SupplementalDescription = string.IsNullOrWhiteSpace(i.SupplementalDescription) ? string.Empty : i.SupplementalDescription,
                        Unit = i.MyRefItem.Unit,
                        WtId = i.Id
                    };
                    section.AddProposalItem(proposalItem);
                    foreach (var pi in i.ProjectItems)
                    {
                        //TODO: this currently rebuilds the proposal only for the current estimator - should all estimators be rebuilt
                        //var projectItem = _proposalRepository.GetProjectItemByWtId(pi.Id, custodyUser);
                        var projectItem = projectItems.FirstOrDefault(ii => ii.WtId == pi.Id);
                        if (projectItem == null)
                        {
                            Console.WriteLine("wT Proposal {0} Section {1} Item {2} with ID {3} was not found in DQE.", wtp.ProposalNumber, s.Name, i.MyRefItem.Name, i.Id);
                            return false;
                        }
                        proposalItem.AddProjectItem(projectItem);
                    }
                }
            }
            return true;
        }

        //public virtual IEnumerable<EstimateGroup> GetEstimateGroups(DqeUser dqeUser)
        //{
        //    var versions = Projects.Select(i => i.ProjectVersions.First(ii => ii.VersionOwner == dqeUser)).Distinct();
        //    var estimates = versions.Select(i => i.ProjectEstimates.First(ii => ii.IsWorkingEstimate)).Distinct();
        //    var groups = new List<EstimateGroup>();
        //    foreach (var estimate in estimates)
        //    {
        //        groups.AddRange(estimate.EstimateGroups);
        //    }
        //    return groups.Distinct();
        //}

        public virtual EstimateTotal GetEstimateTotal(DqeUser estimator)
        {
            var estimateTotal = new EstimateTotal();
            var categorySets = SectionGroups.Select(i => i.AlternateSet).Distinct().ToList();
            foreach (var categorySet in categorySets)
            {
                var categories = SectionGroups.Where(i => i.AlternateSet == categorySet).ToList();
                var categoryMembers = categories.Select(i => i.AlternateMember).Distinct().ToList();
                foreach (var categoryMember in categoryMembers)
                {
                    var cSet = new CategorySet
                    {
                        Set = categorySet,
                        Member = categoryMember
                    };
                    estimateTotal.CategorySets.Add(cSet);
                    var memberCategories = categories.Where(i => i.AlternateMember == categoryMember).ToList();
                    var allItems = new List<ProposalItem>();
                    foreach (var category in memberCategories)
                    {
                        allItems.AddRange(category.ProposalItems);
                    }
                    var itemSets = allItems.Select(i => i.AlternateSet).Distinct().ToList();
                    foreach (var itemSet in itemSets)
                    {
                        var items = allItems.Where(i => i.AlternateSet == itemSet).ToList();
                        var itemMembers = items.Select(i => i.AlternateMember).Distinct().ToList();
                        foreach (var itemMember in itemMembers)
                        {
                            var memberItems = items.Where(i => i.AlternateMember == itemMember).ToList();
                            var iSet = new ItemSet
                            {
                                Set = itemSet,
                                Member = itemMember,
                                //Total = memberItems.Sum(i => i.ProjectItems.Sum(ii => ii.Quantity * ii.Price))

                                Total = memberItems.Sum(i => i.GetEstimatorProjectItems(estimator).Sum(ii => Math.Round(ii.Quantity * ii.Price, 2, MidpointRounding.AwayFromZero)))
                            };
                            cSet.ItemSets.Add(iSet);
                        }
                    }
                }
            }
            return estimateTotal;
        }

        public virtual EstimateTotal GetEstimateTotalWithItems(DqeUser estimator)
        {
            var estimateTotal = new EstimateTotal();
            var categorySets = SectionGroups.Select(i => i.AlternateSet).Distinct().ToList();
            foreach (var categorySet in categorySets)
            {
                var categories = SectionGroups.Where(i => i.AlternateSet == categorySet).ToList();
                var categoryMembers = categories.Select(i => i.AlternateMember).Distinct().ToList();
                foreach (var categoryMember in categoryMembers)
                {
                    var memberCategories = categories.Where(i => i.AlternateMember == categoryMember).ToList();
                    var cSet = new CategorySet
                    {
                        Set = categorySet,
                        Member = categoryMember,
                        SectionGroups = memberCategories
                    };
                    estimateTotal.CategorySets.Add(cSet);
                    var allItems = new List<ProposalItem>();
                    foreach (var category in memberCategories)
                    {
                        allItems.AddRange(category.ProposalItems);
                    }
                    var itemSets = allItems.Select(i => i.AlternateSet).Distinct().ToList();
                    foreach (var itemSet in itemSets)
                    {
                        var items = allItems.Where(i => i.AlternateSet == itemSet).ToList();
                        var itemMembers = items.Select(i => i.AlternateMember).Distinct().ToList();
                        foreach (var itemMember in itemMembers)
                        {
                            var memberItems = items.Where(i => i.AlternateMember == itemMember).ToList();
                            var iSet = new ItemSet
                            {
                                Set = itemSet,
                                Member = itemMember,
                                ProposalItems = memberItems,
                                Total = memberItems.Sum(i => i.GetEstimatorProjectItems(estimator).Sum(ii => Math.Round(ii.Quantity * ii.Price, 2, MidpointRounding.AwayFromZero)))
                            };
                            cSet.ItemSets.Add(iSet);
                        }
                    }
                }
            }
            return estimateTotal;
        }

        //public virtual IEnumerable<ItemGroup> GetItemGroups()
        //{
        //    var l = new List<ProposalItem>();
        //    foreach (var estimateGroup in _sectionGroups)
        //    {
        //        l.AddRange(estimateGroup.ProposalItems);
        //    }
        //    var igl = new List<ItemGroup>();
        //    //combined category and items grouping
        //    var combinedCategoryAndItemKeys = l.Select(i => new
        //    {
        //        CombineCategories = true,
        //        CategoryAlternateSet = i.MySectionGroup.AlternateSet,
        //        CategoryAlternateMember = i.MySectionGroup.AlternateMember,
        //        CategoryDescription = i.MySectionGroup.Description,
        //        i.MySectionGroup.Name,
        //        CombineItems = true,
        //        i.AlternateSet,
        //        i.AlternateMember,
        //        //i.Fund,
        //        i.SupplementalDescription,
        //        i.PayItemNumber,
        //        i.PayItemDescription,
        //        i.CalculatedUnit
        //    })
        //        .Distinct()
        //        .ToList();
        //    foreach (var combinedCategoryAndItemKey in combinedCategoryAndItemKeys)
        //    {
        //        var key = combinedCategoryAndItemKey;
        //        var items = l
        //            .Where(i => i.MySectionGroup.AlternateSet == key.CategoryAlternateSet)
        //            .Where(i => i.MySectionGroup.AlternateMember == key.CategoryAlternateMember)
        //            .Where(i => i.MySectionGroup.Description == key.CategoryDescription)
        //            .Where(i => i.MySectionGroup.Name == key.Name)
        //            .Where(i => i.AlternateSet == key.AlternateSet)
        //            .Where(i => i.AlternateMember == key.AlternateMember)
        //            //.Where(i => i.Fund == key.Fund)
        //            .Where(i => i.SupplementalDescription == key.SupplementalDescription)
        //            .Where(i => i.PayItemNumber == key.PayItemNumber)
        //            .Where(i => i.PayItemDescription == key.PayItemDescription)
        //            .Where(i => i.CalculatedUnit == key.CalculatedUnit)
        //            .Distinct()
        //            .ToList();
        //        var ig = new ItemGroup
        //        {
        //            CategoryAlternateMember = combinedCategoryAndItemKey.CategoryAlternateMember,
        //            CategoryAlternateSet = combinedCategoryAndItemKey.CategoryAlternateSet,
        //            CategoryDescription = combinedCategoryAndItemKey.CategoryDescription,
        //            FederalConstructionClass = combinedCategoryAndItemKey.Name,
        //            Fund = string.Empty,
        //            ItemAlternateMember = combinedCategoryAndItemKey.AlternateMember,
        //            ItemAlternateSet = combinedCategoryAndItemKey.AlternateSet,
        //            ItemDescription = combinedCategoryAndItemKey.PayItemDescription,
        //            ItemNumber = combinedCategoryAndItemKey.PayItemNumber,
        //            SupplementalDescription = combinedCategoryAndItemKey.SupplementalDescription,
        //            Unit = combinedCategoryAndItemKey.CalculatedUnit,
        //            CombineCategories = true,
        //            CombineItems = true
        //        };
        //        foreach (var proposalItem in items)
        //        {
        //            foreach (var projectItem in proposalItem.ProjectItems)
        //            {
        //                if (ig.Fund != "MIXED")
        //                {
        //                    if (string.IsNullOrWhiteSpace(ig.Fund))
        //                    {
        //                        ig.Fund = projectItem.Fund;
        //                    }
        //                    else if (ig.Fund != projectItem.Fund)
        //                    {
        //                        ig.Fund = "MIXED";
        //                    }    
        //                }
        //                ig.ProjectItems.Add(projectItem);    
        //            }
        //        }
        //        igl.Add(ig);
        //    }
        //    return igl;
        //}

        public virtual IEnumerable<ItemGroup> GetItemGroups()
        {
            var l = new List<ProposalItem>();
            foreach (var estimateGroup in SectionGroups)
            {
                l.AddRange(estimateGroup.ProposalItems);
            }
            var igl = new List<ItemGroup>();
            foreach (var proposalItem in l)
            {
                var ig = new ItemGroup
                {
                    CategoryAlternateMember = proposalItem.MySectionGroup.AlternateMember,
                    CategoryAlternateSet = proposalItem.MySectionGroup.AlternateSet,
                    CategoryDescription = proposalItem.MySectionGroup.Description,
                    FederalConstructionClass = !proposalItem.ProjectItems.Any() 
                    ? string.Empty
                    : proposalItem.ProjectItems.Count() == 1
                        ? proposalItem.ProjectItems.ToList()[0].MyEstimateGroup.Name
                        : string.Format("{0}XX", proposalItem.ProjectItems.ToList()[0].MyEstimateGroup.Name.Substring(0, 2)),
                    //proposalItem.MySectionGroup.Name,
                    Fund = string.Empty,
                    ItemAlternateMember = proposalItem.AlternateMember,
                    ItemAlternateSet = proposalItem.AlternateSet,
                    ItemDescription = proposalItem.PayItemDescription,
                    ItemNumber = proposalItem.PayItemNumber,
                    SupplementalDescription = proposalItem.SupplementalDescription,
                    Unit = proposalItem.CalculatedUnit,
                    //Unit = proposalItem.CalculatedUnit.ToUpper().StartsWith("LS")
                    //    ? proposalItem.Unit.ToUpper().Trim() == "LS"
                    //        ? proposalItem.CalculatedUnit
                    //        : string.Format("{0}/{1}", proposalItem.Unit, proposalItem.CalculatedUnit)
                    //    : proposalItem.CalculatedUnit,
                    CombineCategories = true,
                    CombineItems = true
                };
                foreach (var projectItem in proposalItem.ProjectItems)
                {
                    if (ig.Fund != "MIXED")
                    {
                        if (string.IsNullOrWhiteSpace(ig.Fund))
                        {
                            ig.Fund = projectItem.Fund;
                        }
                        else if (ig.Fund != projectItem.Fund)
                        {
                            ig.Fund = "MIXED";
                        }
                    }
                    ig.ProjectItems.Add(projectItem);
                }
                igl.Add(ig);
            }
            return igl;
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
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator && account.Role != DqeRole.Coder)
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