using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PayItemRepository : IPayItemRepository
    {
        public PayItem Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<PayItem>(id);
        }

        public IEnumerable<PayItem> GetByStructure(int payItemStructureId)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItem>()
                .OrderBy(i => i.PayItemId).Asc
                .Inner.JoinQueryOver(i => i.MyPayItemStructure)
                .Where(i => i.Id == payItemStructureId)
                .List();
        }
    }
}