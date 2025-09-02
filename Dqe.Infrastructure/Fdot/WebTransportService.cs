using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using NHibernate;
using NHibernate.Type;

namespace Dqe.Infrastructure.Fdot
{

    public class WebTransportService : IWebTransportService
    {

        private static readonly IDictionary<string, CodeTable> CodeTables = new Dictionary<string, CodeTable>();

        private static readonly object Lock = new object();

        /// <summary>
        /// Retrieves a list of pay item details (name and description) based on the input string.
        /// Supports case-insensitive searching by pay item number (Name) or partial description. Results are filtered to SpecBook "13".
        /// </summary>
        /// <param name="input">The user-provided search term, which may be a pay item number or part of a description.</param>
        /// <returns>A distinct list of matching <see cref="PayItemDTO"/> objects containing Name and combined Description.</returns>
        public IList<PayItemDTO> GetPayItemDetails(string input)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                RefItem ri = null;

                var sanitizedInput = input.Replace(" ", "").Trim();

                var rawResults = session.QueryOver(() => ri)
                    .Where(() => ri.SpecBook == "13")
                    .And(Restrictions.Or(
                        Expression.Sql("LOWER(REPLACE(RTRIM(LTRIM(REFITEM_NM)) + ' - ' + RTRIM(LTRIM(DESCR)), ' ', '')) LIKE ?", $"%{input.Trim().ToLower().Replace(" ", "")}%", NHibernateUtil.String),
                        Restrictions.On(() => ri.Description).IsLike(input.Trim(), MatchMode.Anywhere)
                    ))
                    .SelectList(list => list
                        .Select(() => ri.Id)
                        .Select(() => ri.Name)
                        .Select(() => ri.Description)
                    )
                    .OrderBy(() => ri.Name).Asc()
                    .ThenBy(() => ri.Description).Asc()
                    .Take(40)
                    .List<object[]>();

                var finalResults = rawResults
                    .Select(row => new PayItemDTO
                    {
                        Name = row[1]?.ToString(),
                        Description = $"{row[1]?.ToString()} - {row[2]?.ToString()}"
                    })
                    .Distinct()
                    .ToList();

