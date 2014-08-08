using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemRepository
    {
        PayItem Get(int id);
        PayItem GetByNumberAndMasterFile(string number, int masterFile);
        IEnumerable<PayItem> GetByNumber(string number);
        IEnumerable<PayItem> GetAll();
    }
}