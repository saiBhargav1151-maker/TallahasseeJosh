using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

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
                .Fetch(i => i.Owner).Eager
                .Left.JoinQueryOver(i => i.EstimateGroups)
                .Left.JoinQueryOver(i => i.ProjectItems)
                .SingleOrDefault();
        }

        public IEnumerable<Project> GetAllByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Project>()
                .Where(i => i.ProjectNumber == number)
                .Fetch(i => i.MyMasterFile).Eager
                .Fetch(i => i.Owner).Eager
                .Left.JoinQueryOver(i => i.EstimateGroups)
                .Left.JoinQueryOver(i => i.ProjectItems)
                .List()
                .Distinct();
        }

        public int GetMaxVersionForOwner(string number, DqeUser owner)
        {
            InitializeSession();
            var l = Session
                .QueryOver<Project>()
                .Where(i => i.ProjectNumber == number)
                .Inner.JoinQueryOver(i => i.Owner)
                .Where(i => i.Id == owner.Id)
                .List()
                .Distinct()
                .ToList();
            return !l.Any() ? 0 : l.Max(i => i.Version);
        }
    }
}