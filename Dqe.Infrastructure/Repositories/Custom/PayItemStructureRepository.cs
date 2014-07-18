using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PayItemStructureRepository : IPayItemStructureRepository
    {
        public PayItemStructure Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<PayItemStructure>(id);
        }

        public PayItemStructure GetByStructureId(string structureId, int thisPayItemStructureId)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItemStructure>()
                .Where(i => i.StructureId == structureId)
                .Where(i => i.Id != thisPayItemStructureId)
                .SingleOrDefault();
        }

        public IEnumerable<PayItemStructure> GetAll()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItemStructure>()
                .List();
        } 
    }
}