using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;
using NHibernate.Criterion;

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

        public PayItem GetByNumberAndMasterFile(string number, int masterFile)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItem>()
                .Where(i => i.PayItemId == number)
                .JoinQueryOver(i => i.MyMasterFile)
                .Where(i => i.FileNumber == masterFile)
                .SingleOrDefault();
        }

        public IEnumerable<PayItem> GetByNumber(string number)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItem>()
                .Where(i => i.PayItemId == number)
                .List();
        }

        public IEnumerable<PayItem> GetAll()
        {
            var structureObsoleteDateDisjunction = new Disjunction();
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate == null));
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate >= DateTime.Now));
            var itemObsoleteDateDisjunction = new Disjunction();
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate == null));
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate.Value >= DateTime.Now));
            PayItemStructure payItemStructure = null;
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PayItem>()
                .JoinAlias(p=>p.MyPayItemStructure, ()=>payItemStructure)
                .Where(i => i.EffectiveDate <= DateTime.Now)
                .Where(structureObsoleteDateDisjunction)
                .Where(itemObsoleteDateDisjunction)
                .OrderBy(i => i.PayItemId).Asc
                .Left.JoinQueryOver(i => i.MyMasterFile)
                .OrderBy(i => i.FileNumber).Asc
                .List()
                .Distinct();
        }
    }
}