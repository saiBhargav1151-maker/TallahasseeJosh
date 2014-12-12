using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Xml.Serialization;
using Dqe.Domain.Fdot;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Wt;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories;
using Dqe.Infrastructure.Repositories.Custom;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
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
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session.QueryOver<CodeTable>()
                        .Where(i => i.CodeTableName == codeType)
                        .Fetch(i => i.CodeValues).Eager
                        .SingleOrDefault();
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
            //var wTservice = new WebTransportService();
            var sys = new DqeUserRepository().GetSystemAccount();
            var payItemNumbers = GetDistinctRefItemNumbers();
            var x = 0;
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                foreach (var payItemNumber in payItemNumbers)
                {
                    //if (x > 1000) break;
                    //logic
                    var number = payItemNumber;
                    var items = session
                    .QueryOver<RefItem>()
                    .Where(i => i.Name == number)
                    .List();
                    var specBooks = items.Select(i => i.SpecBook).ToList();
                    for (var i = 0; i < specBooks.Count(); i++)
                    {
                        if (specBooks[i].StartsWith("9"))
                        {
                            specBooks[i] = string.Format("0{0}", specBooks[i]);
                        }
                        else
                        {
                            specBooks[i] = string.Format("1{0}", specBooks[i]);
                        }
                    }
                    specBooks = specBooks.OrderByDescending(i => i).ToList();
                    var maxSpecBook = specBooks.First().Substring(1);
                    var item = items.First(i => i.SpecBook == maxSpecBook);
                    var pim = new PayItemMaster();
                    var pimt = pim.GetTransformer();
                    pimt.Administrative = item.Administrative;
                    pimt.AlternateItemName = item.AlternateItemName;
                    pimt.AutoPaidPercentSchedule = item.AutoPaidPercentSchedule;
                    pimt.BidAsLumpSum = item.BidAsLumpSum;
                    pimt.BidRequirementCode = item.BidRequirementCode;
                    pimt.CalculatedUnit = item.CalculatedUnit;
                    pimt.CoApprovalRequired = item.CoApprovalRequired;
                    pimt.CombineWithLikeItems = item.CombineWithLikeItems;
                    pimt.CommonUnit = item.CommonUnit;
                    pimt.ContractClass = item.ContractClass;
                    pimt.ConversionFactorToCommonUnit = item.ConversionFactorToCommonUnits;
                    pimt.CreatedBy = item.CreatedBy;
                    pimt.CreatedDate = item.CreatedDate;
                    pimt.DbeInterest = item.DbeInterest;
                    pimt.DbePercentToApply = item.DbePercentToApply;
                    pimt.Description = item.Description;
                    pimt.ExemptFromMaa = item.ExemptFromMaa;
                    pimt.ExemptFromRetainage = item.ExemptFromRetainage;
                    pimt.FuelAdjustment = item.FuelAdjustment;
                    pimt.FuelAdjustmentType = item.FuelAdjustmentType;
                    pimt.Ildt1 = item.IlDate1;
                    pimt.Ildt2 = item.IlDate2;
                    pimt.Ildt3 = item.IlDate3;
                    pimt.Ilflg1 = item.IlFlag1;
                    pimt.Illst1 = item.Illst1;
                    pimt.Ilnum1 = item.IlNumber1;
                    pimt.Ilsst1 = item.Ilsst1;
                    pimt.ItemClass = item.ItemClass;
                    pimt.ItemType = item.ItemType;
                    pimt.LastUpdatedBy = item.LastUpdatedBy;
                    pimt.LastUpdatedDate = item.LastUpdatedDate;
                    pimt.LumpSum = item.LumpSum;
                    pimt.MajorItem = item.MajorItem;
                    pimt.NonBid = item.NonBid;
                    pimt.ObsoleteDate = item.ObsoleteDate;
                    pimt.PayPlan = item.PayPlan;
                    pimt.PercentScheduleItem = item.PercentScheduleItem;
                    pimt.RecordSource = item.RecordSource;
                    pimt.RefItemName = item.Name;
                    pimt.RefPrice = item.Price;
                    pimt.RegressionInclusion = item.RegressionInclusion;
                    pimt.ShortDescription = item.ShortDescription;
                    pimt.SpecBook = item.SpecBook;
                    pimt.SpecialtyItem = item.SpecialtyItem;
                    pimt.SuppDescriptionRequired = item.SuppDescriptionRequired;
                    pimt.Unit = item.Unit;
                    pimt.UnitSystem = item.UnitSystem;
                    pim.Transform(pimt, sys);
                    UnitOfWorkProvider.CommandRepository.Add(pim);
                    x += 1;
                    Console.WriteLine(x);
                }
            }
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

        public IEnumerable<Project> GetProjects(string number)
        {
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Project>()
                    .WhereRestrictionOn(i => i.ProjectNumber).IsInsensitiveLike(number, MatchMode.Start)
                    .Where(i => i.IsValid != "Y")
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
                var proposalDisjunction = new Disjunction();
                proposalDisjunction.Add(Restrictions.Where(() => !proposal.IsRejected));
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                proposalDisjunction.Add(Restrictions.Where(() => proposal.IsRejected == null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                return session
                    .QueryOver(() => project)
                    .Where(i => i.ProjectNumber == number)
                    .Where(i => i.IsValid != "Y")
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => district.MyRefDistrict)
                    .Left.JoinQueryOver(() => project.Counties, () => county)
                    .Left.JoinQueryOver(() => county.MyRefCounty)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(proposalDisjunction)
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
                    .Left.JoinQueryOver(i => i.Sections)
                    .Left.JoinQueryOver(i => i.ProposalItems, () => proposalItem)
                    .Left.JoinQueryOver(() => proposalItem.MyRefItem)
                    .Left.JoinQueryOver(() => proposalItem.ProjectItems)
                    .SingleOrDefault();
            }
        }

        //public Estimate ExportProject(string projectNumber)
        //{
        //    var token = ChannelProvider<IConnectionStringService>.Default.GetConnectionToken("DQEWT_SRV");
        //    var svc = new WtEstimatorService.EstimatorServiceClient();
        //    if (svc.ClientCredentials == null) throw new ServiceActivationException("ClientCredentials cannot be null"); 
        //    svc.ClientCredentials.UserName.UserName = token.UserId;
        //    svc.ClientCredentials.UserName.Password = token.Password;
        //    try
        //    {
        //        var project = svc.ExportProject(projectNumber);
        //        var serializer = new XmlSerializer(typeof (Estimate));
        //        using (var stream = new StringReader(project))
        //        {
        //            return (Estimate)serializer.Deserialize(stream);
        //        }
        //    }
        //    catch
        //    {
        //        return null;   
        //    }
        //}

        public bool IsProjectSynced(ProjectEstimate projectEstimate)
        {

            


            var dqeProject = projectEstimate.MyProjectVersion.MyProject;
            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {

                //var lsProjects = session.QueryOver<Project>()
                //    .WhereRestrictionOn(i => i.ProjectNumber)
                //    .IsInsensitiveLike("LS", MatchMode.End)
                //    .List();

                Project project = null;
                Category category = null;
                County county = null;
                District district = null;
                ProjectItem projectItem = null;
                Proposal proposal = null;

                var proposalDisjunction = new Disjunction();
                proposalDisjunction.Add(Restrictions.Where(() => !proposal.IsRejected));
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                proposalDisjunction.Add(Restrictions.Where(() => proposal.IsRejected == null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalse

                var wtProject = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == dqeProject.ProjectNumber)
                    .Where(i => i.IsValid != "Y")
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(proposalDisjunction)
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
                if (!ValidateProjectCategorySynch(estimateGroups, wtProject)) return false;
                var disjunction = new Disjunction();
                disjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}LS", dqeProject.ProjectNumber)));
                disjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}DB", dqeProject.ProjectNumber)));
                var lsdb = session
                    .QueryOver(() => project)
                    .Where(disjunction)
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Categories, () => category)
                    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                    .Left.JoinQueryOver(() => projectItem.MyFundPackage)
                    .SingleOrDefault();
                if (lsdb == null) return true;
                estimateGroups = projectEstimate.EstimateGroups.Where(i => !i.IsLsDbSummary).ToList();
                return ValidateProjectCategorySynch(estimateGroups, lsdb);
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
                //var categoryAlternates = session
                //    .QueryOver(() => project)
                //    .Left.JoinQueryOver(() => project.Districts, () => district)
                //    .Left.JoinQueryOver(() => project.MyProposal)
                //    .Left.JoinQueryOver(() => district.MyRefDistrict)
                //    .Left.JoinQueryOver(() => project.Counties, () => county)
                //    .Left.JoinQueryOver(() => county.MyRefCounty)
                //    .Left.JoinQueryOver(() => project.Categories, () => category)
                //    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                //    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                //    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                //    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                //    .Where(() => category.MyCategoryAlternate != null)
                //    .List()
                //    .Distinct();
                //var alternates = session
                //    .QueryOver(() => project)
                //    .Left.JoinQueryOver(() => project.Districts, () => district)
                //    .Left.JoinQueryOver(() => project.MyProposal)
                //    .Left.JoinQueryOver(() => district.MyRefDistrict)
                //    .Left.JoinQueryOver(() => project.Counties, () => county)
                //    .Left.JoinQueryOver(() => county.MyRefCounty)
                //    .Left.JoinQueryOver(() => project.Categories, () => category)
                //    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                //    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                //    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                //    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                //    .Where(() => projectItem.MyAlternate != null)
                //    .List()
                //    .Distinct();
                //var lsProjects = session
                //    .QueryOver<Project>()
                //    .WhereRestrictionOn(i => i.ProjectNumber).IsInsensitiveLike("LS", MatchMode.End)
                //    .List();

                var proposalDisjunction = new Disjunction();
                proposalDisjunction.Add(Restrictions.Where(() => !proposal.IsRejected));
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                proposalDisjunction.Add(Restrictions.Where(() => proposal.IsRejected == null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                

                var p = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == projectNumber)
                    .Where(i => i.IsValid != "Y")
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
                    .Where(proposalDisjunction)
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
                //pull potential LS/DB details
                var disjunction = new Disjunction();
                disjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}LS", projectNumber)));
                disjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}DB", projectNumber)));
                var lsdb = session
                    .QueryOver(() => project)
                    .Where(disjunction)
                    .Where(i => i.IsLatestVersion)
                    .Left.JoinQueryOver(() => project.Categories, () => category)
                    .Left.JoinQueryOver(() => category.MyCategoryAlternate)
                    .Left.JoinQueryOver(() => category.ProjectItems, () => projectItem)
                    .Left.JoinQueryOver(() => projectItem.MyRefItem)
                    .Left.JoinQueryOver(() => projectItem.MyAlternate)
                    .Left.JoinQueryOver(() => projectItem.MyFundPackage)
                    .SingleOrDefault();
                if (lsdb != null)
                {
                    //LS/DB project
                    p.LsDbId = lsdb.Id;
                    p.LsDbCode = lsdb.ProjectNumber.ToUpper().EndsWith("LS") ? "LS" : "DB";
                    foreach (var cat in p.Categories)
                    {
                        //cat.IsLsDbDetail = lsdb.ProjectNumber.ToUpper().EndsWith("LS");
                        cat.IsLsDbDetail = false;
                    }
                    foreach (var cat in lsdb.Categories)
                    {
                        //cat.IsLsDbDetail = !lsdb.ProjectNumber.ToUpper().EndsWith("LS");
                        cat.IsLsDbDetail = true;
                        p.AddCategory(cat);
                    }                    
                }
                return p;
            }
        }

        //public Estimate ExportProposal(string proposalNumber)
        //{
        //    var token = ChannelProvider<IConnectionStringService>.Default.GetConnectionToken("DQEWT_SRV");
        //    var svc = new WtEstimatorService.EstimatorServiceClient();
        //    if (svc.ClientCredentials == null) throw new ServiceActivationException("ClientCredentials cannot be null");
        //    svc.ClientCredentials.UserName.UserName = token.UserId;
        //    svc.ClientCredentials.UserName.Password = token.Password;
        //    try
        //    {
        //        var proposal = svc.ExportProposal(proposalNumber);
        //        var serializer = new XmlSerializer(typeof(Estimate));
        //        using (var stream = new StringReader(proposal))
        //        {
        //            return (Estimate)serializer.Deserialize(stream);
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

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

        public object GetBidHistory(string number, int range)
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
                var proposalDisjunction = new Disjunction();
                //TODO: include rejected proposals?
                proposalDisjunction.Add(Restrictions.Where(() => !proposal.IsRejected));
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                proposalDisjunction.Add(Restrictions.Where(() => proposal.IsRejected == null));
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                var results = session
                    .QueryOver(() => proposalItem)
                    .JoinQueryOver(() => proposalItem.Bids, () => bid)
                    .JoinQueryOver(() => proposalItem.MyRefItem, () => refItem)
                    .JoinQueryOver(() => bid.MyProposalVendor, () => proposalVendor)
                    .JoinQueryOver(() => proposalVendor.MyProposal, () => proposal)
                    .JoinQueryOver(() => proposal.MyLetting, () => letting)
                    .JoinQueryOver(() => proposal.County, () => refCounty)
                    .Where(proposalDisjunction)
                    .Where(() => refItem.Name == number)    
                    .Where(() => letting.LettingDate < DateTime.Now.Date)
                    .Where(() => letting.LettingDate >= DateTime.Now.Date.AddMonths(range*-1).Date)
                    .Where(() => proposalVendor.BidType == "RESP")
                    .Where(() => proposalVendor.BidStatus != "I")
                    //.Where(() => bid.ValidBid)
                    .WhereRestrictionOn(() => proposal.ProposalStatus).IsIn(new object[]{"01", "02", "03", "22"})
                    .OrderBy(() => letting.LettingDate).Desc
                    .OrderBy(() => proposal.ProposalNumber).Asc
                    .OrderBy(() => bid.BidPrice).Asc
                    .Select
                    (
                        Projections.Property(() => proposalItem.Id).As("Id"),
                        Projections.Property(() => proposal.ProposalNumber).As("ProposalNumber"),
                        Projections.Property(() => refCounty.Description).As("County"),
                        Projections.Property(() => letting.LettingDate).As("LettingDate"),
                        Projections.Property(() => bid.BidPrice).As("BidPrice"),
                        Projections.Property(() => bid.LowCost).As("LowCost"),
                        Projections.Property(() => proposalVendor.Awarded).As("Awarded")
                    )
                    .TransformUsing(new DynamicTransformer())
                    .List<dynamic>();
                var distinctProposals = results.Select(i => i.Id).Distinct().ToList();
                var maxBidders = distinctProposals.Select(distinctProposal => results.Count(i => i.Id == distinctProposal)).Concat(new[] {0}).Max();
                var proposals = new List<ApplicationServices.ProposalHistory>();
                foreach (var p in distinctProposals)
                {
                    var localP = p;
                    var pResults = results.Where(i => i.Id == localP).ToList();
                    var blankCount = maxBidders - pResults.Count;
                    var blanks = new List<ApplicationServices.Bid>();
                    for (var i = 0; i < blankCount; i++)
                    {
                        blanks.Add(new ApplicationServices.Bid
                        {
                            IsBlank = true,
                            Price = (dynamic)0,
                            Included = false,
                            IsLowCost = false,
                            IsAwarded = false
                        });
                    }
                    var bids = pResults.Select(i => new ApplicationServices.Bid
                    {
                        IsBlank = false,
                        Price = i.BidPrice,
                        Included = true,
                        IsLowCost = (bool)i.LowCost,
                        IsAwarded = (bool)i.Awarded
                    }).Concat(blanks);
                    var pr = new ApplicationServices.ProposalHistory
                    {
                        Proposal = results.First(i => i.Id == p).ProposalNumber,
                        County = results.First(i => i.Id == p).County,
                        Included = true,
                        Letting = results.First(i => i.Id == p).LettingDate,
                    };
                    bids.ForEach(b => pr.Bids.Add(b)); 
                    proposals.Add(pr);
                }
                var c = 0;
                var history = new ApplicationServices.BidHistory();
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
                return history;
                //var proposals = new List<dynamic>();
                //foreach (var p in distinctProposals)
                //{
                //    var localP = p;
                //    var pResults = results.Where(i => i.Id == localP).ToList();
                //    var blankCount = maxBidders - pResults.Count;
                //    var blanks = new List<object>();
                //    for (var i = 0; i < blankCount; i++)
                //    {
                //        blanks.Add(new
                //        {
                //            blank = true,
                //            price = (dynamic) 0,
                //            include = false,
                //            lowCost = false,
                //            awarded = false
                //        });
                //    }
                //    var bids = pResults.Select(i => new
                //    {
                //        blank = false,
                //        price = i.BidPrice,
                //        include = true,
                //        lowCost = (bool) i.LowCost,
                //        awarded = (bool) i.Awarded
                //    }).Concat(blanks);
                //    proposals.Add(new
                //    {
                //        proposal = results.First(i => i.Id == p).ProposalNumber,
                //        county = results.First(i => i.Id == p).County,
                //        include = true,
                //        letting = results.First(i => i.Id == p).LettingDate.ToShortDateString(),
                //        bids
                //    });
                //}
                //var c = 0;
                //var history = new
                //{
                //    maxBiddersProposal = maxBidders == 0
                //        ? null
                //        : new
                //        {
                //            bids = new int[maxBidders].Select(ii => new
                //            {
                //                number = c += 1
                //            })
                //        },
                //    proposals
                //};
                //return history;
            }
        }
    }
}