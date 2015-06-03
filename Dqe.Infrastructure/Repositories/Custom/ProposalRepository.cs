using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class ProposalRepository : BaseRepository, IProposalRepository
    {
        public ProposalRepository() { }

        internal ProposalRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<Proposal> GetByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Proposal>()
                .Where(i => i.ProposalNumber == number)
                .List();
        }

        public void DeleteProposalStructure(long id)
        {
            InitializeSession();
            Session.CreateQuery("update ProjectItem item set item.MyProposalItem = null where exists (select 1 from ProjectItem as pri join pri.MyProposalItem where exists (select 1 from ProposalItem as pi join pi.MySectionGroup where exists(select 1 from SectionGroup as sg join sg.MyProposal as p where p.Id = :id)))").SetParameter("id", id).ExecuteUpdate();
            Session.CreateQuery("delete from ProposalItem where exists (select 1 from ProposalItem as pi join pi.MySectionGroup where exists(select 1 from SectionGroup as sg join sg.MyProposal as p where p.Id = :id))").SetParameter("id", id).ExecuteUpdate();
            Session.CreateQuery("delete from SectionGroup where exists (select 1 from SectionGroup as sg join sg.MyProposal as p where p.Id = :id)").SetParameter("id", id).ExecuteUpdate();
            //Session.Flush();
        }

        public void BuildReportProposal(long proposalId, DqeUser owner, IPayItemMasterRepository payItemMasterRepository, IReportRepository reportRepository, IWebTransportService webTransportService, bool working)
        {
            //return;
            InitializeSession();

            Session.Flush();

            ProjectEstimate projectEstimate = null;
            ProjectVersion projectVersion = null;
            Project project = null;
            Proposal proposal = null;
            EstimateGroup estimateGroup = null;
            ProjectItem projectItem = null;
            var proposalEstimate = Session
                .QueryOver<Proposal>()
                .Where(i => i.Id == proposalId)
                .Where(i => i.ProposalSource == ProposalSourceType.Wt)
                .Left.JoinQueryOver(i => i.SectionGroups)
                .Left.JoinQueryOver(i => i.ProposalItems)
                .Left.JoinQueryOver(i => i.ProjectItems)
                .SingleOrDefault();
            var nextSnapshot = proposalEstimate.GetNextSnapshotLabel();
            if (nextSnapshot != SnapshotLabel.Authorization && nextSnapshot != SnapshotLabel.Official && !working) return;

            var proposalLevel = nextSnapshot == SnapshotLabel.Authorization
                ? ReportProposalLevel.Authorization
                : ReportProposalLevel.Official;

            proposalLevel = working ? ReportProposalLevel.WorkingEstimate : proposalLevel;

            var wtProposal = webTransportService.GetProposal(proposalEstimate.ProposalNumber);
            var reportLetting =  wtProposal.MyLetting != null
                    ? reportRepository.GetReportLettingByProposalLevel(wtProposal.MyLetting.LettingName, proposalLevel)
                    : null;

            var existingReport = Session.QueryOver<ReportProposal>()
                .Where(i => i.ProposalNumber == proposalEstimate.ProposalNumber)
                .Where(i => i.ProposalLevel == proposalLevel)
                .SingleOrDefault();

            if (existingReport != null)
            {
                if (reportLetting != null)
                {
                    reportLetting.RemoveReportProposal(existingReport);
                    Session.SaveOrUpdate(reportLetting);
                }
                else
                    Session.Delete(existingReport);

                Session.Flush();
            }

            var specBook = wtProposal.Projects.First().SpecBook;
            var queryProjects = Session
                .QueryOver(() => projectEstimate)
                .Where(() => projectEstimate.IsWorkingEstimate)
                .JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .JoinQueryOver(() => projectVersion.MyProject, () => project)
                .Where(() => project.CustodyOwner == owner)
                .JoinQueryOver(() => project.Proposals, () => proposal)
                .Where(() => proposal.Id == proposalId)
                .Where(() => proposal.ProposalSource == ProposalSourceType.Wt)
                .Left.JoinQueryOver(() => projectEstimate.EstimateGroups, () => estimateGroup)
                .Left.JoinQueryOver(() => estimateGroup.ProjectItems, () => projectItem);
            var projectEstimates = queryProjects.List().Distinct().ToList();
            var payItems = payItemMasterRepository.GetAll(specBook).ToList();
            //build the report structure
            //TODO: bail if query returns not acceptable result

            var reportProposal = CreateReportProposal(owner, webTransportService, wtProposal, proposalEstimate, proposalLevel);

            if (wtProposal.MyLetting != null)
            {
                if (reportLetting == null)
                {
                    reportLetting = new ReportLetting
                    {
                        LettingName = wtProposal.MyLetting.LettingName,
                        LettingDate = wtProposal.MyLetting.LettingDate
                    };
                }
                reportLetting.AddReportProposal(reportProposal);
            }

            if (wtProposal.Milestones != null)
            {
                foreach (var milestone in wtProposal.Milestones)
                {
                    var reportMilestone = new ReportProposalMilestone
                    {
                        Description = milestone.Description,
                        Unit = milestone.Unit,
                        ConstructionDays = milestone.NumberOfUnits ?? (long)0,
                        CostPerDay = milestone.RoadCostPerTimeUnit ?? (decimal)0
                    };
                    reportProposal.AddReportProposalMilestone(reportMilestone);
                }
            }
            foreach (var prj in proposalEstimate.Projects)
            {
                var wtProject = wtProposal.Projects.FirstOrDefault(i => i.ProjectNumber == prj.ProjectNumber);
                string workTypeDescription = string.Empty;

                if (wtProject != null)
                {
                    var code = webTransportService.GetCodeTable("WRKTYP");
                    var workType = code.CodeValues.First(i => i.CodeValueName == wtProject.ProjectWorkType);
                    workTypeDescription = workType.CodeValueName + " - " + workType.Description;
                }

                var reportProject = new ReportProject
                {
                    LettingDate = reportLetting != null ? reportLetting.LettingDate : (DateTime?)null,
                    ProjectNumber = prj.ProjectNumber,
                    Description = prj.Description,
                    District = prj.District,
                    FederalProjectNumber = wtProject != null ? wtProject.FederalProjectNumber : string.Empty,
                    ProjectWorkType = workTypeDescription,
                    County = string.Format("{0} - {1}", prj.MyCounty.Code, prj.MyCounty.Name),
                    Primary = wtProject != null && wtProject.Controlling ? "Y" : "N"
                };
                reportProposal.AddReportProject(reportProject);
            }

            var proposalTotal = proposalEstimate.GetEstimateTotal(owner);
            proposalTotal.Initialize();
            foreach (var categorySet in proposalTotal.CategorySets)
            {
                foreach (var itemSet in categorySet.ItemSets)
                {
                    var reportProposalSummary = CreateReportProposalSummary(categorySet, itemSet);
                    reportProposal.AddReportProposalSummary(reportProposalSummary);
                }
            }
            
            var dateForComparison = reportLetting != null ? reportLetting.LettingDate : DateTime.Now;

            CreateReportProjectData(reportProposal, projectEstimates, payItems, dateForComparison, wtProposal, proposalTotal, owner);

            CreateProposalItems(proposalEstimate, payItems, reportProposal, dateForComparison, wtProposal);

            if (reportLetting != null)
                Session.SaveOrUpdate(reportLetting);
            else
                Session.SaveOrUpdate(reportProposal);

            Session.Flush();
        }

        public Proposal GetWtByNumber(string number)
        {
            InitializeSession();
            Proposal proposal = null;
            DqeUser dqeUser = null;
            Project project = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;
            return Session
                .QueryOver(() => proposal)
                .Where(i => i.ProposalNumber == number)
                .Where(i => i.ProposalSource == ProposalSourceType.Wt)
                .Left.JoinQueryOver(() => proposal.Projects, () => project)
                .Left.JoinQueryOver(() => project.CustodyOwner, () => dqeUser)
                .Left.JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .Left.JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .SingleOrDefault();
        }

        public Proposal GetById(long id)
        {
            InitializeSession();
            Proposal proposal = null;
            DqeUser versionOwner = null;
            DqeUser custodyOwner = null;
            Project project = null;
            SectionGroup sectionGroup = null;
            ProposalItem proposalItem = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;
            EstimateGroup estimateGroup = null;
            ProjectItem projectItem = null;
            var prop = Session
                .QueryOver(() => proposal)
                .Where(i => i.Id == id)
                .Left.JoinQueryOver(() => proposal.Projects, () => project)
                .Left.JoinQueryOver(() => project.CustodyOwner, () => custodyOwner)
                .Left.JoinQueryOver(() => proposal.SectionGroups, () => sectionGroup)
                .Left.JoinQueryOver(() => sectionGroup.ProposalItems, () => proposalItem)
                .Left.JoinQueryOver(() => proposalItem.ProjectItems, () => projectItem)
                .Left.JoinQueryOver(() => projectItem.MyEstimateGroup, () => estimateGroup)
                .Left.JoinQueryOver(() => estimateGroup.MyProjectEstimate, () => projectEstimate)
                .Left.JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .Left.JoinQueryOver(() => projectVersion.VersionOwner, () => versionOwner)
                .SingleOrDefault();
            return prop;
        }

        public IEnumerable<ProjectItem> GetDqeProjectItemsForProposal(DqeUser custodyUser, string[] projects)
        {
            InitializeSession();
            var userId = custodyUser.Id;
            var pa = new List<object>();
            pa.AddRange(projects);
            ProjectItem projectItem = null;
            EstimateGroup estimateGroup = null;
            ProjectEstimate projectEstimate = null;
            ProjectVersion projectVersion = null;
            Project project = null;
            DqeUser versionOwner = null;
            DqeUser custodyOwner = null;
            return Session
                .QueryOver(() => projectItem)
                .JoinQueryOver(() => projectItem.MyEstimateGroup, () => estimateGroup)
                .JoinQueryOver(() => estimateGroup.MyProjectEstimate, () => projectEstimate)
                .Where(() => projectEstimate.IsWorkingEstimate)
                .JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .JoinQueryOver(() => projectVersion.VersionOwner, () => versionOwner)
                .Where(() => versionOwner.Id == userId)
                .JoinQueryOver(() => projectVersion.MyProject, () => project)
                .WhereRestrictionOn(() => project.ProjectNumber).IsIn(pa)
                .JoinQueryOver(() => project.CustodyOwner, () => custodyOwner)
                .Where(() => custodyOwner.Id == userId)
                .List()
                .Distinct();
        }

        public Proposal GetOfficialProposal(string proposalNumber)
        {
            InitializeSession();
            Proposal proposal = null;
            Project project = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;

            return Session
                .QueryOver(() => proposal)
                .Where(() => proposal.ProposalNumber == proposalNumber)
                .JoinQueryOver(() => proposal.Projects, () => project)
                .JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .Where(() => projectEstimate.Label == SnapshotLabel.Official)
                .SingleOrDefault();
        }

        public ProjectItem GetProjectItemByWtId(long id, DqeUser custodyUser)
        {
            InitializeSession();
            var userId = custodyUser.Id;
            ProjectItem projectItem = null;
            EstimateGroup estimateGroup = null;
            ProjectEstimate projectEstimate = null;
            ProjectVersion projectVersion = null;
            Project project = null;
            DqeUser versionOwner = null;
            DqeUser custodyOwner = null;
            return Session
                .QueryOver(() => projectItem)
                .Where(() => projectItem.WtId == id)
                .JoinQueryOver(() => projectItem.MyEstimateGroup, () => estimateGroup)
                .JoinQueryOver(() => estimateGroup.MyProjectEstimate, () => projectEstimate)
                .Where(() => projectEstimate.IsWorkingEstimate)
                .JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .JoinQueryOver(() => projectVersion.VersionOwner, () => versionOwner)
                .Where(() => versionOwner.Id == userId)
                .JoinQueryOver(() => projectVersion.MyProject, () => project)
                .JoinQueryOver(() => project.CustodyOwner, () => custodyOwner)
                .Where(() => custodyOwner.Id == userId)
                .SingleOrDefault();
        }

        public IEnumerable<Proposal> GetProposalByEstimateTypeAndCategory(string proposalNumber, SnapshotLabel estimateType)
        {
            InitializeSession();
            Proposal proposal = null;
            Project project = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;
            var q = Session.QueryOver(() => proposal)
                .WhereRestrictionOn(() => proposal.ProposalNumber).IsLike(proposalNumber, MatchMode.Start)
                .JoinQueryOver(() => proposal.Projects, () => project)
                .JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .Where(() => projectEstimate.Label == estimateType)
                .Select(Projections.Property(() => proposal.Id));
            return Session.QueryOver<Proposal>().WithSubquery.WhereProperty(i => i.Id).In((QueryOver<Proposal>)q).List();
        }

        private static void CreateProposalItems(Proposal proposalEstimate, List<PayItemMaster> payItems, ReportProposal reportProposal, DateTime dateForComparison, Domain.Model.Wt.Proposal wtProposal)
        {
            foreach (var section in proposalEstimate.SectionGroups)
            {
                foreach (var proposalItem in section.ProposalItems)
                {
                    var payItem = payItems.FirstOrDefault(i => i.RefItemName == proposalItem.PayItemNumber);

                    var reportProposalItem = new ReportProposalItem();
                    reportProposalItem.AlternateDescription = string.Empty;
                    reportProposalItem.CategoryAlternateMember = section.AlternateMember;
                    reportProposalItem.CategoryAlternateSet = section.AlternateSet;
                    reportProposalItem.CategoryDescription = string.Format("{0} {1}", section.Name, section.Description);
                    reportProposalItem.Description = proposalItem.PayItemDescription;
                    reportProposalItem.SupplementalDescription = proposalItem.SupplementalDescription ?? string.Empty;
                    reportProposalItem.DoNotBid = payItem != null && payItem.NonBid;
                    reportProposalItem.IsLowCost = reportProposal.ReportProposalSummaries
                        .Where(i => i.CategoryAlternateSet == section.AlternateSet)
                        .Where(i => i.CategoryAlternateMember == section.AlternateMember)
                        .Where(i => i.ItemAlternateSet == proposalItem.AlternateSet)
                        .First(i => i.ItemAlternateMember == proposalItem.AlternateMember).IsLowCost;
                    reportProposalItem.ItemAlternateMember = proposalItem.AlternateMember;
                    reportProposalItem.ItemAlternateSet = proposalItem.AlternateSet;
                    reportProposalItem.ItemNumber = proposalItem.PayItemNumber;
                    reportProposalItem.Price = proposalItem.ProjectItems.Average(i => i.Price);
                    reportProposalItem.Quantity = proposalItem.Quantity;
                    reportProposalItem.ObsoleteDate = payItem == null ? null : payItem.ObsoleteDate;
                    reportProposalItem.IsObsolete = reportProposalItem.ObsoleteDate.HasValue &&
                                                    reportProposalItem.ObsoleteDate.Value.Date < dateForComparison;
                    reportProposalItem.TechSpec = payItem == null ? string.Empty : payItem.Ilflg1 ?? string.Empty;
                    reportProposalItem.Unit = payItem == null
                        ? string.Empty //reportProposalItem.Unit
                        : payItem.CalculatedUnit.ToUpper().StartsWith("LS")
                            ? payItem.Unit.ToUpper() == "LS"
                                ? payItem.CalculatedUnit.Substring(0, 2)
                                : string.Format("{0} - {1}", payItem.CalculatedUnit.Substring(0,2), payItem.Unit)
                            : payItem.CalculatedUnit.Substring(0, 2);
                    reportProposalItem.WorkClass = payItem == null ? string.Empty : payItem.ContractClass ?? string.Empty;

                    foreach (
                        var ln in
                            wtProposal.Sections.Select(item => item.ProposalItems.FirstOrDefault(i => i.Id == proposalItem.WtId))
                                .Where(ln => ln != null))
                    {
                        reportProposalItem.LineNumber = ln.LineNumber;
                        break;
                    }
                    reportProposal.AddReportProposalItem(reportProposalItem);
                }
            }
        }

        private static ReportProposalSummary CreateReportProposalSummary(CategorySet categorySet, ItemSet itemSet)
        {
            var reportProposalSummary = new ReportProposalSummary
            {
                AlternateDescription = string.Empty,
                CategoryAlternateSet = categorySet.Set,
                CategoryAlternateMember = categorySet.Member,
                ItemAlternateSet = itemSet.Set,
                ItemAlternateMember = itemSet.Member,
                IsLowCost = categorySet.Included && itemSet.Included,
                Total = itemSet.Total
            };
            return reportProposalSummary;
        }

        private static ReportProposal CreateReportProposal(DqeUser owner, IWebTransportService webTransportService,
            Domain.Model.Wt.Proposal wtProposal, Proposal proposalEstimate, ReportProposalLevel proposalLevel)
        {
            var days = wtProposal.Milestones != null && wtProposal.Milestones.Any()
                ? wtProposal.Milestones.First().NumberOfUnits
                : Convert.ToInt64(0);
            var cost = wtProposal.Milestones != null && wtProposal.Milestones.Any()
                ? wtProposal.Milestones.First().RoadCostPerTimeUnit
                : (decimal)0;

            var code = webTransportService.GetCodeTable("WRKTYP");
            var workType = code.CodeValues.First(i => i.CodeValueName == wtProposal.ContractWorkType);

            var reportProposal = new ReportProposal
            {
                ConstructionDays = days ?? (long)0,
                CostPerDay = cost ?? (decimal)0,
                County = proposalEstimate.County.Name,
                Description = proposalEstimate.Description,
                District = wtProposal.District.Name,
                //HasOversight =
                ProposalLevel = proposalLevel,
                ProposalNumber = proposalEstimate.ProposalNumber,
                WorkType = workType.CodeValueName + " - " + workType.Description,
                LastUpdatedUser = owner.Name
            };
            return reportProposal;
        }

        private static void CreateReportProjectData(ReportProposal reportProposal, List<ProjectEstimate> projectEstimates, List<PayItemMaster> payItems, DateTime dateForComparison, Domain.Model.Wt.Proposal wtProposal, EstimateTotal proposalTotal, DqeUser owner)
        {
            foreach (var reportProject in reportProposal.ReportProjects)
            {
                //var estimate = projectEstimates.First(i => i.MyProjectVersion.MyProject.ProjectNumber == reportProject.ProjectNumber);
                var estimate = projectEstimates
                    .Where(i => i.IsWorkingEstimate)
                    .Where(i => i.MyProjectVersion.VersionOwner == owner)
                    .First(i => i.MyProjectVersion.MyProject.ProjectNumber == reportProject.ProjectNumber);


                foreach (var category in estimate.EstimateGroups)
                {
                    var reportCategory = CreateReportCategory(reportProposal, category);
                    reportProject.AddCategory(reportCategory);
                    foreach (var pi in category.ProjectItems)
                    {
                        var reportProjectItem = CreateReportProjectItem(reportProposal, payItems, dateForComparison, wtProposal, pi, category);

                        reportCategory.AddProjectItem(reportProjectItem);
                    }
                }
                foreach (var categorySet in proposalTotal.CategorySets)
                {
                    CreateReportProjectSummaries(reportProject, categorySet);
                }
            }
        }

        private static void CreateReportProjectSummaries(ReportProject reportProject, CategorySet categorySet)
        {
            var projectCategories = reportProject.ReportCategories
                .Where(r => r.CategoryAlternateMember.Equals(categorySet.Member))
                .Where(r => r.CategoryAlternateSet.Equals(categorySet.Set))
                .ToList();

            if (!projectCategories.Any()) return;

            foreach (var item in categorySet.ItemSets)
            {
                var projectItems = (from pj in projectCategories
                                    from pi in pj.ReportProjectItems
                                    where pi.ItemAlternateMember == item.Member
                                    where pi.ItemAlternateSet == item.Set
                                    select new { pi }).ToList();

                if (!projectItems.Any()) continue;

                var reportProjectSummary = new ReportProjectSummary
                {
                    AlternateDescription = string.Empty,
                    CategoryAlternateSet = categorySet.Set,
                    CategoryAlternateMember = categorySet.Member,
                    ItemAlternateSet = item.Set,
                    ItemAlternateMember = item.Member,
                    IsLowCost = item.Included,
                    Total = projectItems.Sum(i => i.pi.Total)
                };
                reportProject.AddReportProjectSummary(reportProjectSummary);
            }
        }

        private static ReportProjectItem CreateReportProjectItem(ReportProposal reportProposal, List<PayItemMaster> payItems, DateTime dateForComparison, Domain.Model.Wt.Proposal wtProposal, ProjectItem pi, EstimateGroup category)
        {
            var payItem = payItems.FirstOrDefault(i => i.RefItemName == pi.PayItemNumber);
            var reportProjectItem = new ReportProjectItem();
            reportProjectItem.Description = pi.PayItemDescription;
            reportProjectItem.SupplementalDescription = pi.SupplementalDescription ?? string.Empty;
            reportProjectItem.DoNotBid = payItem != null && payItem.NonBid;

            var summary = reportProposal.ReportProposalSummaries
                .Where(i => i.CategoryAlternateSet == category.AlternateSet)
                .Where(i => i.CategoryAlternateMember == category.AlternateMember)
                .Where(i => i.ItemAlternateSet == pi.AlternateSet)
                .FirstOrDefault(i => i.ItemAlternateMember == pi.AlternateMember);
            if (summary == null)
            {
                throw new InvalidOperationException("SYSTEMERROR:Not all project pay items are being displayed for this Proposal.  For DQE to display all pay items on a proposal, every project pay item must be assigned a section number within the proposal.");
            }

            //reportProjectItem.IsLowCost = reportProposal.ReportProposalSummaries
            //    .Where(i => i.CategoryAlternateSet == category.AlternateSet)
            //    .Where(i => i.CategoryAlternateMember == category.AlternateMember)
            //    .Where(i => i.ItemAlternateSet == pi.AlternateSet)
            //    .First(i => i.ItemAlternateMember == pi.AlternateMember).IsLowCost;

            reportProjectItem.IsLowCost = summary.IsLowCost;

            reportProjectItem.ItemAlternateMember = pi.AlternateMember;
            reportProjectItem.ItemAlternateSet = pi.AlternateSet;
            reportProjectItem.ItemNumber = pi.PayItemNumber;
            reportProjectItem.Price = pi.Price;
            reportProjectItem.Quantity = pi.Quantity;
            reportProjectItem.ObsoleteDate = payItem == null ? null : payItem.ObsoleteDate;
            reportProjectItem.IsObsolete = reportProjectItem.ObsoleteDate.HasValue &&
                                           reportProjectItem.ObsoleteDate.Value.Date < dateForComparison;
            reportProjectItem.TechSpec = payItem == null ? string.Empty : payItem.Ilflg1 ?? string.Empty;
            reportProjectItem.Unit = payItem == null
                ? string.Empty //reportProjectItem.Unit
                : payItem.CalculatedUnit.ToUpper().StartsWith("LS")
                    ? payItem.Unit.ToUpper() == "LS"
                        ? payItem.CalculatedUnit.Substring(0, 2)
                        : string.Format("{0} - {1}", payItem.CalculatedUnit.Substring(0, 2), payItem.Unit)
                    : payItem.CalculatedUnit.Substring(0, 2);
            reportProjectItem.WorkClass = payItem == null ? string.Empty : payItem.ContractClass ?? string.Empty;

            foreach (var section in wtProposal.Sections)
            {
                foreach (var proposalItem in section.ProposalItems)
                {
                    var wtPi = proposalItem.ProjectItems.FirstOrDefault(i => i.Id == pi.WtId);
                    if (wtPi != null)
                    {
                        reportProjectItem.LineNumber = wtPi.LineNumber;
                        break;
                    }
                }
            }
            if (reportProjectItem.LineNumber == null)
                reportProjectItem.LineNumber = string.Empty;

            return reportProjectItem;
        }

        private static ReportCategory CreateReportCategory(ReportProposal reportProposal, EstimateGroup category)
        {
            var reportCategory = new ReportCategory();
            reportCategory.AlternateDescription = string.Empty;
            reportCategory.CategoryAlternateMember = category.AlternateMember;
            reportCategory.CategoryAlternateSet = category.AlternateSet;
            reportCategory.Description = category.FederalConstructionClass + "00 " + category.Description;
            reportCategory.IsLowCost = reportProposal.ReportProposalSummaries
                .Where(i => i.CategoryAlternateSet == category.AlternateSet)
                .Where(i => i.CategoryAlternateMember == category.AlternateMember)
                .Any(i => i.IsLowCost);
            return reportCategory;
        }
    }
}