using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PayItemRepository : BaseRepository, IPayItemRepository
    {
        public PayItemRepository() { }

        internal PayItemRepository(ISession session)
        {
            Session = session;
        }

        public PayItem Get(int id)
        {
            InitializeSession();
            return Session.Get<PayItem>(id);
        }

        public PayItem GetByNumberAndMasterFile(string number, int masterFile)
        {
            InitializeSession();
            return Session
                .QueryOver<PayItem>()
                .Where(i => i.PayItemId == number)
                .JoinQueryOver(i => i.MyMasterFile)
                .Where(i => i.FileNumber == masterFile)
                .SingleOrDefault();
        }

        public IEnumerable<PayItem> GetByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<PayItem>()
                .Where(i => i.PayItemId == number)
                .List();
        }

        public IEnumerable<PayItem> GetAll(int range)
        {
            InitializeSession();
            PayItemStructure payItemStructure = null;
            ICriterion restriction;
            if (range < 10)
            {
                restriction = Restrictions.On(() => payItemStructure.StructureId)
                        .IsLike(range.ToString(CultureInfo.InvariantCulture)
                        .PadLeft(2, '0'), MatchMode.Start);
            }
            else if (range == 10)
            {
                restriction = Restrictions.Not(Restrictions.On(() => payItemStructure.StructureId).IsLike("0", MatchMode.Start));
            }
            else
            {
                restriction = Restrictions.EqProperty(Projections.Constant(1), Projections.Constant(1));
            }
            var structureObsoleteDateDisjunction = new Disjunction();
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate == null));
            structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate >= DateTime.Now));
            var itemObsoleteDateDisjunction = new Disjunction();
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate == null));
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItem>(i => i.ObsoleteDate.Value >= DateTime.Now));
            return Session
                .QueryOver<PayItem>()
                .JoinAlias(p => p.MyPayItemStructure, () => payItemStructure)
                .Where(restriction)
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