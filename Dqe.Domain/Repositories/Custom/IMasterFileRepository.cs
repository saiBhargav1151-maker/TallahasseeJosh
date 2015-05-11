using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IMasterFileRepository
    {
        MasterFile Get(long id);
        MasterFile GetByFileNumber(int fileNumber);
        IEnumerable<MasterFile> GetAll();
    }
}