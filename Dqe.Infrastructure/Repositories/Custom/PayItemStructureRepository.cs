using System.Collections.Generic;
using System.Globalization;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;
using NHibernate.Criterion;

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

        public IEnumerable<PayItemStructure> GetGroup(int panel)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItemStructure>()
                .WhereRestrictionOn(i => i.StructureId).IsLike(panel < 10 ? string.Format("0{0}", panel) : panel.ToString(CultureInfo.InvariantCulture), MatchMode.Start)
                .OrderBy(i => i.StructureId).Asc
                .List();
        } 
    }
}