using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<PayItemStructure> GetAll(bool viewAll)
        {
            CostBasedTemplate costBasedTemplate = null;

            if (viewAll)
            {
                return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItemStructure>()
                .Left.JoinAlias(i => i.MyCostBasedTemplate, () => costBasedTemplate)
                .OrderBy(i => i.StructureId).Asc
                .Left.JoinQueryOver(i => i.PayItems)
                .OrderBy(i => i.PayItemId).Asc
                .Left.JoinQueryOver(i => i.MyMasterFile)
                .OrderBy(i => i.FileNumber).Asc
                .List()
                .Distinct();
            }
            var structureObsoleteDateDisjunction = new Disjunction();
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate == null));
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate >= DateTime.Now));
            var itemObsoleteDateDisjunction = new Disjunction();
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate == null));
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate.Value >= DateTime.Now));
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItemStructure>()
                .Where(i => i.EffectiveDate <= DateTime.Now)
                .Where(structureObsoleteDateDisjunction)
                .Left.JoinAlias(i => i.MyCostBasedTemplate, () => costBasedTemplate)
                .OrderBy(i => i.StructureId).Asc
                .Left.JoinQueryOver(i => i.PayItems)
                .Where(itemObsoleteDateDisjunction)
                .OrderBy(i => i.PayItemId).Asc
                .Left.JoinQueryOver(i => i.MyMasterFile)
                .OrderBy(i => i.FileNumber).Asc
                .List()
                .Distinct();
        }
    }
}