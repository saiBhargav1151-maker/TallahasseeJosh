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

        public ProjectItem GetProjectItem(int id)
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
            Project project = null;

            InitializeSession();
            return Session
                .QueryOver(() => project)
                .Left.JoinQueryOver(() => project.CustodyOwner)
                .Left.JoinQueryOver(() => project.Proposals)
                .Left.JoinQueryOver(() => project.ProjectVersions)
                .Where(i => i.VersionOwner.Id == dqeUser.Id)
                .List()
                .Distinct();
        }

        public Project Get(int id)
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

        public IEnumerable<Project> GetByProposalId(int id)
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

        public ProjectEstimate GetEstimate(int id)
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

        public Project GetByEstimateId(int id)
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
                .WhereProperty(i => i.Id).Eq((QueryOver<Project, ProjectEstimate>) q)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectEstimates)
                .OrderBy(i => i.Estimate).Desc
                .SingleOrDefault();
        }

        public Project GetByVersionId(int id)
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