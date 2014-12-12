using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Fdot;

namespace Dqe.Domain.Model
{
    public class ProjectEstimate : Entity<Transformers.ProjectEstimate>
    {
        private readonly ICollection<EstimateGroup> _estimateGroups;
        private readonly IWebTransportService _webTransportService;

        public ProjectEstimate(IWebTransportService webTransportService)
        {
            _estimateGroups = new Collection<EstimateGroup>();
            _webTransportService = webTransportService;
        }

        [Range(1, int.MaxValue)]
        public virtual int Estimate { get; protected internal set; }

        [StringLength(500)]
        public virtual string EstimateComment { get; protected internal set; }

        public virtual DateTime Created { get; protected internal set; }

        public virtual DateTime LastUpdated { get; set; }

        public virtual bool IsWorkingEstimate { get; set; }

        public virtual SnapshotLabel Label { get; protected internal set; }

        [Required]
        public virtual ProjectVersion MyProjectVersion { get; protected internal set; }

        public virtual IEnumerable<ItemGroup> GetItemGroups()
        {
            var l = new List<ProjectItem>();
            foreach (var estimateGroup in _estimateGroups.Where(i => !i.IsLsDbSummary))
            {
                l.AddRange(estimateGroup.ProjectItems);
            }
            var igl = new List<ItemGroup>();
            //combined category and items grouping
            var combinedCategoryAndItemKeys = l.Select(i => new
            {
                CombineCategories = i.MyEstimateGroup.CombineWithLikeItems,
                CategoryAlternateSet = i.MyEstimateGroup.AlternateSet,
                CategoryAlternateMember = i.MyEstimateGroup.AlternateMember,
                CategoryDescription = i.MyEstimateGroup.Description,
                i.MyEstimateGroup.FederalConstructionClass,
                CombineItems = i.CombineWithLikeItems,
                i.AlternateSet,
                i.AlternateMember,
                //i.Fund,
                i.SupplementalDescription,
                i.PayItemNumber,
                i.PayItemDescription,
                i.CalculatedUnit
            })
                .Where(i => i.CombineCategories)
                .Where(i => i.CombineItems)
                .Distinct()
                .ToList();
            foreach (var combinedCategoryAndItemKey in combinedCategoryAndItemKeys)
            {
                var key = combinedCategoryAndItemKey;
                var items = l
                    .Where(i => i.MyEstimateGroup.CombineWithLikeItems == key.CombineCategories)
                    .Where(i => i.MyEstimateGroup.AlternateSet == key.CategoryAlternateSet)
                    .Where(i => i.MyEstimateGroup.AlternateMember == key.CategoryAlternateMember)
                    .Where(i => i.MyEstimateGroup.Description == key.CategoryDescription)
                    .Where(i => i.MyEstimateGroup.FederalConstructionClass == key.FederalConstructionClass)
                    .Where(i => i.CombineWithLikeItems == key.CombineItems)
                    .Where(i => i.AlternateSet == key.AlternateSet)
                    .Where(i => i.AlternateMember == key.AlternateMember)
                    //.Where(i => i.Fund == key.Fund)
                    .Where(i => i.SupplementalDescription == key.SupplementalDescription)
                    .Where(i => i.PayItemNumber == key.PayItemNumber)
                    .Where(i => i.PayItemDescription == key.PayItemDescription)
                    .Where(i => i.CalculatedUnit == key.CalculatedUnit)
                    .Distinct()
                    .ToList();
                var ig = new ItemGroup
                {
                    CategoryAlternateMember = combinedCategoryAndItemKey.CategoryAlternateMember,
                    CategoryAlternateSet = combinedCategoryAndItemKey.CategoryAlternateSet,
                    CategoryDescription = combinedCategoryAndItemKey.CategoryDescription,
                    FederalConstructionClass = combinedCategoryAndItemKey.FederalConstructionClass,
                    //Fund = combinedCategoryAndItemKey.Fund,
                    ItemAlternateMember = combinedCategoryAndItemKey.AlternateMember,
                    ItemAlternateSet = combinedCategoryAndItemKey.AlternateSet,
                    ItemDescription = combinedCategoryAndItemKey.PayItemDescription,
                    ItemNumber = combinedCategoryAndItemKey.PayItemNumber,
                    SupplementalDescription = combinedCategoryAndItemKey.SupplementalDescription,
                    Unit = combinedCategoryAndItemKey.CalculatedUnit,
                    CombineCategories = true,
                    CombineItems = true
                };
                foreach (var projectItem in items)
                {
                    ig.ProjectItems.Add(projectItem);
                }
                igl.Add(ig);
            }
            //not combined category with combined items grouping
            var notCombinedCategoryAndCombinedItemKeys = l.Select(i => new
            {
                CategoryId = i.MyEstimateGroup.Id,
                CombineCategories = i.MyEstimateGroup.CombineWithLikeItems,
                CategoryAlternateSet = i.MyEstimateGroup.AlternateSet,
                CategoryAlternateMember = i.MyEstimateGroup.AlternateMember,
                CategoryDescription = i.MyEstimateGroup.Description,
                i.MyEstimateGroup.FederalConstructionClass,
                CombineItems = i.CombineWithLikeItems,
                i.AlternateSet,
                i.AlternateMember,
                //i.Fund,
                i.SupplementalDescription,
                i.PayItemNumber,
                i.PayItemDescription,
                i.CalculatedUnit
            })
                .Where(i => !i.CombineCategories)
                .Where(i => i.CombineItems)
                .Distinct()
                .ToList();
            foreach (var notCombinedCategoryAndCombinedItemKey in notCombinedCategoryAndCombinedItemKeys)
            {
                var key = notCombinedCategoryAndCombinedItemKey;
                var items = l
                    .Where(i => i.MyEstimateGroup.Id == notCombinedCategoryAndCombinedItemKey.CategoryId)
                    .Where(i => i.MyEstimateGroup.CombineWithLikeItems == key.CombineCategories)
                    .Where(i => i.MyEstimateGroup.AlternateSet == key.CategoryAlternateSet)
                    .Where(i => i.MyEstimateGroup.AlternateMember == key.CategoryAlternateMember)
                    .Where(i => i.MyEstimateGroup.Description == key.CategoryDescription)
                    .Where(i => i.MyEstimateGroup.FederalConstructionClass == key.FederalConstructionClass)
                    .Where(i => i.CombineWithLikeItems == key.CombineItems)
                    .Where(i => i.AlternateSet == key.AlternateSet)
                    .Where(i => i.AlternateMember == key.AlternateMember)
                    //.Where(i => i.Fund == key.Fund)
                    .Where(i => i.SupplementalDescription == key.SupplementalDescription)
                    .Where(i => i.PayItemNumber == key.PayItemNumber)
                    .Where(i => i.PayItemDescription == key.PayItemDescription)
                    .Where(i => i.CalculatedUnit == key.CalculatedUnit)
                    .Distinct()
                    .ToList();
                var ig = new ItemGroup
                {
                    CategoryAlternateMember = notCombinedCategoryAndCombinedItemKey.CategoryAlternateMember,
                    CategoryAlternateSet = notCombinedCategoryAndCombinedItemKey.CategoryAlternateSet,
                    CategoryDescription = notCombinedCategoryAndCombinedItemKey.CategoryDescription,
                    FederalConstructionClass = notCombinedCategoryAndCombinedItemKey.FederalConstructionClass,
                    //Fund = notCombinedCategoryAndCombinedItemKey.Fund,
                    ItemAlternateMember = notCombinedCategoryAndCombinedItemKey.AlternateMember,
                    ItemAlternateSet = notCombinedCategoryAndCombinedItemKey.AlternateSet,
                    ItemDescription = notCombinedCategoryAndCombinedItemKey.PayItemDescription,
                    ItemNumber = notCombinedCategoryAndCombinedItemKey.PayItemNumber,
                    SupplementalDescription = notCombinedCategoryAndCombinedItemKey.SupplementalDescription,
                    Unit = notCombinedCategoryAndCombinedItemKey.CalculatedUnit,
                    CombineCategories = false,
                    CombineItems = true
                };
                foreach (var projectItem in items)
                {
                    ig.ProjectItems.Add(projectItem);
                }
                igl.Add(ig);
            }
            //not combined items grouping
            var notCombinedItemKeys = l.Select(i => new
            {
                CombineCategories = i.MyEstimateGroup.CombineWithLikeItems,
                CategoryAlternateSet = i.MyEstimateGroup.AlternateSet,
                CategoryAlternateMember = i.MyEstimateGroup.AlternateMember,
                CategoryDescription = i.MyEstimateGroup.Description,
                i.MyEstimateGroup.FederalConstructionClass,
                ItemId = i.Id,
                CombineItems = i.CombineWithLikeItems,
                i.AlternateSet,
                i.AlternateMember,
                //i.Fund,
                i.SupplementalDescription,
                i.PayItemNumber,
                i.PayItemDescription,
                i.CalculatedUnit
            })
                .Where(i => !i.CombineItems)
                .Distinct()
                .ToList();
            foreach (var notCombinedItemKey in notCombinedItemKeys)
            {
                var key = notCombinedItemKey;
                var items = l
                    .Where(i => i.MyEstimateGroup.CombineWithLikeItems == key.CombineCategories)
                    .Where(i => i.MyEstimateGroup.AlternateSet == key.CategoryAlternateSet)
                    .Where(i => i.MyEstimateGroup.AlternateMember == key.CategoryAlternateMember)
                    .Where(i => i.MyEstimateGroup.Description == key.CategoryDescription)
                    .Where(i => i.MyEstimateGroup.FederalConstructionClass == key.FederalConstructionClass)
                    .Where(i => i.Id == key.ItemId)
                    .Where(i => i.CombineWithLikeItems == key.CombineItems)
                    .Where(i => i.AlternateSet == key.AlternateSet)
                    .Where(i => i.AlternateMember == key.AlternateMember)
                    //.Where(i => i.Fund == key.Fund)
                    .Where(i => i.SupplementalDescription == key.SupplementalDescription)
                    .Where(i => i.PayItemNumber == key.PayItemNumber)
                    .Where(i => i.PayItemDescription == key.PayItemDescription)
                    .Where(i => i.CalculatedUnit == key.CalculatedUnit)
                    .Distinct()
                    .ToList();
                foreach (var projectItem in items)
                {
                    var ig = new ItemGroup
                    {
                        CategoryAlternateMember = notCombinedItemKey.CategoryAlternateMember,
                        CategoryAlternateSet = notCombinedItemKey.CategoryAlternateSet,
                        CategoryDescription = notCombinedItemKey.CategoryDescription,
                        FederalConstructionClass = notCombinedItemKey.FederalConstructionClass,
                        //Fund = notCombinedItemKey.Fund,
                        ItemAlternateMember = notCombinedItemKey.AlternateMember,
                        ItemAlternateSet = notCombinedItemKey.AlternateSet,
                        ItemDescription = notCombinedItemKey.PayItemDescription,
                        ItemNumber = notCombinedItemKey.PayItemNumber,
                        SupplementalDescription = notCombinedItemKey.SupplementalDescription,
                        Unit = notCombinedItemKey.CalculatedUnit,
                        CombineCategories = notCombinedItemKey.CombineCategories,
                        CombineItems = false
                    };
                    ig.ProjectItems.Add(projectItem);
                    igl.Add(ig);
                }
            }
            //TEST
            //var groupedItemCount = igl.Sum(ig => ig.ProjectItems.Count);
            //var areAllItemsGrouped = groupedItemCount == l.Count;
            return igl;
        } 

