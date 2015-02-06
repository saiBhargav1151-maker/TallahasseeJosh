using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class MasterFileRepository : BaseRepository, IMasterFileRepository
    {
        public MasterFileRepository() { }

        internal MasterFileRepository(ISession session)
        {
            Session = session;
        }

        public MasterFile Get(int id)
        {
            InitializeSession();
            return Session.Get<MasterFile>(id);
        }

        public MasterFile GetByFileNumber(int fileNumber)
        {
            InitializeSession();
            return Session
                .QueryOver<MasterFile>()
                .Where(i => i.FileNumber == fileNumber)
                .SingleOrDefault();
        }

        public IEnumerable<MasterFile> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<MasterFile>()
                .OrderBy(i => i.FileNumber).Desc
                .List();
        } 
    }
}