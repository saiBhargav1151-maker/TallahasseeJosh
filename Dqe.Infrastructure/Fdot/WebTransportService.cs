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
using Dqe.Domain.Model.Wt;
using Dqe.Infrastructure.Repositories;
using FDOT.Enterprise;
using FDOT.Enterprise.ConnectionStrings.Client;
using NHibernate.Criterion;

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
                    .List()
                    .OrderBy(i => i.ProjectNumber)
                    .ToList();
            }
        }

        public Project GetProject(string number)
        {
            Proposal proposal = null;

            using (var session = Initializer.TransportSessionFactory.OpenSession())
            {
                return session
                    .QueryOver<Project>()
                    .Where(i => i.ProjectNumber == number)
                    .Left.JoinQueryOver(i => i.MyProposal, () => proposal)
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
                return session
                    .QueryOver<Proposal>()
                    .Where(i => i.ProposalNumber == number)
                    .Fetch(i => i.MyLetting).Eager
                    .Fetch(i => i.District).Eager
                    .Fetch(i => i.County).Eager
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

        public bool IsProjectSynced(Domain.Model.ProjectEstimate projectEstimate)
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
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
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
                //var lettingDate = (wtProject.MyProposal == null)
                //    ? (DateTime?) null
                //    : wtProject.MyProposal.MyLetting.LettingDate;
                if (dqeProject.Description != wtProject.Description
                    //|| dqeProject.DesignerName != wtProject.Designer
                    || dqeProject.District != wtProject.Districts.First(i => i.PrimaryDistrict).MyRefDistrict.Name
                    //|| dqeProject.LettingDate != lettingDate
                    || dqeProject.MyCounty.Name != wtProject.Counties.First(i => i.PrimaryCounty).MyRefCounty.Description
                    || dqeProject.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0') != wtProject.SpecBook
                    || dqeProject.WtId != wtProject.Id)
                {
                    return false;
                }
                if (projectEstimate.EstimateGroups.Count() != wtProject.Categories.Count())
                {
                    return false;
                }
                foreach (var estimateGroup in projectEstimate.EstimateGroups)
                {
                    var eg = estimateGroup;
                    var egMatch = wtProject
                        .Categories
                        .Where(i => i.CombineLikeItems == eg.CombineWithLikeItems)
                        .Where(i => string.IsNullOrWhiteSpace(i.AlternateMember) ? string.IsNullOrWhiteSpace(eg.AlternateMember) : eg.AlternateMember == i.AlternateMember)
                        .Where(i => i.MyCategoryAlternate == null ? string.IsNullOrWhiteSpace(eg.AlternateSet) : i.MyCategoryAlternate.Name == eg.AlternateSet)
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
                            //.Where(i => i.LineNumber == pi.LineNumber)
                            .Where(i => i.MyAlternate == null ? string.IsNullOrWhiteSpace(pi.AlternateSet) : pi.AlternateSet == i.MyAlternate.Name)
                            .Where(i => i.MyRefItem.Name == pi.PayItemNumber)
                            .Where(i => i.Quantity == pi.Quantity)
                            .FirstOrDefault(i => i.Id == pi.WtId);
                        if (piMatch == null)
                        {
                            return false;
                        }
                    }
                }
                return true;
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

                var o = session
                    .QueryOver(() => project)
                    .Where(() => project.ProjectNumber == projectNumber)
                    .Left.JoinQueryOver(() => project.Districts, () => district)
                    .Left.JoinQueryOver(() => project.MyProposal, () => proposal)
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
                return o;
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
                    .Left.JoinQueryOver(i => i.Sections)
                    .Left.JoinQueryOver(i => i.ProposalItems)
                    .SingleOrDefault();
            }
        }
    }
}