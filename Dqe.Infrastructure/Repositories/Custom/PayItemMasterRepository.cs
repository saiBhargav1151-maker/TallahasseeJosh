using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Glimpse.Core.Extensibility;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PayItemMasterRepository : BaseRepository, IPayItemMasterRepository
    {
        public PayItemMasterRepository() { }

        internal PayItemMasterRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<PayItemMaster> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<PayItemMaster>()
                .List();
        }

        public decimal? GetStatePriceForItem(string item)
        {
            InitializeSession();
            return Session
                .QueryOver<PayItemMaster>()
                .Where(i => i.RefItemName == item)
                .Select(i => i.StateReferencePrice)
                .SingleOrDefault<decimal?>();

        }

        public decimal GetMarketPriceForItem(string item, string countyName)
        {
            MarketAreaAveragePrice marketAreaAveragePrice = null;
            PayItemMaster payItemMaster = null;
            MarketArea marketArea = null;
            County county = null;
            InitializeSession();
            var ap = Session
                .QueryOver(() => marketAreaAveragePrice)
                .Left.JoinQueryOver(() => marketAreaAveragePrice.MyPayItemMaster, () => payItemMaster)
                .Where(() => payItemMaster.RefItemName == item)
                .Left.JoinQueryOver(() => marketAreaAveragePrice.MyMarketArea, () => marketArea)
                .Left.JoinQueryOver(() => marketArea.Counties, () => county)
                .Where(() => county.Name == countyName)
                .SingleOrDefault();
            return ap == null ? 0 : ap.Price;
        }

        public decimal GetCountyPriceForItem(string item, string countyName)
        {
            CountyAveragePrice countyAveragePrice = null;
            PayItemMaster payItemMaster = null;
            County county = null;
            InitializeSession();
            var ap = Session
                .QueryOver(() => countyAveragePrice)
                .Left.JoinQueryOver(() => countyAveragePrice.MyPayItemMaster, () => payItemMaster)
                .Where(() => payItemMaster.RefItemName == item)
                .Left.JoinQueryOver(() => countyAveragePrice.MyCounty, () => county)
                .Where(() => county.Name == countyName)
                .SingleOrDefault();
            return ap == null ? 0 : ap.Price;
        }
    }
}