using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PayItemStructureRepository : BaseRepository, IPayItemStructureRepository
    {
        public PayItemStructureRepository() { }

        internal PayItemStructureRepository(ISession session)
        {
            Session = session;
        }

        public PayItemStructure Get(int id)
        {
            InitializeSession();
            return Session.Get<PayItemStructure>(id);
        }

        public PayItemStructure GetByStructureId(string structureId, int? thisPayItemStructureId)
        {
            InitializeSession();
            var q = Session
                .QueryOver<PayItemStructure>()
                .Where(i => i.StructureId == structureId);
            return thisPayItemStructureId.HasValue 
                ? q.Where(i => i.Id != thisPayItemStructureId).SingleOrDefault() 
                : q.SingleOrDefault();
        }

        public IEnumerable<PayItemStructure> GetAll(bool viewAll, int range)
        {
            InitializeSession();
            CostBasedTemplate costBasedTemplate = null;
            ICriterion restriction; 
            if (range < 10)
            {
                restriction = Restrictions.On<PayItemStructure>(i => i.StructureId)
                        .IsLike(range.ToString(CultureInfo.InvariantCulture)
                        .PadLeft(2, '0'), MatchMode.Start);
            }
            else if (range == 10)
            {
                restriction = Restrictions.Not(Restrictions.On<PayItemStructure>(i => i.StructureId).IsLike("0", MatchMode.Start));
            }
            else
            {
                restriction = Restrictions.EqProperty(Projections.Constant(1), Projections.Constant(1));
            }
            if (viewAll)
            {
                return Session
                .QueryOver<PayItemStructure>()
                .Where(restriction)
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
            return Session
                .QueryOver<PayItemStructure>()
                .Where(restriction)
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