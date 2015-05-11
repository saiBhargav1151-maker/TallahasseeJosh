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

        public PayItemStructure Get(long id)
        {
            InitializeSession();
            return Session.Get<PayItemStructure>(id);
        }

        public Domain.Transformers.PayItemStructure Get(int specYear, string itemName)
        {
            InitializeSession();
            var pis = Session
                .QueryOver<PayItemStructure>()
                .JoinQueryOver(i => i.PayItemMasters)
                .Where(i => i.RefItemName == itemName.ToUpper().Trim())
                .JoinQueryOver(i => i.MyMasterFile)
                .Where(i => i.FileNumber == specYear)
                .SingleOrDefault();
            if (pis == null) return null;
            return new Domain.Transformers.PayItemStructure
            {
                Id = pis.Id,
                StructureId = pis.StructureId,
                Title = pis.Title,
                Units = pis.PayItemMasters
                    .Where(ii => !ii.ObsoleteDate.HasValue || ii.ObsoleteDate.Value.Date > DateTime.Now.Date)
                    .Select(ii => ii.BidAsLumpSum ? string.Format("LS/{0}", ii.Unit) : ii.Unit)
                    .Distinct()
                    .ToList()
            };
        }

        public PayItemStructure GetByStructureId(string structureId, long? thisPayItemStructureId)
        {
            InitializeSession();
            var q = Session
                .QueryOver<PayItemStructure>()
                .Where(i => i.StructureId == structureId);
            return thisPayItemStructureId.HasValue 
                ? q.Where(i => i.Id != thisPayItemStructureId).SingleOrDefault() 
                : q.SingleOrDefault();
        }

        public IEnumerable<Domain.Transformers.PayItemStructure> GetAllHeaders(string range)
        {
            InitializeSession();
            if (string.IsNullOrWhiteSpace(range))
            {
                return Session
                    .QueryOver<PayItemStructure>()
                    .Left.JoinQueryOver(i => i.PayItemMasters)
                    .List()
                    .Distinct()
                    .Select(i => new Domain.Transformers.PayItemStructure
                    {
                        Id = i.Id,
                        StructureId = i.StructureId,
                        Title = i.Title,
                        Units = i.PayItemMasters
                            .Where(ii => !ii.ObsoleteDate.HasValue || ii.ObsoleteDate.Value.Date > DateTime.Now.Date)
                            .Select(ii => ii.BidAsLumpSum ? string.Format("LS/{0}", ii.Unit) : ii.Unit)
                            .Distinct()
                            .ToList()
                        //PrimaryUnit = i.PrimaryUnit,
                        //SecondaryUnit = i.SecondaryUnit
                    });
            }
            var range1 = range;
            var range2 = range1 == " 0" ? " 1" : range1;
            var disjunction = new Disjunction();
            if (range1 == " 0")
            {
                disjunction.Add(Restrictions.On<PayItemStructure>(i => i.StructureId).IsLike("  ", MatchMode.Start));
            }
            disjunction.Add(Restrictions.On<PayItemStructure>(i => i.StructureId).IsLike(range1, MatchMode.Start));
            disjunction.Add(Restrictions.On<PayItemStructure>(i => i.StructureId).IsLike(range2, MatchMode.Start));
            return Session
                .QueryOver<PayItemStructure>()
                .Where(disjunction)
                .Left.JoinQueryOver(i => i.PayItemMasters)
                .List()
                .Distinct()
                .Select(i => new Domain.Transformers.PayItemStructure
                {
                    Id = i.Id,
                    StructureId = i.StructureId,
                    Title = i.Title,
                    Units = i.PayItemMasters
                        .Where(ii => !ii.ObsoleteDate.HasValue || ii.ObsoleteDate.Value.Date > DateTime.Now.Date)
                        .Select(ii => ii.BidAsLumpSum ? string.Format("LS/{0}", ii.Unit) : ii.Unit)
                        .Distinct()
                        .ToList()
                    //PrimaryUnit = i.PrimaryUnit,
                    //SecondaryUnit = i.SecondaryUnit
                });
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
                .Left.JoinQueryOver(i => i.PayItemMasters)
                .OrderBy(i => i.RefItemName).Asc
                .Left.JoinQueryOver(i => i.MyMasterFile)
                .OrderBy(i => i.FileNumber).Asc
                .List()
                .Distinct();
            }
            //var structureObsoleteDateDisjunction = new Disjunction();
            //structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate == null));
            //structureObsoleteDateDisjunction.Add(Restrictions.Where<PayItemStructure>(i => i.ObsoleteDate >= DateTime.Now));
            var itemObsoleteDateDisjunction = new Disjunction();
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItemMaster>(i => i.ObsoleteDate == null));
            itemObsoleteDateDisjunction.Add(Restrictions.Where<PayItemMaster>(i => i.ObsoleteDate.Value >= DateTime.Now));
            return Session
                .QueryOver<PayItemStructure>()
                .Where(restriction)
                //.Where(structureObsoleteDateDisjunction)
                .Left.JoinAlias(i => i.MyCostBasedTemplate, () => costBasedTemplate)
                .OrderBy(i => i.StructureId).Asc
                .Left.JoinQueryOver(i => i.PayItemMasters)
                .Where(itemObsoleteDateDisjunction)
                .OrderBy(i => i.RefItemName).Asc
                .Left.JoinQueryOver(i => i.MyMasterFile)
                .OrderBy(i => i.FileNumber).Asc
                .List()
                .Distinct();
        }
    }
}