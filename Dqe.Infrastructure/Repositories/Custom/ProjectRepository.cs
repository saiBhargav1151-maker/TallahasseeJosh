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
                .Left.JoinQueryOver(i => i.ProjectSnapshots)
                .OrderBy(i => i.Snapshot).Desc
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
                .Left.JoinQueryOver(i => i.ProjectSnapshots)
                .OrderBy(i => i.Snapshot).Desc
                .SingleOrDefault();
        }

        public Project GetBySnapshotId(int id)
        {
            InitializeSession();
            var q = Session
                .QueryOver<Project>()
                .Select(i => i.Id)
                .Inner.JoinQueryOver(i => i.ProjectVersions)
                .Inner.JoinQueryOver(i => i.ProjectSnapshots)
                .Where(i => i.Id == id);
            return Session
                .QueryOver<Project>()
                .WithSubquery
                .WhereProperty(i => i.Id).Eq((QueryOver<Project, ProjectSnapshot>) q)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.CustodyOwner).Eager
                .Left.JoinQueryOver(i => i.ProjectVersions)
                .OrderBy(i => i.Version).Asc
                .Left.JoinQueryOver(i => i.ProjectSnapshots)
                .OrderBy(i => i.Snapshot).Desc
                .SingleOrDefault();
        }

        public int GetMaxSnapshot(string number, int version)
        {
            InitializeSession();
            var l = Session
                .QueryOver<ProjectSnapshot>()
                .JoinQueryOver(i => i.MyProjectVersion)
                .Where(i => i.Version == version)
                .JoinQueryOver(i => i.MyProject)
                .Where(i => i.ProjectNumber == number)
                .List()
                .Distinct()
                .ToList();
            return !l.Any() ? 0 : l.Max(i => i.Snapshot);
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