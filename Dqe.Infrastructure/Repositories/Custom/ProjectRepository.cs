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