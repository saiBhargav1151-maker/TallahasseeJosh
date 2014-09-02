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


        public Project GetByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Project>()
                .Where(i => i.ProjectNumber == number)
                .SingleOrDefault();
        }
    }
}