using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class MasterFileRepository : IMasterFileRepository
    {
        public MasterFile Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<MasterFile>(id);
        }

        public MasterFile GetByFileNumber(int fileNumber)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<MasterFile>()
                .Where(i => i.FileNumber == fileNumber)
                .SingleOrDefault();
        }

        public IEnumerable<MasterFile> GetAll()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<MasterFile>()
                .OrderBy(i => i.FileNumber).Desc
                .List();
        } 
    }
}