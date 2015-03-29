using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Wt;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories;
using Dqe.Infrastructure.Repositories.Custom;
using NHibernate.Criterion;
using NHibernate.Linq;
using County = Dqe.Domain.Model.Wt.County;
using Project = Dqe.Domain.Model.Wt.Project;
using ProjectItem = Dqe.Domain.Model.Wt.ProjectItem;
using Proposal = Dqe.Domain.Model.Wt.Proposal;
using ProposalItem = Dqe.Domain.Model.Wt.ProposalItem;

namespace Dqe.Infrastructure.Fdot
{
    public class WebTransportService : IWebTransportService
    {

        private static readonly IDictionary<string, CodeTable> CodeTables = new Dictionary<string, CodeTable>(); 

        private static readonly object Lock = new object();

        public IEnumerable<CodeTable> GetCodeTables()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session.QueryOver<CodeTable>()
                        .OrderBy(i => i.CodeTableName).Asc
                        .Fetch(i => i.CodeValues).Eager
                        .List()
                        .Distinct();
            }
        }

        public CodeTable GetCodeTable(string codeType)
        {
            lock (Lock)
            {
                if (CodeTables.ContainsKey(codeType))
                {
                    return CodeTables[codeType];
                }
                using (var session = Initializer.TransportSessionFactory.OpenSession())
                {
                    var ct = session.QueryOver<CodeTable>()
                        .Where(i => i.CodeTableName == codeType)
                        .Fetch(i => i.CodeValues).Eager
                        .SingleOrDefault();
                    CodeTables.Add(codeType, ct);
                    return ct;
                }
            }
        }

        public IEnumerable<RefItem> GetRefItems()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .List()
                    .OrderBy(i => i.Name)
                    .ToList();
            }
        }

        public IEnumerable<string> GetDistinctRefItemNumbers()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .Select(Projections.Distinct(Projections.Property<RefItem>(i => i.Name)))
                    .OrderBy(i => i.Name).Asc
                    .List<string>();
            }
        }

        public void ConvertPayItems()
        {
            var sys = new DqeUserRepository().GetSystemAccount();
            var mfl = new MasterFileRepository().GetAll().ToList();
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Console.WriteLine("Loading All Pay Items");
                var allItems = session.QueryOver<RefItem>().List();
                //hybred test
                //var hybrids = allItems.Where(i => i.BidAsLumpSum).Where(i => i.Unit != "LS").ToList();
                //end test
                Console.WriteLine("Processing");
                foreach (var wtpi in allItems)
                {
                    var pim = new PayItemMaster();
                    var pimt = pim.GetTransformer();
                    pimt.Administrative = wtpi.Administrative;
                    pimt.AlternateItemName = wtpi.AlternateItemName;
                    pimt.AutoPaidPercentSchedule = wtpi.AutoPaidPercentSchedule;
                    pimt.BidAsLumpSum = wtpi.BidAsLumpSum;
                    pimt.BidRequirementCode = wtpi.BidRequirementCode;
                    pimt.CalculatedUnit = wtpi.CalculatedUnit;
                    pimt.CoApprovalRequired = wtpi.CoApprovalRequired;
                    pimt.CombineWithLikeItems = wtpi.CombineWithLikeItems;
                    pimt.CommonUnit = wtpi.CommonUnit;
                    pimt.ContractClass = wtpi.ContractClass;
                    pimt.ConversionFactorToCommonUnit = wtpi.ConversionFactorToCommonUnits;
                    pimt.CreatedBy = wtpi.CreatedBy;
                    pimt.CreatedDate = wtpi.CreatedDate;
                    pimt.DbeInterest = wtpi.DbeInterest;
                    pimt.DbePercentToApply = wtpi.DbePercentToApply;
                    pimt.Description = wtpi.Description;
                    pimt.ExemptFromMaa = wtpi.ExemptFromMaa;
                    pimt.ExemptFromRetainage = wtpi.ExemptFromRetainage;
                    pimt.FuelAdjustment = wtpi.FuelAdjustment;
                    pimt.FuelAdjustmentType = wtpi.FuelAdjustmentType;
                    pimt.EffectiveDate = wtpi.IlDate1;
                    pimt.Ildt2 = wtpi.IlDate2;
                    pimt.OpenedDate = wtpi.IlDate3;
                    pimt.Ilflg1 = wtpi.IlFlag1;
                    pimt.Illst1 = wtpi.Illst1;
                    pimt.Ilnum1 = wtpi.IlNumber1;
                    pimt.Ilsst1 = wtpi.Ilsst1;
                    pimt.ItemClass = wtpi.ItemClass;
                    pimt.ItemType = wtpi.ItemType;
                    pimt.LastUpdatedBy = wtpi.LastUpdatedBy;
                    pimt.LastUpdatedDate = wtpi.LastUpdatedDate;
                    pimt.LumpSum = wtpi.LumpSum;
                    pimt.MajorItem = wtpi.MajorItem;
                    pimt.NonBid = wtpi.NonBid;
                    pimt.ObsoleteDate = wtpi.ObsoleteDate;
                    pimt.PayPlan = wtpi.PayPlan;
                    pimt.PercentScheduleItem = wtpi.PercentScheduleItem;
                    pimt.RecordSource = wtpi.RecordSource;
                    pimt.RefItemName = wtpi.Name;
                    pimt.RefPrice = wtpi.Price;
                    pimt.RegressionInclusion = wtpi.RegressionInclusion;
                    pimt.ShortDescription = wtpi.ShortDescription;
                    pimt.SpecialtyItem = wtpi.SpecialtyItem;
                    pimt.SuppDescriptionRequired = wtpi.SuppDescriptionRequired;
                    pimt.Unit = wtpi.Unit;
                    pimt.UnitSystem = wtpi.UnitSystem;
                    pim.Transform(pimt, sys);
                    var mf = mfl.First(i => i.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0') == wtpi.SpecBook);
                    mf.AddPayItemMaster(pim);
                }
            }
            Console.WriteLine("Committing");
            UnitOfWorkProvider.TransactionManager.Commit();
        }

        public IEnumerable<RefItem> GetRefItems(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .Where(i => i.Name == number)
                    .List();
            }
        }

        public IEnumerable<RefItem> GetRefItemsBySpecYear(int specYear)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .Where(i => i.SpecBook == specYear.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                    .List()
                    .OrderBy(i => i.Name)
                    .ToList();
            }
        }

        private ICriterion GetProjectValidRestriction()
        {
            return Restrictions.Eq(Projections.Constant(1), 1);

            Project project = null;
            var validDisjunction = new Disjunction();
            var nullConjunction = new Conjunction();
            nullConjunction.Add(Restrictions.Where(() => project.IsValid == null));
            nullConjunction.Add(Restrictions.Where(() => project.ProjectNumber.StrLength() == 11));
            validDisjunction.Add(nullConjunction);
            var lsdbConjunction = new Conjunction();
            var lsdbDisjunction = new Disjunction();
            lsdbDisjunction.Add(Restrictions.On(() => project.ProjectNumber).IsInsensitiveLike("LS", MatchMode.End));
            lsdbDisjunction.Add(Restrictions.On(() => project.ProjectNumber).IsInsensitiveLike("DB", MatchMode.End));
            lsdbConjunction.Add(lsdbDisjunction);
            lsdbConjunction.Add(Restrictions.Where(() => project.ProjectNumber.StrLength() == 13));
            validDisjunction.Add(lsdbConjunction);
            return validDisjunction;
        }

        private ICriterion GetRejectedProposalRestriction()
        {
            return Restrictions.Eq(Projections.Constant(1), 1);

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            Proposal proposal = null;
            var proposalDisjunction = new Disjunction();
            proposalDisjunction.Add(Restrictions.Where(() => !proposal.IsRejected));
            proposalDisjunction.Add(Restrictions.Where(() => proposal.IsRejected == null));
            return proposalDisjunction;
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        public IEnumerable<Project> GetProjects(string number)
        {
            //using (var session = Initializer.TransportSessionFactory.OpenSession())
            //{
            //    var proposals = session
            //        .QueryOver<Proposal>()
            //        .JoinQueryOver(i => i.Sections)
            //        .JoinQueryOver(i => i.ProposalItems)
            //        .JoinQueryOver(i => i.ProjectItems)
            //        .List();

            //    var proposals2 = session
            //        .QueryOver<Proposal>()
            //        .JoinQueryOver(i => i.Sections)
            //        .JoinQueryOver(i => i.ProposalItems)
            //        .List();
            //    var o = new object();
            //}


            Project project = null;
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver(() => project)
                    .WhereRestrictionOn(i => i.ProjectNumber).IsInsensitiveLike(number, MatchMode.Start)
                    .Where(GetProjectValidRestriction())
                    .Where(i => i.IsLatestVersion)
                    .List()
                    .OrderBy(i => i.ProjectNumber)
                    .ToList();
            }
        }

        public Project GetProject(string number)
        {
            Proposal proposal = null;
            Project project = null;
            District district = null;
            County county = null;
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver(() => project)
                    .Where(i => i.ProjectNumber == number)
                    .Where(GetProjectValidRestriction())
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => district.MyRefDistrict)
                    .Left.JoinQueryOver(() => project.Counties, () => county)
                    .Left.JoinQueryOver(() => county.MyRefCounty)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(GetRejectedProposalRestriction())
                    .Left.JoinQueryOver(() => proposal.MyLetting)
                    .Left.JoinQueryOver(() => proposal.District)
                    .Left.JoinQueryOver(() => proposal.County)
                    .SingleOrDefault();
            }
        }

        public IEnumerable<Project> GetProjectsByProposalId(long id)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Project>()
                    .Inner.JoinQueryOver(i => i.MyProposal)
                    .Where(i => i.Id == id)
                    .List()
                    .Distinct();
            }
        }

        public IEnumerable<Proposal> GetProposals(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                

                return session
                    .QueryOver<Proposal>()
                    .Where(i => !i.IsRejected)
                    .WhereRestrictionOn(i => i.ProposalNumber).IsInsensitiveLike(number, MatchMode.Start)
                    .List()
                    .OrderBy(i => i.ProposalNumber)
                    .ToList();
            }
        }

        public Proposal GetProposal(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                ProposalItem proposalItem = null;
                return session
                    .QueryOver<Proposal>()
                    .Where(i => !i.IsRejected)
                    .Where(i => i.ProposalNumber == number)
                    .Fetch(i => i.MyLetting).Eager
                    .Fetch(i => i.District).Eager
                    .Fetch(i => i.County).Eager
                    .Fetch(i => i.Projects).Eager
                    .Fetch(i => i.Milestones).Eager
                    .Left.JoinQueryOver(i => i.Sections)
                    .Left.JoinQueryOver(i => i.ProposalItems, () => proposalItem)
                    .Left.JoinQueryOver(() => proposalItem.MyRefItem)
                    .Left.JoinQueryOver(() => proposalItem.ProjectItems)
                    .SingleOrDefault();
            }
        }

        public Proposal GetProposalAndProjectHeaders(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                ProposalVendor proposalVendor = null;

                return session
                    .QueryOver<Proposal>()
                    .Where(i => !i.IsRejected)
                    .Where(i => i.ProposalNumber == number)
                    .Fetch(i => i.Sections).Eager
                    .Fetch(i => i.MyLetting).Eager
                    .Fetch(i => i.District).Eager
                    .Fetch(i => i.County).Eager
                    .Fetch(i => i.Projects).Eager
                    .Fetch(i => i.Milestones).Eager
                    .Fetch(i => i.ProposalVendors).Eager
                    .Left.JoinQueryOver(i => i.ProposalVendors, () => proposalVendor)
                    .Left.JoinQueryOver(() => proposalVendor.Bids)
                    .SingleOrDefault();
            }
        }

        public bool IsProjectSynced(ProjectEstimate projectEstimate)
        {
            var dqeProject = projectEstimate.MyProjectVersion.MyProject;
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Project project = null;
                Category category = null;
                County county = null;
                District district = null;
                ProjectItem projectItem = null;
                Proposal proposal = null;
                var wtProject = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == dqeProject.ProjectNumber)
                    .Where(GetProjectValidRestriction())
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(GetRejectedProposalRestriction())
                    .Left.JoinQueryOver(() => proposal.MyLetting)
                    .Left.JoinQueryOver(() => proposal.Projects)
                    .Left.JoinQueryOver(() => district.MyRefDistrict)
                    .Left.JoinQueryOver(() => project.Counties, () => county)
                    .Left.JoinQueryOver(() => county.MyRefCounty)
                    .Left.JoinQueryOver(() => project.Categories, () => category)
                    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                    .Left.JoinQueryOver(() => projectItem.MyFundPackage)
                    .SingleOrDefault();
                var estimateGroups = projectEstimate.MyProjectVersion.MyProject.WtLsDbId == 0 
                    ? projectEstimate.EstimateGroups.Where(i => !i.IsLsDbSummary).ToList() 
                    :  projectEstimate.EstimateGroups.Where(i => i.IsLsDbSummary).ToList();
                return ValidateProjectCategorySynch(estimateGroups, wtProject);
            }
        }

        private bool ValidateProjectCategorySynch(IList<EstimateGroup> estimateGroups, Project wtProject)
        {
            if (estimateGroups.Count() != wtProject.Categories.Count())
            {
                return false;
            }
            foreach (var estimateGroup in estimateGroups)
            {
                var eg = estimateGroup;
                var egMatch = wtProject
                    .Categories
                    .Where(i => i.CombineLikeItems == eg.CombineWithLikeItems)
                    .Where(i => string.IsNullOrWhiteSpace(i.AlternateMember) ? string.IsNullOrWhiteSpace(eg.AlternateMember) : eg.AlternateMember == i.AlternateMember)
                    .Where(i => i.MyCategoryAlternate == null ? string.IsNullOrWhiteSpace(eg.AlternateSet) : i.MyCategoryAlternate.Name == eg.AlternateSet)
                    .Where(i => i.FederalConstructionClass == null ? string.IsNullOrWhiteSpace(eg.FederalConstructionClass) : i.FederalConstructionClass == eg.FederalConstructionClass)
                    .Where(i => i.Name == eg.Name)
                    .Where(i => i.Description == eg.Description)
                    .FirstOrDefault(i => i.Id == eg.WtId);
                if (egMatch == null)
                {
                    return false;
                }
                if (estimateGroup.ProjectItems.Count() != egMatch.ProjectItems.Count())
                {
                    return false;
                }
                foreach (var pItem in eg.ProjectItems)
                {
                    var pi = pItem;
                    var piMatch = egMatch
                        .ProjectItems
                        .Where(i => i.CombineLikeItems == pi.CombineWithLikeItems)
                        .Where(i => string.IsNullOrWhiteSpace(i.AlternateMember) ? string.IsNullOrWhiteSpace(pi.AlternateMember) : pi.AlternateMember == i.AlternateMember)
                        .Where(i => i.MyAlternate == null ? string.IsNullOrWhiteSpace(pi.AlternateSet) : pi.AlternateSet == i.MyAlternate.Name)
                        .Where(i => i.MyRefItem.Name == pi.PayItemNumber)
                        .Where(i => i.Quantity == pi.Quantity)
                        .Where(i => i.MyFundPackage == null ? string.IsNullOrWhiteSpace(pi.Fund) : pi.Fund == i.MyFundPackage.Name)
                        .Where(i => i.SupplementalDescription == null ? string.IsNullOrWhiteSpace(pi.SupplementalDescription) : pi.SupplementalDescription == i.SupplementalDescription)
                        .FirstOrDefault(i => i.Id == pi.WtId);
                    if (piMatch == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public IEnumerable<string> GetMasterFiles()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<RefItem>()
                    .Select(i => i.SpecBook)
                    .List<string>()
                    .Distinct();
            }
        }

        public Project ExportProjectForInitialLoad(string projectNumber)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Project project = null;
                County county = null;
                District district = null;
                Proposal proposal = null;
                var p = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == projectNumber)
                    .Where(GetProjectValidRestriction())
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(GetRejectedProposalRestriction())
                    .Left.JoinQueryOver(() => proposal.MyLetting)
                    .Left.JoinQueryOver(() => proposal.Projects)
                    .Left.JoinQueryOver(() => district.MyRefDistrict)
                    .Left.JoinQueryOver(() => project.Counties, () => county)
                    .Left.JoinQueryOver(() => county.MyRefCounty)
                    .SingleOrDefault();
                return p;
            }
        }

        public Project ExportProject(string projectNumber)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Project project = null;
                Category category = null;
                County county = null;
                District district = null;
                ProjectItem projectItem = null;
                Proposal proposal = null;
                var p = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == projectNumber)
                    .Where(GetProjectValidRestriction())
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(GetRejectedProposalRestriction())
                    .Left.JoinQueryOver(() => proposal.MyLetting)
                    .Left.JoinQueryOver(() => proposal.Projects)
                    .Left.JoinQueryOver(() => district.MyRefDistrict)
                    .Left.JoinQueryOver(() => project.Counties, () => county)
                    .Left.JoinQueryOver(() => county.MyRefCounty)
                    .Left.JoinQueryOver(() => project.Categories, () => category)
                    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                    .Left.JoinQueryOver(() => projectItem.MyFundPackage)
                    .SingleOrDefault();   
                return p;
            }
        }

        public Proposal ExportProposal(string proposalNumber)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Proposal>()
                    .Where(i => i.ProposalNumber == proposalNumber)
                    .Where(i => !i.IsRejected)
                    .Left.JoinQueryOver(i => i.Sections)
                    .Left.JoinQueryOver(i => i.ProposalItems)
                    .SingleOrDefault();
            }
        }

        public void UpdateRefItem(PayItemMaster payItemMaster, bool insert, DqeUser user)
        {
            //return;
            var queryUpdateRefItemSb = new StringBuilder();
            queryUpdateRefItemSb.Append(" update RefItem                                                        ");
            queryUpdateRefItemSb.Append("    set Description                   = :description,                  ");
            queryUpdateRefItemSb.Append("        Unit                          = :unit,                         ");
            queryUpdateRefItemSb.Append("        UnitSystem                    = :unitSystem,                   ");
            queryUpdateRefItemSb.Append("        LumpSum                       = :lumpSum,                      ");
            queryUpdateRefItemSb.Append("        BidAsLumpSum                  = :bidAsLumpSum,                 ");
            queryUpdateRefItemSb.Append("        SuppDescriptionRequired       = :suppDescriptionRequired,      ");
            queryUpdateRefItemSb.Append("        CombineWithLikeItems          = :combineWithLikeItems,         ");
            queryUpdateRefItemSb.Append("        MajorItem                     = :majorItem,                    ");
            queryUpdateRefItemSb.Append("        NonBid                        = :nonBid,                       ");
            queryUpdateRefItemSb.Append("        DbeInterest                   = :dbeInterest,                  ");
            queryUpdateRefItemSb.Append("        Price                         = :price,                        ");
            queryUpdateRefItemSb.Append("        ObsoleteDate                  = :obsoleteDate,                 ");
            queryUpdateRefItemSb.Append("        CommonUnit                    = :commonUnit,                   ");
            queryUpdateRefItemSb.Append("        ItemType                      = :itemType,                     ");
            queryUpdateRefItemSb.Append("        ItemClass                     = :itemClass,                    ");
            queryUpdateRefItemSb.Append("        ContractClass                 = :contractClass,                ");
            queryUpdateRefItemSb.Append("        ShortDescription              = :shortDescription,             ");
            queryUpdateRefItemSb.Append("        ConversionFactorToCommonUnits = :conversionFactorToCommonUnits,");
            queryUpdateRefItemSb.Append("        DbePercentToApply             = :dbePercentToApply,            ");
            queryUpdateRefItemSb.Append("        RecordSource                  = :recordSource,                 ");
            queryUpdateRefItemSb.Append("        BidRequirementCode            = :bidRequirementCode,           ");
            queryUpdateRefItemSb.Append("        Administrative                = :administrative,               ");
            queryUpdateRefItemSb.Append("        ExemptFromMaa                 = :exemptFromMaa,                ");
            queryUpdateRefItemSb.Append("        PayPlan                       = :payPlan,                      ");
            queryUpdateRefItemSb.Append("        AutoPaidPercentSchedule       = :autoPaidPercentSchedule,      ");
            queryUpdateRefItemSb.Append("        CoApprovalRequired            = :coApprovalRequired,           ");
            queryUpdateRefItemSb.Append("        PercentScheduleItem           = :percentScheduleItem,          ");
            queryUpdateRefItemSb.Append("        FuelAdjustment                = :fuelAdjustment,               ");
            queryUpdateRefItemSb.Append("        CreatedDate                   = :createdDate,                  ");
            queryUpdateRefItemSb.Append("        CreatedBy                     = :createdBy,                    ");
            queryUpdateRefItemSb.Append("        LastUpdatedDate               = :lastUpdatedDate,              ");
            queryUpdateRefItemSb.Append("        LastUpdatedBy                 = :lastUpdatedBy,                ");
            queryUpdateRefItemSb.Append("        SpecialtyItem                 = :specialtyItem,                ");
            queryUpdateRefItemSb.Append("        ExemptFromRetainage           = :exemptFromRetainage,          ");
            queryUpdateRefItemSb.Append("        RegressionInclusion           = :regressionInclusion,          ");
            queryUpdateRefItemSb.Append("        AlternateItemName             = :alternateItemName,            ");
            queryUpdateRefItemSb.Append("        FuelAdjustmentType            = :fuelAdjustmentType,           ");
            queryUpdateRefItemSb.Append("        IlDate2                       = :ilDate2,                      ");
            queryUpdateRefItemSb.Append("        IlDate3                       = :ilDate3,                      ");
            queryUpdateRefItemSb.Append("        Illst1                        = :illst1                        ");
            queryUpdateRefItemSb.Append("  where Name                          = :name                          ");
            queryUpdateRefItemSb.Append("    and SpecBook                      = :specBook                      ");
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        if (insert)
                        {
                            var ri = new RefItem
                            {
                                Name = payItemMaster.RefItemName,
                                SpecBook = payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'),
                                Description = payItemMaster.Description,
                                Unit = payItemMaster.Unit,
                                UnitSystem = string.IsNullOrWhiteSpace(payItemMaster.UnitSystem) ? "English" : payItemMaster.UnitSystem,
                                LumpSum = payItemMaster.LumpSum,
                                BidAsLumpSum = payItemMaster.BidAsLumpSum,
                                SuppDescriptionRequired = payItemMaster.SuppDescriptionRequired,
                                CombineWithLikeItems = payItemMaster.CombineWithLikeItems,
                                MajorItem = payItemMaster.MajorItem,
                                NonBid = payItemMaster.NonBid,
                                DbeInterest = payItemMaster.DbeInterest,
                                Price = payItemMaster.RefPrice,
                                ObsoleteDate = payItemMaster.ObsoleteDate.HasValue ? payItemMaster.ObsoleteDate.Value : (DateTime?) null,
                                ItemType = payItemMaster.ItemType,
                                ItemClass = payItemMaster.ItemClass,
                                ContractClass = payItemMaster.ContractClass,
                                ShortDescription = payItemMaster.ShortDescription,
                                ConversionFactorToCommonUnits = payItemMaster.ConversionFactorToCommonUnit,
                                DbePercentToApply = payItemMaster.DbePercentToApply,
                                RecordSource = payItemMaster.RecordSource,
                                BidRequirementCode = payItemMaster.IsFixedPrice ? "Fixed" : null,
                                Administrative = payItemMaster.Administrative,
                                ExemptFromMaa = payItemMaster.ExemptFromMaa,
                                PayPlan = payItemMaster.PayPlan,
                                AutoPaidPercentSchedule = payItemMaster.AutoPaidPercentSchedule,
                                CoApprovalRequired = payItemMaster.CoApprovalRequired,
                                PercentScheduleItem = payItemMaster.PercentScheduleItem,
                                FuelAdjustment = payItemMaster.FuelAdjustment,
                                CreatedDate = payItemMaster.CreatedDate.HasValue ? payItemMaster.CreatedDate.Value : (DateTime?) null,
                                CreatedBy = payItemMaster.CreatedBy,
                                LastUpdatedDate = payItemMaster.LastUpdatedDate.HasValue ? payItemMaster.LastUpdatedDate.Value : (DateTime?) null,
                                LastUpdatedBy = payItemMaster.LastUpdatedBy,
                                SpecialtyItem = payItemMaster.SpecialtyItem,
                                ExemptFromRetainage = payItemMaster.ExemptFromRetainage,
                                RegressionInclusion = payItemMaster.RegressionInclusion,
                                AlternateItemName = payItemMaster.AlternateItemName,
                                FuelAdjustmentType = payItemMaster.FuelAdjustmentType,
                                IlDate2 = payItemMaster.EffectiveDate.HasValue ? payItemMaster.EffectiveDate.Value : (DateTime?) null,
                                IlDate3 = payItemMaster.OpenedDate.HasValue ? payItemMaster.OpenedDate.Value : (DateTime?) null,
                                Illst1 = payItemMaster.Illst1
                            };
                            session.Save(ri);
                        }
                        else
                        {
                            var q = session.CreateQuery(queryUpdateRefItemSb.ToString());
                            q.SetParameter("name", payItemMaster.RefItemName)
                                .SetParameter("specBook", payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                                .SetParameter("description", payItemMaster.Description)
                                .SetParameter("unit", payItemMaster.Unit)
                                .SetParameter("unitSystem", string.IsNullOrWhiteSpace(payItemMaster.UnitSystem) ? "English" : payItemMaster.UnitSystem)
                                .SetParameter("lumpSum", payItemMaster.LumpSum)
                                .SetParameter("bidAsLumpSum", payItemMaster.BidAsLumpSum)
                                .SetParameter("suppDescriptionRequired", payItemMaster.SuppDescriptionRequired)
                                .SetParameter("combineWithLikeItems", payItemMaster.CombineWithLikeItems)
                                .SetParameter("majorItem", payItemMaster.MajorItem)
                                .SetParameter("nonBid", payItemMaster.NonBid)
                                .SetParameter("dbeInterest", payItemMaster.DbeInterest)
                                .SetParameter("price", payItemMaster.RefPrice)
                                .SetParameter("obsoleteDate", payItemMaster.ObsoleteDate.HasValue ? payItemMaster.ObsoleteDate.Value : (DateTime?)null)
                                .SetParameter("commonUnit", null)
                                .SetParameter("itemType", payItemMaster.ItemType)
                                .SetParameter("itemClass", payItemMaster.ItemClass)
                                .SetParameter("contractClass", payItemMaster.ContractClass)
                                .SetParameter("shortDescription", payItemMaster.ShortDescription)
                                .SetParameter("conversionFactorToCommonUnits", payItemMaster.ConversionFactorToCommonUnit)
                                .SetParameter("dbePercentToApply", payItemMaster.DbePercentToApply)
                                .SetParameter("recordSource", payItemMaster.RecordSource)
                                .SetParameter("bidRequirementCode", payItemMaster.IsFixedPrice ? "Fixed" : null)
                                .SetParameter("administrative", payItemMaster.Administrative)
                                .SetParameter("exemptFromMaa", payItemMaster.ExemptFromMaa)
                                .SetParameter("payPlan", payItemMaster.PayPlan)
                                .SetParameter("autoPaidPercentSchedule", payItemMaster.AutoPaidPercentSchedule)
                                .SetParameter("coApprovalRequired", payItemMaster.CoApprovalRequired)
                                .SetParameter("percentScheduleItem", payItemMaster.PercentScheduleItem)
                                .SetParameter("fuelAdjustment", payItemMaster.FuelAdjustment)
                                .SetParameter("createdDate", payItemMaster.CreatedDate.HasValue ? payItemMaster.CreatedDate.Value : (DateTime?)null)
                                .SetParameter("createdBy", payItemMaster.CreatedBy)
                                .SetParameter("lastUpdatedDate", payItemMaster.LastUpdatedDate.HasValue ? payItemMaster.LastUpdatedDate.Value : (DateTime?)null)
                                .SetParameter("lastUpdatedBy", payItemMaster.LastUpdatedBy)
                                .SetParameter("specialtyItem", payItemMaster.SpecialtyItem)
                                .SetParameter("exemptFromRetainage", payItemMaster.ExemptFromRetainage)
                                .SetParameter("regressionInclusion", payItemMaster.RegressionInclusion)
                                .SetParameter("alternateItemName", payItemMaster.AlternateItemName)
                                .SetParameter("fuelAdjustmentType", payItemMaster.FuelAdjustmentType)
                                .SetParameter("ilDate2", payItemMaster.EffectiveDate.HasValue ? payItemMaster.EffectiveDate.Value : (DateTime?)null)
                                .SetParameter("ilDate3", payItemMaster.OpenedDate.HasValue ? payItemMaster.OpenedDate.Value : (DateTime?)null)
                                .SetParameter("illst1", payItemMaster.Illst1)
                                .ExecuteUpdate();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdatePrices(Domain.Model.Proposal p, bool isOfficialEstimate, DqeUser user)
        {
#if DEBUG
            return;
#endif
            var projects = p.Projects;
            var total = p.GetEstimateTotalWithItems(user);
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //proposal dml
                        var queryUpdateProposalSb = new StringBuilder();
                        queryUpdateProposalSb.Append(" update Proposal                                ");
                        queryUpdateProposalSb.Append("    set ProposalItemTotal = :price,             ");
                        queryUpdateProposalSb.Append("        OfficialEstimate  = :officialEstimate,  ");
                        queryUpdateProposalSb.Append("        LastUpdatedDate   = :lastUpdatedDate,   ");
                        queryUpdateProposalSb.Append("        LastUpdatedBy     = :lastUpdatedBy      ");
                        queryUpdateProposalSb.Append("  where Id                = :id                 ");
                        //section dml
                        var queryUpdateSectionSb = new StringBuilder();
                        queryUpdateSectionSb.Append(" update Section                                  ");
                        queryUpdateSectionSb.Append("    set SectionTotal      = :price,              ");
                        queryUpdateSectionSb.Append("        IsLowCost         = :isLowCost,          ");
                        queryUpdateSectionSb.Append("        IsOriginalLowCost = :isOriginalLowCost   ");
                        queryUpdateSectionSb.Append("  where Id                = :id                  ");
                        //proposal item dml
                        var queryUpdateProposalItemSb = new StringBuilder();
                        queryUpdateProposalItemSb.Append(" update ProposalItem                        ");
                        queryUpdateProposalItemSb.Append("    set Price           = :price,           ");
                        queryUpdateProposalItemSb.Append("        IsLowCost       = :isLowCost,       ");
                        queryUpdateProposalItemSb.Append("        ExtendedAmount  = :extendedAmount,  ");
                        queryUpdateProposalItemSb.Append("        EstimateType    = 'Ad Hoc',         ");
                        queryUpdateProposalItemSb.Append("        PricingComments = :pricingComments, ");
                        queryUpdateProposalItemSb.Append("        LastUpdatedDate = :lastUpdatedDate, ");
                        queryUpdateProposalItemSb.Append("        LastUpdatedBy   = :lastUpdatedBy    ");
                        queryUpdateProposalItemSb.Append("  where Id              = :id               ");
                        //project item dml
                        var queryUpdateProjectItemSb = new StringBuilder();
                        queryUpdateProjectItemSb.Append(" update ProjectItem                          ");
                        queryUpdateProjectItemSb.Append("    set Price           = :price,            ");
                        queryUpdateProjectItemSb.Append("        IsLowCost       = :isLowCost,        ");
                        queryUpdateProjectItemSb.Append("        PricingComments = :pricingComments,  ");
                        queryUpdateProjectItemSb.Append("        EstimateType    = 'Ad Hoc',          ");
                        queryUpdateProjectItemSb.Append("        ExtendedAmount  = :extendedAmount,   ");
                        queryUpdateProjectItemSb.Append("        LastUpdatedDate = :lastUpdatedDate,  ");
                        queryUpdateProjectItemSb.Append("        LastUpdatedBy   = :lastUpdatedBy     ");
                        queryUpdateProjectItemSb.Append("  where Id              = :id                ");
                        //project dml
                        var queryUpdateProjectSb = new StringBuilder();
                        queryUpdateProjectSb.Append(" update Project                                  ");
                        queryUpdateProjectSb.Append("        set ProjectItemTotal = :price,           ");
                        queryUpdateProjectSb.Append("            EstimatedDate    = :estimatedDate,   ");
                        queryUpdateProjectSb.Append("            LastUpdatedDate  = :lastUpdatedDate, ");
                        queryUpdateProjectSb.Append("            LastUpdatedBy    = :lastUpdatedBy,   ");
                        queryUpdateProjectSb.Append("            PricedBy         = :pricedBy,        ");
                        queryUpdateProjectSb.Append("            PricedDate       = :pricedDate       ");
                        queryUpdateProjectSb.Append("      where Id               = :id               ");
                        //category dml
                        var queryUpdateCategorySb = new StringBuilder();
                        queryUpdateCategorySb.Append(" update Category                                ");
                        queryUpdateCategorySb.Append("    set IsLowCost       = :isLowCost,           ");
                        queryUpdateCategorySb.Append("        LastUpdatedDate = :lastUpdatedDate,     ");
                        queryUpdateCategorySb.Append("        LastUpdatedBy   = :lastUpdatedBy        ");
                        queryUpdateCategorySb.Append("  where Id              = :id                   ");
                        //
                        var queryUpdateProposal = session.CreateQuery(queryUpdateProposalSb.ToString());
                        var queryUpdateSection = session.CreateQuery(queryUpdateSectionSb.ToString());
                        var queryUpdateProposalItem = session.CreateQuery(queryUpdateProposalItemSb.ToString());
                        var queryUpdateProjectItem = session.CreateQuery(queryUpdateProjectItemSb.ToString());
                        var queryUpdateProject = session.CreateQuery(queryUpdateProjectSb.ToString());
                        var queryUpdateCategory = session.CreateQuery(queryUpdateCategorySb.ToString());
                        //set proposal price
                        var records = queryUpdateProposal
                            .SetParameter("price", Math.Round(total.Total, 2))
                            .SetParameter("officialEstimate", isOfficialEstimate ? "Y" : null)
                            .SetParameter("lastUpdatedDate", DateTime.Now)
                            .SetParameter("lastUpdatedBy", "DQE")
                            .SetParameter("id", p.WtId)
                            .ExecuteUpdate();
                        if (records == 0) throw new InvalidOperationException("Updated unexpected proposal");
                        //set project level prices
                        foreach (var project in projects)
                        {
                            var versions = project.ProjectVersions.Where(i => i.VersionOwner == user);
                            var estimates = new List<ProjectEstimate>();
                            foreach (var projectVersion in versions)
                            {
                                estimates.AddRange(projectVersion.ProjectEstimates);
                            }
                            var estimate = estimates.FirstOrDefault(i => i.IsWorkingEstimate);
                            if (estimate != null)
                            {
                                var projectTotal = estimate.GetEstimateTotalWithItems();
                                records = queryUpdateProject
                                    .SetParameter("price", Math.Round(projectTotal.Total, 2))
                                    .SetParameter("estimatedDate", DateTime.Now)
                                    .SetParameter("lastUpdatedDate", DateTime.Now)
                                    .SetParameter("lastUpdatedBy", "DQE")
                                    .SetParameter("pricedBy", "DQE")
                                    .SetParameter("pricedDate", DateTime.Now)
                                    .SetParameter("id", project.WtId)
                                    .ExecuteUpdate();
                                if (records == 0) throw new InvalidOperationException("Updated unexpected project");
                                foreach (var categorySet in projectTotal.CategorySets)
                                {
                                    foreach (var estimateGroup in categorySet.EstimateGroups)
                                    {
                                        records = queryUpdateCategory
                                            .SetParameter("isLowCost", categorySet.Included)
                                            .SetParameter("lastUpdatedDate", DateTime.Now)
                                            .SetParameter("lastUpdatedBy", "DQE")
                                            .SetParameter("id", estimateGroup.WtId)
                                            .ExecuteUpdate();
                                        if (records == 0) throw new InvalidOperationException("Updated unexpected category");
                                    }
                                }
                            }
                        }
                        //set proposal level prices
                        foreach (var categorySet in total.CategorySets)
                        {
                            foreach (var sectionGroup in categorySet.SectionGroups)
                            {
                                var includedItemSets = categorySet.ItemSets.Where(i => i.Included).ToList();
                                var includedItems = new List<Domain.Model.ProposalItem>();
                                foreach (var includedItemSet in includedItemSets)
                                {
                                    includedItems.AddRange(includedItemSet.ProposalItems);
                                }
                                includedItems = includedItems.Intersect(sectionGroup.ProposalItems).ToList();
                                var sectionGroupTotal = includedItems.Sum(i => i.GetEstimatorProjectItems(user).Sum(ii => ii.Price * ii.Quantity));
                                records = queryUpdateSection
                                    .SetParameter("price", Math.Round(sectionGroupTotal, 2))
                                    .SetParameter("isLowCost", categorySet.Included)
                                    .SetParameter("isOriginalLowCost", true)
                                    .SetParameter("id", sectionGroup.WtId)
                                    .ExecuteUpdate();
                                if (records == 0) throw new InvalidOperationException("Updated unexpected section");
                            }
                            foreach (var itemSet in categorySet.ItemSets)
                            {
                                foreach (var proposalItem in itemSet.ProposalItems)
                                {
                                    var projectItems = proposalItem.GetEstimatorProjectItems(user).ToList();
                                    if (!projectItems.Any()) continue;
                                    var proposalItemTemp = projectItems.First();
                                    if (proposalItemTemp != null)
                                    {
                                        records = queryUpdateProposalItem
                                            .SetParameter("price", Math.Round(proposalItemTemp.Price, 2))
                                            .SetParameter("isLowCost", itemSet.Included)
                                            .SetParameter("extendedAmount", Math.Round(proposalItem.Quantity * proposalItemTemp.Price, 2))
                                            .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), proposalItemTemp.PriceSet))
                                            .SetParameter("lastUpdatedDate", DateTime.Now)
                                            .SetParameter("lastUpdatedBy", "DQE")
                                            .SetParameter("id", proposalItem.WtId)
                                            .ExecuteUpdate();
                                        if (records == 0) throw new InvalidOperationException("Updated unexpected proposal item");
                                        foreach (var projectItem in projectItems)
                                        {
                                            records = queryUpdateProjectItem
                                            .SetParameter("price", Math.Round(projectItem.Price, 2))
                                            .SetParameter("isLowCost", itemSet.Included)
                                            .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2))
                                            .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), projectItem.PriceSet))
                                            .SetParameter("lastUpdatedDate", DateTime.Now)
                                            .SetParameter("lastUpdatedBy", "DQE")
                                            .SetParameter("id", projectItem.WtId)
                                            .ExecuteUpdate();
                                            if (records == 0) throw new InvalidOperationException("Updated unexpected proposal item");
                                        }
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public object GetAllBidHistory(int range)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                ProposalItem proposalItem = null;
                RefItem refItem = null;
                Bid bid = null;
                ProposalVendor proposalVendor = null;
                Proposal proposal = null;
                RefCounty refCounty = null;
                Letting letting = null;
                Milestone milestone = null;

                var milestoneDisjunction = new Disjunction();
                milestoneDisjunction.Add(Restrictions.Where(() => milestone.Main == null));
                milestoneDisjunction.Add(Restrictions.Where(() => milestone.Main));

                var allResults = session
                    .QueryOver(() => proposalItem)
                    .JoinQueryOver(() => proposalItem.Bids, () => bid)
                    .JoinQueryOver(() => proposalItem.MyRefItem, () => refItem)
                    .JoinQueryOver(() => bid.MyProposalVendor, () => proposalVendor)
                    .JoinQueryOver(() => proposalVendor.MyProposal, () => proposal)
                    .JoinQueryOver(() => proposal.MyLetting, () => letting)
                    .JoinQueryOver(() => proposal.County, () => refCounty)
                    .Left.JoinQueryOver(() => proposal.Milestones, () => milestone)
                    .Where(GetRejectedProposalRestriction())
                    .Where(() => letting.LettingDate < DateTime.Now.Date)
                    .Where(() => letting.LettingDate >= DateTime.Now.Date.AddMonths(range*-1).Date)
                    .Where(() => proposalVendor.BidType == "RESP")
                    .Where(() => proposalVendor.BidStatus != "I")
                    .Where(milestoneDisjunction)
                    .WhereRestrictionOn(() => proposal.ProposalStatus).IsIn(new object[] {"01", "02", "03", "22"})
                    .OrderBy(() => refItem.Name).Asc
                    .OrderBy(() => letting.LettingDate).Desc
                    .OrderBy(() => proposal.ProposalNumber).Asc
                    .OrderBy(() => bid.BidPrice).Asc
                    .Select
                    (
                        Projections.Property(() => refItem.Name).As("ItemName"),
                        Projections.Property(() => proposalItem.Id).As("Id"),
                        Projections.Property(() => proposalItem.Quantity).As("Quantity"),
                        Projections.Property(() => proposal.ProposalNumber).As("ProposalNumber"),
                        Projections.Property(() => proposal.ProposalType).As("ProposalType"),
                        Projections.Property(() => proposal.ContractType).As("ContractType"),
                        Projections.Property(() => proposal.ContractWorkType).As("ContractWorkType"),
                        Projections.Property(() => milestone.NumberOfUnits).As("Days"),
                        Projections.Property(() => refCounty.Description).As("County"),
                        Projections.Property(() => letting.LettingDate).As("LettingDate"),
                        Projections.Property(() => bid.BidPrice).As("BidPrice"),
                        Projections.Property(() => bid.LowCost).As("LowCost"),
                        Projections.Property(() => proposalVendor.Awarded).As("Awarded"),
                        Projections.Property(() => proposalVendor.BidTotal).As("BidTotal")
                    )
                    .TransformUsing(new DynamicTransformer())
                    .List<dynamic>();
                var histories = new List<ApplicationServices.BidHistory>();
                var distinctItems = allResults.Select(i => i.ItemName).Distinct().ToList();
                foreach (var distinctItem in distinctItems)
                {
                    var results = allResults.Where(i => i.ItemName == distinctItem).ToList();
                    var distinctProposals = results.Select(i => i.Id).Distinct().ToList();
                    var maxBidders = distinctProposals.Select(distinctProposal => results.Count(i => i.Id == distinctProposal)).Concat(new[] { 0 }).Max();
                    var proposals = new List<ApplicationServices.ProposalHistory>();
                    foreach (var p in distinctProposals)
                    {
                        var localP = p;
                        var pResults = results.Where(i => i.Id == localP).ToList();
                        var bids = pResults.Select(i => new ApplicationServices.Bid
                        {
                            IsBlank = false,
                            Price = ((decimal?)i.BidPrice).HasValue ? ((decimal?)i.BidPrice).Value : 0,
                            Included = true,
                            IsLowCost = (bool) i.LowCost,
                            IsAwarded = (bool) i.Awarded,
                            BidTotal = ((decimal?)i.BidTotal).HasValue ? ((decimal?)i.BidTotal).Value : 0,
                            LettingDate = i.LettingDate,
                            Quantity = i.Quantity,
                            County = i.County
                        });
                        var pr = new ApplicationServices.ProposalHistory
                        {
                            Proposal = results.First(i => i.Id == p).ProposalNumber,
                            County = results.First(i => i.Id == p).County,
                            Included = true,
                            Quantity = results.First(i => i.Id == p).Quantity,
                            Letting = results.First(i => i.Id == p).LettingDate,
                            ProposalType = results.First(i => i.Id == p).ProposalType,
                            ContractType = results.First(i => i.Id == p).ContractType,
                            ContractWorkType = results.First(i => i.Id == p).ContractWorkType,
                            Duration = ((long?)results.First(i => i.Id == p).Days).HasValue ? ((long?)results.First(i => i.Id == p).Days).Value : 0
                         };
                        bids.ForEach(b => pr.Bids.Add(b));
                        proposals.Add(pr);
                    }
                    var c = 0;
                    var history = new ApplicationServices.BidHistory
                    {
                        ItemName = distinctItem
                    };
                    proposals.ForEach(i => history.Proposals.Add(i));
                    history.MaxBiddersProposal = maxBidders == 0
                        ? null
                        : new
                        {
                            bids = new int[maxBidders].Select(ii => new
                            {
                                number = c += 1
                            })
                        };
                    histories.Add(history);
                }
                return histories;
            }
        }

        public IEnumerable<Letting> GetLettingNames(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Letting>()
                    .WhereRestrictionOn(i => i.LettingName).IsInsensitiveLike(number, MatchMode.Start)
                    .List()
                    .OrderBy(i => i.LettingName)
                    .ToList();
            }
        }

        public Letting GetLetting(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Letting letting = null;
                Proposal proposal = null;
                ProposalVendor proposalVendor = null;
                Bid bid = null;
                ProposalItem proposalItem = null;
                RefVendor refVendor = null;

                return session
                    .QueryOver(() => letting)
                    .Where(() => letting.LettingName == number)
                    .Left.JoinQueryOver(() => letting.Proposals, () => proposal)
                    .Left.JoinQueryOver(() => proposal.ProposalVendors, () => proposalVendor)
                    .Left.JoinQueryOver(() => proposalVendor.Bids, () => bid)
                    .Left.JoinQueryOver(() => proposalVendor.MyRefVendor, () => refVendor)
                    .Left.JoinQueryOver(() => bid.MyProposalItem, () => proposalItem)
                    .Left.JoinQueryOver(() => proposalItem.MyRefItem)
                    .SingleOrDefault();
            }
        }

        public Letting GetLettingByProposal(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Letting letting = null;
                Proposal proposal = null;

                return session
                    .QueryOver(() => proposal)
                    .Where(() => proposal.ProposalNumber == number)
                    .Left.JoinQueryOver(() => proposal.MyLetting, () => letting)
                    .SingleOrDefault()
                    .MyLetting;
            }
        }
    }
}