        public virtual IEnumerable<EstimateGroup> EstimateGroups
        {
            get { return _estimateGroups.ToList().AsReadOnly(); }
        }

        public virtual EstimateTotal GetEstimateTotal()
        {
            var estimateTotal = new EstimateTotal();
            var categorySets = EstimateGroups.Select(i => i.AlternateSet).Distinct().ToList();
            foreach (var categorySet in categorySets)
            {
                var categories = EstimateGroups.Where(i => i.AlternateSet == categorySet).ToList();
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
                    var allItems = new List<ProjectItem>();
                    foreach (var category in memberCategories)
                    {
                        allItems.AddRange(category.ProjectItems);
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
                                Total = memberItems.Sum(i => i.Quantity * i.Price)
                            };
                            cSet.ItemSets.Add(iSet);
                        }
                    }
                }
            }
            return estimateTotal;
        }

        public virtual bool IsSyncedWithWt()
        {
            return _webTransportService.IsProjectSynced(this);
        }

        public virtual void SyncWithWt(DqeUser account)
        {
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (MyProjectVersion.VersionOwner != account)
            {
                throw new InvalidOperationException(string.Format("{0} is not the owner of Project {1} Version {2} Estimate {3}", account.Name, MyProjectVersion.MyProject.ProjectNumber, MyProjectVersion.Version, Estimate));
            }
            var project = _webTransportService.ExportProject(MyProjectVersion.MyProject.ProjectNumber);
            if (project == null) return;
            var newEstimate = MyProjectVersion.MyProject.CreateNewVersionFromWt("Synchronized with Web Trns*Port design changes", project, account);
            foreach (var estimateGroup in EstimateGroups.Where(i => !i.IsLsDbSummary))
            {
                var eg = estimateGroup;
                var egMatch = newEstimate
                    .EstimateGroups
                    //.Where(i => i.CombineWithLikeItems == eg.CombineWithLikeItems)
                    .Where(i => i.AlternateMember == eg.AlternateMember)
                    .Where(i => i.AlternateSet == eg.AlternateSet)
                    .Where(i => i.Name == eg.Name)
                    .Where(i => i.Description == eg.Description)
                    .Where(i => i.FederalConstructionClass == eg.FederalConstructionClass)
                    .FirstOrDefault(i => i.WtId == eg.WtId);
                if (egMatch != null)
                {
                    foreach (var pItem in estimateGroup.ProjectItems)
                    {
                        var pi = pItem;
                        var piMatch = egMatch
                            .ProjectItems
                            //.Where(i => i.CombineWithLikeItems == pi.CombineWithLikeItems)
                            .Where(i => i.AlternateMember == pi.AlternateMember)
                            .Where(i => i.AlternateSet == pi.AlternateSet)
                            .Where(i => i.PayItemNumber == pi.PayItemNumber)
                            .Where(i => i.Fund == pi.Fund)
                            .Where(i => i.SupplementalDescription == pi.SupplementalDescription)
                            .Where(i => i.CalculatedUnit == pi.CalculatedUnit)
                            .FirstOrDefault(i => i.WtId == pi.WtId);
                        if (piMatch != null)
                        {
                            piMatch.Price = pi.Price;
                            piMatch.PriceSet = pi.PriceSet;
                        }
                    }    
                }
            }
            //set group prices
            var igl = GetItemGroups()
                .OrderBy(i => i.FederalConstructionClass)
                .ThenBy(i => i.CategoryDescription)
                .ThenBy(i => i.ItemNumber)
                .ThenBy(i => i.CategoryAlternateSet)
                .ThenBy(i => i.CategoryAlternateMember)
                .ThenBy(i => i.ItemAlternateSet)
                .ThenBy(i => i.ItemAlternateMember)
                .ThenBy(i => i.SupplementalDescription)
                .ThenBy(i => i.Fund);
            foreach (var itemGroup in igl)
            {
                var pis = itemGroup.ProjectItems.Where(i => i.Price > 0).ToList();
                var average = pis.Count == 0 
                    ? 0 
                    : pis.Sum(i => i.Price) / pis.Count();
                foreach (var item in itemGroup.ProjectItems)
                {
                    item.Price = average;
                }
            }
        }

        public virtual void SetComment(string comment, DqeUser account)
        {
            if (comment == null) throw new ArgumentNullException("comment");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (MyProjectVersion.VersionOwner != account)
            {
                throw new InvalidOperationException(string.Format("{0} is not the owner of Project {1} Version {2} Estimate {3}", account.Name, MyProjectVersion.MyProject.ProjectNumber, MyProjectVersion.Version, Estimate));
            }
            EstimateComment = comment;
        }

        protected internal virtual void AddEstimateGroup(EstimateGroup estimateGroup)
        {
            _estimateGroups.Add(estimateGroup);
            estimateGroup.MyProjectEstimate = this;
        }

        public override Transformers.ProjectEstimate GetTransformer()
        {
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProjectEstimate transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}