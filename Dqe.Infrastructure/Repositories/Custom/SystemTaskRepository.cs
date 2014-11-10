using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class SystemTaskRepository : BaseRepository, ISystemTaskRepository
    {
        public SystemTaskRepository() { }

        internal SystemTaskRepository(ISession session)
        {
            Session = session;
        }

        public SystemTask GetByTaskId(string taskId)
        {
            InitializeSession();
            return Session
                .QueryOver<SystemTask>()
                .Where(i => i.TaskId == taskId)
                .SingleOrDefault();
        }

        public IEnumerable<SystemTask> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<SystemTask>()
                .List();
        }
    }
}