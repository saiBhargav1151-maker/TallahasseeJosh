using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        public ProjectRepository() { }

        internal ProjectRepository(ISession session)
        {
            Session = session;
        }

        public ProjectItem GetProjectItem(long id)
        {
            InitializeSession();
            return Session
                .QueryOver<ProjectItem>()
                .Where(i => i.Id == id)
                //.Left.JoinQueryOver(i => i.ProposalHistories)
                //.Left.JoinQueryOver(i => i.BidHistories)
                .SingleOrDefault();
        }

        public IEnumerable<Project> GetProjects(DqeUser dqeUser)
        {
            InitializeSession();

            Project project = null;
            DqeUser dqeUserQuery = null;
            Proposal proposal = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;

            return Session
                .QueryOver(() => project)
                .Left.JoinQueryOver(() => project.CustodyOwner, () => dqeUserQuery)
                .Left.JoinQueryOver(() => project.Proposals, () => proposal)
                .Left.JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .Where(i => i.VersionOwner.Id == dqeUser.Id)
                .Left.JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .Where(i => i.LastUpdated > DateTime.Now.AddMonths(-3))
                .List()
                .Distinct();
        }

        public void Delete(long id)
        {
            InitializeSession();
            Session.CreateQuery("update DqeUser user set user.MyRecentProjectEstimate = null where exists (select 1 from DqeUser as u join u.MyRecentProjectEstimate as p where p.Id == :id)").SetParameter("id", id).ExecuteUpdate();
            Session.CreateQuery("update DqeUser user set user.MyRecentProjectEstimate = null where exists (select 1 from DqeUser as u join u.MyRecentProjectEstimate as p where p.Id == :id)").SetParameter("id", id).ExecuteUpdate();
        }

        public Project Get(long id)
        {
            InitializeSession();
            return Session
                .QueryOver<Project>()
                .Where(i => i.Id == id)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectEstimates)
                .OrderBy(i => i.Estimate).Desc
                .Left.JoinQueryOver(i => i.EstimateGroups)
                .Left.JoinQueryOver(i => i.ProjectItems)
                .SingleOrDefault();
        }

        public IEnumerable<Project> GetByProposalId(long id)
        {
            Proposal proposal = null;
            Project project = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;
            EstimateGroup estimateGroup = null;
            ProjectItem projectItem = null;
            MasterFile masterFile = null;
            DqeUser dqeUser = null;
            InitializeSession();
            return Session
                .QueryOver(() => project)
                .Left.JoinQueryOver(() => project.Proposals, () => proposal)
                .Left.JoinQueryOver(() => project.MyMasterFile, () => masterFile)
                .Left.JoinQueryOver(() => project.CustodyOwner, () => dqeUser)
                .Left.JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .Left.JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .Left.JoinQueryOver(() => projectEstimate.EstimateGroups, () => estimateGroup)
                .Left.JoinQueryOver(() => estimateGroup.ProjectItems, () => projectItem)
                .Where(() => proposal.Id == id)
                .OrderBy(() => projectVersion.Version).Asc
                .OrderBy(() => projectEstimate.Estimate).Desc
                .List();
        }

        public Project GetByProjectNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Project>()
                .Where(i => i.ProjectNumber == number)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectEstimates)
                .OrderBy(i => i.Estimate).Desc
                .Left.JoinQueryOver(i => i.EstimateGroups)
                .Left.JoinQueryOver(i => i.ProjectItems)
                .SingleOrDefault();
        }

        public Project GetDetailProjectForLsBd(string number, DqeUser owner)
        {
            InitializeSession();
            Project project = null;
            ProjectVersion projectVersion = null;
            ProjectEstimate projectEstimate = null;
            var lsdbDisjunction = new Disjunction();
            lsdbDisjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}LS", number)));
            lsdbDisjunction.Add(Restrictions.Where(() => project.ProjectNumber == string.Format("{0}DB", number)));
            return Session
                .QueryOver(() => project)
                .JoinQueryOver(() => project.ProjectVersions, () => projectVersion)
                .JoinQueryOver(() => projectVersion.ProjectEstimates, () => projectEstimate)
                .Where(lsdbDisjunction)
                .Where(() => projectVersion.VersionOwner == owner)
                .Where(() => projectEstimate.IsWorkingEstimate)
                .SingleOrDefault();
        }

        public ProjectEstimate GetEstimate(long id)
        {
            InitializeSession();
            ProjectEstimate projectEstimate = null;
            ProjectVersion projectVersion = null;
            Project project = null;
            EstimateGroup estimateGroup = null;
            ProjectItem projectItem = null;
            Proposal proposal = null;
            return Session
                .QueryOver(() => projectEstimate)
                .Where(i => i.Id == id)
                .JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .JoinQueryOver(() => projectVersion.MyProject, () => project)
                .Left.JoinQueryOver(() => project.Proposals, () => proposal)
                .Left.JoinQueryOver(() => projectEstimate.EstimateGroups, () => estimateGroup)
                .Left.JoinQueryOver(() => estimateGroup.ProjectItems, () => projectItem)
                .SingleOrDefault();
        }

        public Project GetByEstimateId(long id)
        {
            InitializeSession();
            var q = Session
                .QueryOver<Project>()
                .Select(Projections.Distinct(Projections.Property<Project>(i => i.Id)))
                .Inner.JoinQueryOver(i => i.ProjectVersions)
                .Inner.JoinQueryOver(i => i.ProjectEstimates)
                .Where(i => i.Id == id);
            return Session
                .QueryOver<Project>()
                .WithSubquery
                .WhereProperty(i => i.Id).Eq((QueryOver<Project, ProjectEstimate>)q)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectEstimates)
                .OrderBy(i => i.Estimate).Desc
                .SingleOrDefault();
        }

        public Project GetByVersionId(long id)
        {
            InitializeSession();
            var q = Session
                .QueryOver<Project>()
                .Select(Projections.Distinct(Projections.Property<Project>(i => i.Id)))
                .Inner.JoinQueryOver(i => i.ProjectVersions)
                .Where(i => i.Id == id)
                .Inner.JoinQueryOver(i => i.ProjectEstimates);
            return Session
                .QueryOver<Project>()
                .WithSubquery
                .WhereProperty(i => i.Id).Eq((QueryOver<Project, ProjectEstimate>)q)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectEstimates)
                .OrderBy(i => i.Estimate).Desc
                .SingleOrDefault();
        }

        public int GetMaxEstimate(string number, int version)
        {
            InitializeSession();
            var l = Session
                .QueryOver<ProjectEstimate>()
                .JoinQueryOver(i => i.MyProjectVersion)
                .Where(i => i.Version == version)
                .JoinQueryOver(i => i.MyProject)
                .Where(i => i.ProjectNumber == number)
                .List()
                .Distinct()
                .ToList();
            return !l.Any() ? 0 : l.Max(i => i.Estimate);
        }

        public int GetMaxVersion(string number)
        {
            InitializeSession();
            var l = Session
                .QueryOver<ProjectVersion>()
                .JoinQueryOver(i => i.MyProject)
                .Where(i => i.ProjectNumber == number)
                .List()
                .Distinct()
                .ToList();
            return !l.Any() ? 0 : l.Max(i => i.Version);
        }
    }
}