                return finalResults;
            }



        }

        /// <summary>
        /// Retrieves a list of bid details from WTP database.
        /// and sorted by Descending by letting date (l.LettingDate) and Ascending by bid price.
        /// </summary>
        public IList<ProposalItemDTO> GetUnitPriceDetails(
        string payItem,
        List<string> contractType = null,
        int months = 12,
        List<string> contractWorkType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string[] counties = null,
        string bidStatus = null,
        string[] marketCounties = null,
        decimal? minRank = null,
        decimal? maxRank = null,
        List<string> workTypeNames = null,
        string projectNumber = null,
        decimal? minBidAmount = null, decimal? maxBidAmount = null
            )
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {

                var categorySubquery = DetachedCriteria.For<Category>("catSub")
                    .CreateAlias("catSub.ProjectItems", "piSub")
                    .CreateAlias("piSub.MyRefItem", "riSub")
                    .Add(Restrictions.EqProperty("riSub.Id", "ri.Id"))
                    .Add(Restrictions.EqProperty("piSub.MyProposalItem.Id", "this.Id"))
                    .SetProjection(Projections.Property("catSub.Description"))
                    .SetMaxResults(1);


                var projectNumberSubquery = DetachedCriteria.For<ProjectItem>("pitem")
                    .CreateAlias("pitem.MyProject", "prjSub")
                    .Add(Restrictions.EqProperty("pitem.MyProposalItem.Id", "this.Id"))
                    .Add(Restrictions.EqProperty("pitem.MyRefItem.Id", "ri.Id"))
                    .SetProjection(Projections.Property("prjSub.ProjectNumber"))
                    .SetMaxResults(1);
                var projectIdSubquery = DetachedCriteria.For<ProjectItem>("pitem")
                    .CreateAlias("pitem.MyProject", "prjSub")
                    .Add(Restrictions.EqProperty("pitem.MyProposalItem.Id", "this.Id"))
                    .Add(Restrictions.EqProperty("pitem.MyRefItem.Id", "ri.Id"))
                    .SetProjection(Projections.Property("prjSub.Id"))
                    .SetMaxResults(1);
                var projectCodeSubquery = DetachedCriteria.For<Project>("prj")
                    .Add(Subqueries.PropertyEq("prj.Id", projectIdSubquery))
                    .SetProjection(Projections.Property("prj.Pjcde1"))
                    .SetMaxResults(1);

                // Subquery: Work Mix Description (for projection)
                var workMixSubquery = DetachedCriteria.For<CodeValue>("cv")
                    .CreateAlias("cv.MyCodeTable", "ct")
                    .Add(Restrictions.Eq("ct.Id", 203L))
                    .Add(Subqueries.PropertyEq("cv.CodeValueName", projectCodeSubquery))
                    .SetProjection(Projections.Property("cv.Description"))
                    .SetMaxResults(1);

                // Main query
                var query = session.CreateCriteria<ProposalItem>()
                    .CreateAlias("MyRefItem", "ri")
                    .CreateAlias("Bids", "b")
                    .CreateAlias("b.MyProposalVendor", "pv")
                    .CreateAlias("pv.MyProposal", "p")
                    .CreateAlias("p.MyLetting", "l")
                    .CreateAlias("p.County", "c")
                    .CreateAlias("p.District", "d")
                    .CreateAlias("p.Milestones", "m")
                    .CreateAlias("pv.MyRefVendor", "rv")

                    .Add(Restrictions.Or(
                        Restrictions.In("pv.BidType", new[] { "RESP", "NONR", "" }),
                        Restrictions.IsNull("pv.BidType")
                    ))
                    .Add(Restrictions.Eq("p.ProposalStatus", "03"))
                    .Add(Restrictions.Or(
                        Restrictions.IsNull("m.Main"),
                        Restrictions.Eq("m.Main", true)
                    ))
                    .Add(Restrictions.Lt("l.LettingDate", DateTime.Today))
                    .Add(Restrictions.Eq("ri.SpecBook", "13"))
                    .Add(GetProjectValidRestriction())
                    .AddOrder(Order.Asc("ri.Name"))
                    .AddOrder(Order.Desc("l.LettingDate"))
                    .AddOrder(Order.Asc("p.ProposalNumber"))
                    .AddOrder(Order.Asc("b.BidPrice"));
                if (!string.IsNullOrEmpty(projectNumber))
                {
                    query.Add(Restrictions.Eq("p.ProposalNumber", projectNumber));
                }
                if (!string.IsNullOrEmpty(payItem))
                    query.Add(Restrictions.Eq("ri.Name", payItem));
                if (contractWorkType != null && contractWorkType.Any())
                    query.Add(Restrictions.In("p.ContractWorkType", contractWorkType));

                if (startDate.HasValue && endDate.HasValue)
                    query.Add(Restrictions.Between("l.LettingDate", startDate.Value, endDate.Value));
                else
                    query.Add(Restrictions.Ge("l.LettingDate", DateTime.Today.AddMonths(-months)));

                if (counties != null && counties.Length > 0)
                    query.Add(Restrictions.In("c.Description", counties));

                if (!string.IsNullOrEmpty(bidStatus))
                {
                    if (bidStatus == "FMV")
                        query.Add(Restrictions.In("pv.VendorRanking", new[] { 1, 2, 3 }));
                    else
                        query.Add(Restrictions.Eq("pv.BidStatus", bidStatus));
                }

                if (contractType != null && contractType.Any())
                    query.Add(Restrictions.In("p.ContractType", contractType));

                if (marketCounties != null && marketCounties.Any())
                    query.Add(Restrictions.In("c.Description", marketCounties));

                if (minRank > 0 && maxRank > 0)
                    query.Add(Restrictions.Between("Quantity", minRank, maxRank));
                else if (minRank > 0)
                {
                    query.Add(Restrictions.Ge("Quantity", minRank));
                }
                else if (maxRank > 0)
                {
                    query.Add(Restrictions.Le("Quantity", maxRank));
                }
                if (minBidAmount.HasValue && maxBidAmount.HasValue && minBidAmount > 0 && maxBidAmount > 0)
                {
                    query.Add(Restrictions.Between("pv.BidTotal", minBidAmount.Value, maxBidAmount.Value));
                }
                else if (minBidAmount.HasValue && minBidAmount > 0)
                {
                    query.Add(Restrictions.Ge("pv.BidTotal", minBidAmount.Value));
                }
                else if (maxBidAmount.HasValue && maxBidAmount > 0)
                {
                    query.Add(Restrictions.Le("pv.BidTotal", maxBidAmount.Value));
                }
                if (workTypeNames != null && workTypeNames.Any())
                {
                    var workMixFilterSubquery = DetachedCriteria.For<CodeValue>("cv")
                        .CreateAlias("cv.MyCodeTable", "ct")
                        .Add(Restrictions.Eq("ct.Id", 203L))
                        .Add(Restrictions.In("cv.Description", workTypeNames))
                        .Add(Subqueries.PropertyEq("cv.CodeValueName", projectCodeSubquery))
                        .SetProjection(Projections.Id());

                    // Apply EXISTS filter
                    query.Add(Subqueries.Exists(workMixFilterSubquery));
                }
                // Projection
                query.SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property("pv.BidStatus"), "BidStatus")
                    .Add(Projections.Property("pv.BidTotal"), "PvBidTotal")
                    .Add(Projections.Property("ri.Name"), "ri")
                    .Add(Projections.Property("ri.Id"), "riId")
                    .Add(Projections.Property("Id"), "Id")
                    .Add(Projections.Property("Quantity"), "Quantity")
                    .Add(Projections.Property("b.BidPrice"), "b")
                    .Add(Projections.SubQuery(projectNumberSubquery), "ProjectNumber")
                    .Add(Projections.SubQuery(projectIdSubquery), "ProjectId")
                    .Add(Projections.Property("p.ProposalNumber"), "p")
                    .Add(Projections.Property("p.ProposalType"), "ProposalType")
                    .Add(Projections.Property("p.ContractType"), "ContractType")
                    .Add(Projections.Property("p.ContractWorkType"), "ContractWorkType")
                    .Add(Projections.Property("c.Description"), "c")
                    .Add(Projections.Property("d.Description"), "d")
                    .Add(Projections.Property("rv.VendorName"), "VendorName")
                    .Add(Projections.Property("l.LettingDate"), "l")
                    .Add(Projections.Property("ri.Description"), "Description")
                    .Add(Projections.Property("SupplementalDescription"), "SupplementalDescription")
                    .Add(Projections.Property("ri.CalculatedUnit"), "CalculatedUnit")
                    .Add(Projections.Property("m.NumberOfUnits"), "Duration")
                    .Add(Projections.Property("p.ExecutedDate"), "ExecutedDate")
                    .Add(Projections.Property("pv.BidType"), "BidType")
                    .Add(Projections.Property("pv.VendorRanking"), "VendorRanking")
                    .Add(Projections.Property("ri.ObsoleteDate"), "ObsoleteDate")
                    .Add(Projections.SubQuery(categorySubquery), "CategoryDescription")
                    .Add(Projections.SubQuery(workMixSubquery), "WorkMixDescription"))
                    .SetResultTransformer(NHibernate.Transform.Transformers.AliasToBean<ProposalItemDTO>());

                var res = query.List<ProposalItemDTO>();
                return res.Distinct().ToList();
            }
        }

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
                    /*
                    new 
                    {
                        name = "(0)", description = "1 Whole"
                    },
                    new 
                    {
                        name = "(1)", description = "1/10"
                    },
                    new 
                    {
                        name = "(2)", description = "1/100"
                    },
                    new 
                    {
                        name = "(3)", description = "1/1000"
                    },
                    AC 1/10 of an acre 
                    CF 1/10 of a cubic foot 
                    CY 1/10 of a cubic yard 
                    MB 1/10 of a thousand foot board measure 
                    TN 1/10 of a Ton
                    GM 1/1000 of a mile 
                    NM 1/1000 of a mile 
                    */
                    pimt.Itmqtyprecsn = pimt.Unit == "AC" || pimt.Unit == "CF" || pimt.Unit == "CY" || pimt.Unit == "MB" || pimt.Unit == "TN"
                        ? "1"
                        : pimt.Itmqtyprecsn = pimt.Unit == "GM" || pimt.Unit == "NM"
                            ? "3"
                            : "0";
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

        public DateTime? GetProjectLetting(long wtId)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Project>()
                    .Where(i => i.Id == wtId)
                    .Select(i => i.LettingDate)
                    .SingleOrDefault<DateTime?>();

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
                var res = session
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
                return res;
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
                    : projectEstimate.EstimateGroups.Where(i => i.IsLsDbSummary).ToList();
                return ValidateProjectCategorySynch(estimateGroups, wtProject, dqeProject);
            }
        }

        private bool ValidateProjectCategorySynch(IList<EstimateGroup> estimateGroups, Project wtProject, Domain.Model.Project dProject)
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
                        //.Where(i => i.MyFundPackage == null ? string.IsNullOrWhiteSpace(pi.Fund) : pi.Fund == i.MyFundPackage.Name)
                        .Where(i => i.SupplementalDescription == null ? string.IsNullOrWhiteSpace(pi.SupplementalDescription) : pi.SupplementalDescription == i.SupplementalDescription)
                        .FirstOrDefault(i => i.Id == pi.WtId);
                    if (piMatch == null)
                    {
                        return false;
                    }
                }
                if (wtProject.SpecBook != dProject.MyMasterFile.FileNumber.ToString())
                {
                    return false;
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

        public void UpdateRefItem(PayItemMaster payItemMaster, DqeUser user)
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
            queryUpdateRefItemSb.Append("        IlDate1                       = :ilDate1,                      ");
            queryUpdateRefItemSb.Append("        IlDate3                       = :ilDate3,                      ");
            queryUpdateRefItemSb.Append("        Illst1                        = :illst1,                       ");
            queryUpdateRefItemSb.Append("        Itmqtyprecsn                  = :itmqtyprecsn,                 ");
            queryUpdateRefItemSb.Append("        IlFlag1                       = :ilFlag1                       ");
            queryUpdateRefItemSb.Append("  where Name                          = :name                          ");
            queryUpdateRefItemSb.Append("    and SpecBook                      = :specBook                      ");
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var existingItem = session.QueryOver<RefItem>()
                            .Where(i => i.Name == payItemMaster.RefItemName)
                            .Where(i => i.SpecBook == payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                            .SingleOrDefault();

                        if (existingItem == null)
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
                                ObsoleteDate = payItemMaster.ObsoleteDate.HasValue ? payItemMaster.ObsoleteDate.Value : (DateTime?)null,
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
                                CreatedDate = payItemMaster.CreatedDate.HasValue ? payItemMaster.CreatedDate.Value : (DateTime?)null,
                                CreatedBy = payItemMaster.CreatedBy,
                                LastUpdatedDate = payItemMaster.LastUpdatedDate.HasValue ? payItemMaster.LastUpdatedDate.Value : (DateTime?)null,
                                LastUpdatedBy = payItemMaster.LastUpdatedBy,
                                SpecialtyItem = payItemMaster.SpecialtyItem,
                                ExemptFromRetainage = payItemMaster.ExemptFromRetainage,
                                RegressionInclusion = payItemMaster.RegressionInclusion,
                                AlternateItemName = payItemMaster.AlternateItemName,
                                FuelAdjustmentType = payItemMaster.FuelAdjustmentType,
                                IlDate1 = payItemMaster.EffectiveDate.HasValue ? payItemMaster.EffectiveDate.Value : (DateTime?)null,
                                IlDate3 = payItemMaster.OpenedDate.HasValue ? payItemMaster.OpenedDate.Value : (DateTime?)null,
                                Illst1 = payItemMaster.Illst1,
                                Itmqtyprecsn = payItemMaster.Itmqtyprecsn,
                                IlFlag1 = payItemMaster.Ilflg1,
                                CommonUnit = payItemMaster.CommonUnit
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
                                .SetParameter("commonUnit", payItemMaster.CommonUnit)
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
                                .SetParameter("createdDate", payItemMaster.CreatedDate.HasValue ? payItemMaster.CreatedDate.Value : existingItem.CreatedDate)
                                .SetParameter("createdBy", !string.IsNullOrEmpty(payItemMaster.CreatedBy) ? payItemMaster.CreatedBy : existingItem.CreatedBy)
                                .SetParameter("lastUpdatedDate", payItemMaster.LastUpdatedDate.HasValue ? payItemMaster.LastUpdatedDate.Value : (DateTime?)null)
                                .SetParameter("lastUpdatedBy", payItemMaster.LastUpdatedBy)
                                .SetParameter("specialtyItem", payItemMaster.SpecialtyItem)
                                .SetParameter("exemptFromRetainage", payItemMaster.ExemptFromRetainage)
                                .SetParameter("regressionInclusion", payItemMaster.RegressionInclusion)
                                .SetParameter("alternateItemName", payItemMaster.AlternateItemName)
                                .SetParameter("fuelAdjustmentType", payItemMaster.FuelAdjustmentType)
                                .SetParameter("ilDate1", payItemMaster.EffectiveDate.HasValue ? payItemMaster.EffectiveDate.Value : (DateTime?)null)
                                .SetParameter("ilDate3", payItemMaster.OpenedDate.HasValue ? payItemMaster.OpenedDate.Value : (DateTime?)null)
                                .SetParameter("illst1", payItemMaster.Illst1)
                                .SetParameter("itmqtyprecsn", payItemMaster.Itmqtyprecsn)
                                .SetParameter("ilFlag1", payItemMaster.Ilflg1)
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

        public Exception InsertRefItems(IEnumerable<PayItemMaster> payItemMasters, DqeUser user)
        {
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
            queryUpdateRefItemSb.Append("        IlDate1                       = :ilDate1,                      ");
            queryUpdateRefItemSb.Append("        IlDate3                       = :ilDate3,                      ");
            queryUpdateRefItemSb.Append("        Illst1                        = :illst1,                       ");
            queryUpdateRefItemSb.Append("        Itmqtyprecsn                  = :itmqtyprecsn,                 ");
            queryUpdateRefItemSb.Append("        IlFlag1                       = :ilFlag1                       ");
            queryUpdateRefItemSb.Append("  where Name                          = :name                          ");
            queryUpdateRefItemSb.Append("    and SpecBook                      = :specBook                      ");
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        foreach (var payItemMaster in payItemMasters)
                        {

                            var existingItem = session.QueryOver<RefItem>()
                                .Where(i => i.Name == payItemMaster.RefItemName)
                                .Where(
                                    i =>
                                        i.SpecBook ==
                                        payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture)
                                            .PadLeft(2, '0'))
                                .SingleOrDefault();

                            if (existingItem == null)
                            {
                                var ri = new RefItem
                                {
                                    Name = payItemMaster.RefItemName,
                                    SpecBook =
                                        payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture)
                                            .PadLeft(2, '0'),
                                    Description = payItemMaster.Description,
                                    Unit = payItemMaster.Unit,
                                    UnitSystem =
                                        string.IsNullOrWhiteSpace(payItemMaster.UnitSystem)
                                            ? "English"
                                            : payItemMaster.UnitSystem,
                                    LumpSum = payItemMaster.LumpSum,
                                    BidAsLumpSum = payItemMaster.BidAsLumpSum,
                                    SuppDescriptionRequired = payItemMaster.SuppDescriptionRequired,
                                    CombineWithLikeItems = payItemMaster.CombineWithLikeItems,
                                    MajorItem = payItemMaster.MajorItem,
                                    NonBid = payItemMaster.NonBid,
                                    DbeInterest = payItemMaster.DbeInterest,
                                    Price = payItemMaster.RefPrice,
                                    ObsoleteDate =
                                        payItemMaster.ObsoleteDate.HasValue
                                            ? payItemMaster.ObsoleteDate.Value
                                            : (DateTime?)null,
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
                                    CreatedDate =
                                        payItemMaster.CreatedDate.HasValue
                                            ? payItemMaster.CreatedDate.Value
                                            : (DateTime?)null,
                                    CreatedBy = payItemMaster.CreatedBy,
                                    LastUpdatedDate =
                                        payItemMaster.LastUpdatedDate.HasValue
                                            ? payItemMaster.LastUpdatedDate.Value
                                            : (DateTime?)null,
                                    LastUpdatedBy = payItemMaster.LastUpdatedBy,
                                    SpecialtyItem = payItemMaster.SpecialtyItem,
                                    ExemptFromRetainage = payItemMaster.ExemptFromRetainage,
                                    RegressionInclusion = payItemMaster.RegressionInclusion,
                                    AlternateItemName = payItemMaster.AlternateItemName,
                                    FuelAdjustmentType = payItemMaster.FuelAdjustmentType,
                                    IlDate1 =
                                        payItemMaster.EffectiveDate.HasValue
                                            ? payItemMaster.EffectiveDate.Value
                                            : (DateTime?)null,
                                    IlDate3 =
                                        payItemMaster.OpenedDate.HasValue
                                            ? payItemMaster.OpenedDate.Value
                                            : (DateTime?)null,
                                    Illst1 = payItemMaster.Illst1,
                                    Itmqtyprecsn = payItemMaster.Itmqtyprecsn,
                                    IlFlag1 = payItemMaster.Ilflg1,
                                    CommonUnit = payItemMaster.CommonUnit
                                };
                                session.Save(ri);
                            }
                            else
                            {
                                var q = session.CreateQuery(queryUpdateRefItemSb.ToString());
                                q.SetParameter("name", payItemMaster.RefItemName)
                                    .SetParameter("specBook",
                                        payItemMaster.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture)
                                            .PadLeft(2, '0'))
                                    .SetParameter("description", payItemMaster.Description)
                                    .SetParameter("unit", payItemMaster.Unit)
                                    .SetParameter("unitSystem",
                                        string.IsNullOrWhiteSpace(payItemMaster.UnitSystem)
                                            ? "English"
                                            : payItemMaster.UnitSystem)
                                    .SetParameter("lumpSum", payItemMaster.LumpSum)
                                    .SetParameter("bidAsLumpSum", payItemMaster.BidAsLumpSum)
                                    .SetParameter("suppDescriptionRequired", payItemMaster.SuppDescriptionRequired)
                                    .SetParameter("combineWithLikeItems", payItemMaster.CombineWithLikeItems)
                                    .SetParameter("majorItem", payItemMaster.MajorItem)
                                    .SetParameter("nonBid", payItemMaster.NonBid)
                                    .SetParameter("dbeInterest", payItemMaster.DbeInterest)
                                    .SetParameter("price", payItemMaster.RefPrice)
                                    .SetParameter("obsoleteDate",
                                        payItemMaster.ObsoleteDate.HasValue
                                            ? payItemMaster.ObsoleteDate.Value
                                            : (DateTime?)null)
                                    .SetParameter("commonUnit", payItemMaster.CommonUnit)
                                    .SetParameter("itemType", payItemMaster.ItemType)
                                    .SetParameter("itemClass", payItemMaster.ItemClass)
                                    .SetParameter("contractClass", payItemMaster.ContractClass)
                                    .SetParameter("shortDescription", payItemMaster.ShortDescription)
                                    .SetParameter("conversionFactorToCommonUnits",
                                        payItemMaster.ConversionFactorToCommonUnit)
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
                                    .SetParameter("createdDate",
                                        payItemMaster.CreatedDate.HasValue
                                            ? payItemMaster.CreatedDate.Value
                                            : (DateTime?)null)
                                    .SetParameter("createdBy", payItemMaster.CreatedBy)
                                    .SetParameter("lastUpdatedDate",
                                        payItemMaster.LastUpdatedDate.HasValue
                                            ? payItemMaster.LastUpdatedDate.Value
                                            : (DateTime?)null)
                                    .SetParameter("lastUpdatedBy", payItemMaster.LastUpdatedBy)
                                    .SetParameter("specialtyItem", payItemMaster.SpecialtyItem)
                                    .SetParameter("exemptFromRetainage", payItemMaster.ExemptFromRetainage)
                                    .SetParameter("regressionInclusion", payItemMaster.RegressionInclusion)
                                    .SetParameter("alternateItemName", payItemMaster.AlternateItemName)
                                    .SetParameter("fuelAdjustmentType", payItemMaster.FuelAdjustmentType)
                                    .SetParameter("ilDate1",
                                        payItemMaster.EffectiveDate.HasValue
                                            ? payItemMaster.EffectiveDate.Value
                                            : (DateTime?)null)
                                    .SetParameter("ilDate3",
                                        payItemMaster.OpenedDate.HasValue
                                            ? payItemMaster.OpenedDate.Value
                                            : (DateTime?)null)
                                    .SetParameter("illst1", payItemMaster.Illst1)
                                    .SetParameter("itmqtyprecsn", payItemMaster.Itmqtyprecsn)
                                    .SetParameter("ilFlag1", payItemMaster.Ilflg1)
                                    .ExecuteUpdate();
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        transaction.Rollback();
                        return exception;
                    }
                }
            }
            return null;
        }

        public string UpdateFixedPrices(ProjectEstimate p, DqeUser user)
        {
#if DEBUG
            return string.Empty;
#endif
            var payItemRepo = new PayItemMasterRepository();
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var wtProject = session.CreateQuery("from Project where ProjectNumber = :projectNumber and IsLatestVersion = true")
                        .SetParameter("projectNumber", p.MyProjectVersion.MyProject.ProjectNumber)
                        .UniqueResult<Project>();
                        var fixedPriceItems = payItemRepo.GetFixedPriceItems(wtProject).Select(i => i.RefItemName.ToUpper().Trim()).ToList();
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
                        var queryUpdateProjectItem = session.CreateQuery(queryUpdateProjectItemSb.ToString());
                        foreach (var eg in p.EstimateGroups)
                        {
                            foreach (var projectItem in eg.ProjectItems)
                            {
                                if (fixedPriceItems.Contains(projectItem.PayItemNumber.ToUpper().Trim()))
                                {
                                    queryUpdateProjectItem
                                    .SetParameter("price", Math.Round(projectItem.Price, 2))
                                    .SetParameter("isLowCost", true)
                                    .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2))
                                    .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), projectItem.PriceSet))
                                    .SetParameter("lastUpdatedDate", DateTime.Now)
                                    .SetParameter("lastUpdatedBy", "DQE")
                                    .SetParameter("id", projectItem.WtId)
                                    .ExecuteUpdate();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            //no throw because wT could have rolled back the transaction
                        }
                        if (exception.InnerException != null)
                        {
                            var message = exception.InnerException.Message;
                            var regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
                            var matches = regex.Matches(message);
                            var l = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                            if (l.Any())
                            {
                                var innerMessageArray = message.Split(new[] { '{', '}' });
                                if (innerMessageArray.Length < 2) return message;
                                var innerMessage = string.Format("Project Preconstruction Validation - {0}", message.Split(new[] { '{', '}' })[1]);
                                return innerMessage;
                            }
                            throw;
                        }
                        throw;
                    }
                    return string.Empty;
                }
            }
        }

        public string UpdateFixedPrices(Domain.Model.Proposal p, DqeUser user)
        {
#if DEBUG
            return string.Empty;
#endif
            var payItemRepo = new PayItemMasterRepository();
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var wtProposal = session.CreateQuery("from Proposal where ProposalNumber = :proposalNumber")
                        .SetParameter("proposalNumber", p.ProposalNumber)
                        .UniqueResult<Proposal>();
                        var fixedPriceItems = payItemRepo.GetFixedPriceItems(wtProposal == null ? null : wtProposal.Projects.FirstOrDefault()).Select(i => i.RefItemName.ToUpper().Trim()).ToList();
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
                        var queryUpdateProposalItem = session.CreateQuery(queryUpdateProposalItemSb.ToString());
                        var queryUpdateProjectItem = session.CreateQuery(queryUpdateProjectItemSb.ToString());
                        foreach (var section in p.SectionGroups)
                        {
                            foreach (var proposalItem in section.ProposalItems)
                            {
                                if (fixedPriceItems.Contains(proposalItem.PayItemNumber.ToUpper().Trim()))
                                {
                                    var projectItems = proposalItem.GetEstimatorProjectItems(user).ToList();
                                    if (!projectItems.Any()) continue;
                                    var proposalItemTemp = projectItems.First();
                                    if (proposalItemTemp != null)
                                    {
                                        queryUpdateProposalItem
                                            .SetParameter("price", Math.Round(proposalItemTemp.Price, 2))
                                            .SetParameter("isLowCost", true)
                                            .SetParameter("extendedAmount", Math.Round(proposalItem.Quantity * proposalItemTemp.Price, 2, MidpointRounding.AwayFromZero))
                                            .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), proposalItemTemp.PriceSet))
                                            .SetParameter("lastUpdatedDate", DateTime.Now)
                                            .SetParameter("lastUpdatedBy", "DQE")
                                            .SetParameter("id", proposalItem.WtId)
                                            .ExecuteUpdate();
                                        foreach (var projectItem in projectItems)
                                        {
                                            queryUpdateProjectItem
                                                .SetParameter("price", Math.Round(projectItem.Price, 2))
                                                .SetParameter("isLowCost", true)
                                                .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2, MidpointRounding.AwayFromZero))
                                                .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), projectItem.PriceSet))
                                                .SetParameter("lastUpdatedDate", DateTime.Now)
                                                .SetParameter("lastUpdatedBy", "DQE")
                                                .SetParameter("id", projectItem.WtId)
                                                .ExecuteUpdate();
                                        }
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            //no throw because wT could have rolled back the transaction
                        }
                        if (exception.InnerException != null)
                        {
                            var message = exception.InnerException.Message;
                            var regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
                            var matches = regex.Matches(message);
                            var l = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                            if (l.Any())
                            {
                                var innerMessageArray = message.Split(new[] { '{', '}' });
                                if (innerMessageArray.Length < 2) return message;
                                var innerMessage = string.Format("Project Preconstruction Validation - {0}", message.Split(new[] { '{', '}' })[1]);
                                return innerMessage;
                            }
                            throw;
                        }
                        throw;
                    }
                    return string.Empty;
                }
            }
        }

        public string UpdateProjectPrices(ProjectEstimate p, DqeUser user, bool allPrices = false)
        {
#if DEBUG
            return string.Empty;
#endif
            var total = p.GetEstimateTotalWithItems();
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
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
                        var queryUpdateProjectItem = session.CreateQuery(queryUpdateProjectItemSb.ToString());
                        var queryUpdateProject = session.CreateQuery(queryUpdateProjectSb.ToString());
                        var queryUpdateCategory = session.CreateQuery(queryUpdateCategorySb.ToString());
                        var records = queryUpdateProject
                            .SetParameter("price", Math.Round(total.Total, 2))
                            .SetParameter("estimatedDate", DateTime.Now)
                            .SetParameter("lastUpdatedDate", DateTime.Now)
                            .SetParameter("lastUpdatedBy", "DQE")
                            .SetParameter("pricedBy", "DQE")
                            .SetParameter("pricedDate", DateTime.Now)
                            .SetParameter("id", p.MyProjectVersion.MyProject.WtId)
                            .ExecuteUpdate();
                        if (records == 0) throw new InvalidOperationException("Updated unexpected project");
                        foreach (var categorySet in total.CategorySets)
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

                                if (allPrices)
                                {
                                    foreach (var projectItem in estimateGroup.ProjectItems)
                                    {
                                        records = queryUpdateProjectItem
                                            .SetParameter("price", Math.Round(projectItem.Price, 2))
                                            .SetParameter("isLowCost", categorySet.Included)
                                            .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2, MidpointRounding.AwayFromZero))
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
                        if (!allPrices)
                        {
                            //state furnished items
                            var projectItems = new List<Domain.Model.ProjectItem>();
                            foreach (var eg in p.EstimateGroups)
                            {
                                projectItems.AddRange(eg.ProjectItems.Where(i => !i.ParentProposalId.HasValue).ToList());
                            }
                            foreach (var projectItem in projectItems)
                            {
                                records = queryUpdateProjectItem
                                .SetParameter("price", Math.Round(projectItem.Price, 2))
                                .SetParameter("isLowCost", true)
                                .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2, MidpointRounding.AwayFromZero))
                                .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), projectItem.PriceSet))
                                .SetParameter("lastUpdatedDate", DateTime.Now)
                                .SetParameter("lastUpdatedBy", "DQE")
                                .SetParameter("id", projectItem.WtId)
                                .ExecuteUpdate();
                                if (records == 0) throw new InvalidOperationException("Updated unexpected proposal item");
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            //no throw because wT could have rolled back the transaction
                        }
                        if (exception.InnerException != null)
                        {
                            var message = exception.InnerException.Message;
                            var regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
                            var matches = regex.Matches(message);
                            var l = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                            if (l.Any())
                            {
                                var innerMessageArray = message.Split(new[] { '{', '}' });
                                if (innerMessageArray.Length < 2) return message;
                                var innerMessage = string.Format("Project Preconstruction Validation - {0}", message.Split(new[] { '{', '}' })[1]);
                                return innerMessage;
                            }
                            throw;
                        }
                        throw;
                    }
                    return string.Empty;
                }
            }
        }

        public string UpdatePrices(Domain.Model.Proposal p, bool isOfficialEstimate, DqeUser user)
        {
#if DEBUG
            return string.Empty;
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
                                        if (records == 0 && estimateGroup.ProjectItems.Any()) throw new InvalidOperationException("Updated unexpected category");
                                    }
                                }
                                //state furnished items
                                var projectItems = new List<Domain.Model.ProjectItem>();
                                foreach (var eg in estimate.EstimateGroups)
                                {
                                    projectItems.AddRange(eg.ProjectItems.Where(i => !i.ParentProposalId.HasValue).ToList());
                                }
                                foreach (var projectItem in projectItems)
                                {
                                    records = queryUpdateProjectItem
                                    .SetParameter("price", Math.Round(projectItem.Price, 2))
                                    .SetParameter("isLowCost", true)
                                    .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2, MidpointRounding.AwayFromZero))
                                    .SetParameter("pricingComments", Enum.GetName(typeof(PriceSetType), projectItem.PriceSet))
                                    .SetParameter("lastUpdatedDate", DateTime.Now)
                                    .SetParameter("lastUpdatedBy", "DQE")
                                    .SetParameter("id", projectItem.WtId)
                                    .ExecuteUpdate();
                                    if (records == 0) throw new InvalidOperationException("Updated unexpected proposal item");
                                }
                                //end state furnished items
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
                                            .SetParameter("extendedAmount", Math.Round(proposalItem.Quantity * proposalItemTemp.Price, 2, MidpointRounding.AwayFromZero))
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
                                            .SetParameter("extendedAmount", Math.Round(projectItem.Quantity * projectItem.Price, 2, MidpointRounding.AwayFromZero))
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
                    catch (Exception exception)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            //no throw because wT could have rolled back the transaction
                        }
                        if (exception.InnerException != null)
                        {
                            var message = exception.InnerException.Message;
                            var regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
                            var matches = regex.Matches(message);
                            var l = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
                            if (l.Any())
                            {
                                var innerMessageArray = message.Split(new[] { '{', '}' });
                                if (innerMessageArray.Length < 2) return message;
                                var innerMessage = string.Format("Project Preconstruction Validation - {0}", message.Split(new[] { '{', '}' })[1]);
                                return innerMessage;
                            }
                            throw;
                        }
                        throw;
                    }
                    return string.Empty;
                }
            }
        }

        public object GetLsDbEstimateHistory()
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                ProposalItem proposalItem = null;
                RefItem refItem = null;
                Proposal proposal = null;
                Section section = null;
                RefCounty refCounty = null;
                Milestone milestone = null;

                var milestoneDisjunction = new Disjunction();
                milestoneDisjunction.Add(Restrictions.Where(() => milestone.Main == null));
                milestoneDisjunction.Add(Restrictions.Where(() => milestone.Main));

                var lsDbDisjunction = new Disjunction();
                lsDbDisjunction.Add(Restrictions.On(() => proposal.ProposalNumber).IsInsensitiveLike("%LS"));
                lsDbDisjunction.Add(Restrictions.On(() => proposal.ProposalNumber).IsInsensitiveLike("%DB"));

                var allResults = session
                    .QueryOver(() => proposalItem)
                    .JoinQueryOver(() => proposalItem.MyRefItem, () => refItem)
                    .JoinQueryOver(() => proposalItem.MySection, () => section)
                    .JoinQueryOver(() => section.MyProposal, () => proposal)
                    .JoinQueryOver(() => proposal.County, () => refCounty)
                    .Left.JoinQueryOver(() => proposal.Milestones, () => milestone)
                    .Where(lsDbDisjunction)
                    .Where(milestoneDisjunction)

                    // This to test with specific data
                    //
                    //.Where(() => proposal.ProposalNumber == "E2Z33DB")
                    //.Where(() => refItem.Name == "0102  1")
                    //.Where(() => proposal.ProposalNumber == "E8P90")
                    //.Where(() => refItem.Name == "0561  1")
                    // end filter for specific data

                    .OrderBy(() => refItem.Name).Asc
                    .OrderBy(() => proposal.ProposalNumber).Asc
                    .Select
                    (
                        Projections.Property(() => refItem.Name).As("ItemName"),
                        Projections.Property(() => proposalItem.Id).As("Id"),
                        Projections.Property(() => proposalItem.Quantity).As("Quantity"),
                        Projections.Property(() => proposalItem.Price).As("Estimate"),
                        Projections.Property(() => proposalItem.ExtendedAmount).As("ExtendedAmount"),
                        Projections.Property(() => proposal.ProposalNumber).As("ProposalNumber"),
                        Projections.Property(() => proposal.ProposalType).As("ProposalType"),
                        Projections.Property(() => proposal.ContractType).As("ContractType"),
                        Projections.Property(() => proposal.ContractWorkType).As("ContractWorkType"),
                        Projections.Property(() => milestone.NumberOfUnits).As("Days"),
                        Projections.Property(() => refCounty.Description).As("County")
                    )
                    .TransformUsing(new DynamicTransformer())
                    .List<dynamic>();
                var histories = new List<ApplicationServices.BidHistory>();
                var distinctItems = allResults.Select(i => i.ItemName).Distinct().ToList();
                foreach (var distinctItem in distinctItems)
                {
                    var results = allResults.Where(i => i.ItemName == distinctItem).ToList();
                    var distinctProposals = results.Select(i => i.Id).Distinct().ToList();
                    const int maxBidders = 1;
                    var proposals = new List<ApplicationServices.ProposalHistory>();
                    foreach (var p in distinctProposals)
                    {
                        var localP = p;
                        var pResults = results.First(i => i.Id == localP);
                        var bid = new ApplicationServices.Bid
                        {
                            IsBlank = false,
                            Price = ((decimal?)pResults.Estimate).HasValue ? ((decimal?)pResults.Estimate).Value : 0,
                            Included = true,
                            IsLowCost = true,
                            IsAwarded = true,
                            IsEstimate = true,
                            BidTotal = ((decimal?)pResults.Estimate).HasValue ? ((decimal?)pResults.Estimate).Value : 0,
                            LettingDate = DateTime.MinValue,
                            Quantity = pResults.Quantity,
                            County = pResults.County
                        };
                        var pr = new ApplicationServices.ProposalHistory
                        {
                            Proposal = pResults.ProposalNumber,
                            County = pResults.County,
                            Included = true,
                            Quantity = pResults.Quantity,
                            EstimateAmount = ((decimal?)pResults.Estimate).HasValue ? ((decimal?)pResults.Estimate).Value : 0,
                            ExtendedAmount = ((decimal?)pResults.ExtendedAmount).HasValue ? ((decimal?)pResults.ExtendedAmount).Value : 0,
                            Letting = DateTime.MinValue,
                            ProposalType = pResults.ProposalType,
                            ContractType = pResults.ContractType,
                            ContractWorkType = pResults.ContractWorkType,
                            Duration = ((long?)pResults.Days).HasValue ? ((long?)pResults.Days).Value : 0
                        };
                        pr.Bids.Add(bid);
                        proposals.Add(pr);
                    }
                    var c = 0;
                    var history = new ApplicationServices.BidHistory
                    {
                        ItemName = distinctItem
                    };
                    proposals.ForEach(i => history.Proposals.Add(i));
                    history.MaxBiddersProposal =
                        new
                        {
                            bids = new int[maxBidders].Select(ii => new
                            {
                                number = c += 1
                            })
                        };
                    histories.Add(history);
                }
                var test = histories.SelectMany(s => s.Proposals).Where(w => w.Quantity == 0);
                return histories;
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

                var bidTypeDisjunction = new Disjunction();
                //!!!!NOT - IRR (irregular), NONQ (not qualified), OTH (other) 
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == "RESP"));
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == ""));
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == null));
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == "NONR"));

                var bidStatusDisjunction = new Disjunction();
                bidStatusDisjunction.Add(Restrictions.Where(() => proposalVendor.BidStatus == null));
                bidStatusDisjunction.Add(Restrictions.Where(() => proposalVendor.BidStatus != "I"));

                //TODO: convert to left join once we want to pull DB and LS detail estimates. They are currently excluded because the detail versions won't have bids.

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
                    .Where(() => letting.LettingDate >= DateTime.Now.Date.AddMonths(range * -1).Date)

                    // This to test with specific data
                    //.Where(() => proposal.ProposalNumber == "E2Z33")
                    //.Where(() => refItem.Name == "0560  1  1")
                    //.Where(() => proposal.ProposalNumber == "E8P90")
                    //.Where(() => refItem.Name == "0561  1")
                    // end filter for specific data

                    .Where(bidTypeDisjunction)
                    .Where(bidStatusDisjunction)
                    .Where(milestoneDisjunction)
                    //00 - !!!!NOT advertised
                    //01 - bids received
                    //02 - awarded
                    //03 - executed
                    //04 - bids rejected
                    //05 - !!!!NOT withdrawn
                    //06 - award cancel
                    //07 - execution cancel
                    //08 - !!!!NOT no bids received
                    //09 - !!!!NOT moved
                    //17 - !!!!NOT postponed
                    //22 - intent to award
                    //24 - intent to reject
                    //SA - scope alternate rejected

                    .WhereRestrictionOn(() => proposal.ProposalStatus).IsIn(new object[] { "01", "02", "03", "04", "06", "07", "22", "24", "SA" })
                    .OrderBy(() => refItem.Name).Asc
                    .OrderBy(() => letting.LettingDate).Desc
                    .OrderBy(() => proposal.ProposalNumber).Asc
                    .OrderBy(() => bid.BidPrice).Asc
                    .Select
                    (
                        Projections.Property(() => refItem.Name).As("ItemName"),
                        Projections.Property(() => proposalItem.Id).As("Id"),
                        Projections.Property(() => proposalItem.Quantity).As("Quantity"),
                        Projections.Property(() => proposalItem.Price).As("Estimate"),
                        Projections.Property(() => proposalItem.ExtendedAmount).As("ExtendedAmount"),
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
                            IsLowCost = (bool)i.LowCost,
                            IsAwarded = (bool)i.Awarded,
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
                            EstimateAmount = results.First(i => i.Id == p).Estimate ?? 0,
                            ExtendedAmount = results.First(i => i.Id == p).ExtendedAmount ?? 0,
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
                BidTime bidTime = null;
                ProposalItem proposalItem = null;
                RefVendor refVendor = null;
                Project project = null;

                return session
                    .QueryOver(() => letting)
                    .Where(() => letting.LettingName == number)
                    .Left.JoinQueryOver(() => letting.Proposals, () => proposal)
                    .Left.JoinQueryOver(() => proposal.Projects, () => project)
                    .Left.JoinQueryOver(() => proposal.ProposalVendors, () => proposalVendor)
                    .Left.JoinQueryOver(() => proposalVendor.BidTimes, () => bidTime)
                    .Left.JoinQueryOver(() => proposalVendor.Bids, () => bid)
                    .Left.JoinQueryOver(() => proposalVendor.MyRefVendor, () => refVendor)
                    .Left.JoinQueryOver(() => bid.MyProposalItem, () => proposalItem)
                    .Left.JoinQueryOver(() => proposalItem.MyRefItem)
                    .SingleOrDefault();
            }
        }

        public Letting GetResponsiveLettings(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                Letting letting = null;
                Proposal proposal = null;
                ProposalVendor proposalVendor = null;
                BidTime bidTime = null;
                Bid bid = null;
                ProposalItem proposalItem = null;
                RefVendor refVendor = null;
                Project project = null;

                var bidTypeDisjunction = new Disjunction();
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == "RESP"));
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == ""));
                bidTypeDisjunction.Add(Restrictions.Where(() => proposalVendor.BidType == null));


                return session
                    .QueryOver(() => letting)
                    .Where(() => letting.LettingName == number)
                    .Left.JoinQueryOver(() => letting.Proposals, () => proposal)
                    .Left.JoinQueryOver(() => proposal.Projects, () => project)
                    .Left.JoinQueryOver(() => proposal.ProposalVendors, () => proposalVendor)
                    .Left.JoinQueryOver(() => proposalVendor.BidTimes, () => bidTime)
                    .Where(bidTypeDisjunction)
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

                var res = session
                    .QueryOver(() => proposal)
                    .Where(() => proposal.ProposalNumber == number)
                    .Left.JoinQueryOver(() => proposal.MyLetting, () => letting)
                    .SingleOrDefault()
                    .MyLetting;
                return res;
            }
        }

        public IList<Proposal> GetProposalsReadyForOfficialEstimate()
        {

            var statusDateConjunction = new Conjunction();
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate != null));
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate >= new DateTime(2015, 6, 1)));
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate <= DateTime.Now.Date));


            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Proposal>()
                    .Where(i => i.ProposalStatus == "02" || i.ProposalStatus == "03")
                    .Where(statusDateConjunction)
                    .Where(i => i.OfficialEstimate == null)
                    .Fetch(i => i.Projects).Eager
                    .Fetch(i => i.District).Eager
                    .List()
                    .Distinct()
                    .ToList();
            }
        }

        public bool IsProposalReadyForOfficialEstimate(string proposalNumber)
        {
            var statusDateConjunction = new Conjunction();
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate != null));
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate >= new DateTime(2015, 6, 1)));
            statusDateConjunction.Add(Restrictions.Where<Proposal>(i => i.StatusDate <= DateTime.Now.Date));

            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                var wtProposal = session
                    .QueryOver<Proposal>()
                    .Where(i => i.ProposalNumber == proposalNumber)
                    .Where(i => i.ProposalStatus == "02" || i.ProposalStatus == "03")
                    .Where(statusDateConjunction)
                    //.Where(i => i.OfficialEstimate == null)
                    .SingleOrDefault();

                if (wtProposal != null)
                    return true;
            }
            return false;
        }

        public void UpdateProposalReadyForDssPass(Proposal proposal)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        var queryUpdateProposalSb = new StringBuilder();
                        queryUpdateProposalSb.Append(" update Proposal                                ");
                        queryUpdateProposalSb.Append("    set PassToDss         = :passToDss,         ");
                        queryUpdateProposalSb.Append("        PassToDssDate     = :passToDssDate,     ");
                        queryUpdateProposalSb.Append("        LastUpdatedDate   = :lastUpdatedDate,   ");
                        queryUpdateProposalSb.Append("        LastUpdatedBy     = :lastUpdatedBy      ");
                        queryUpdateProposalSb.Append("  where Id                = :id                 ");

                        var queryUpdateProposal = session.CreateQuery(queryUpdateProposalSb.ToString());

                        var records = queryUpdateProposal
                            .SetParameter("passToDss", proposal.PassToDss == "B" ? "C" : null)
                            .SetParameter("passToDssDate", null)
                            .SetParameter("lastUpdatedDate", DateTime.Now)
                            .SetParameter("lastUpdatedBy", "DQE")
                            .SetParameter("id", proposal.Id)
                            .ExecuteUpdate();

                        if (records == 0) throw new InvalidOperationException("Proposal Not updated for DSS");

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
    }
}