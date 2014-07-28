using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemRepository
    {
        PayItem Get(int id);
        IEnumerable<PayItem> GetByStructure(int payItemStructureId);
    }
}