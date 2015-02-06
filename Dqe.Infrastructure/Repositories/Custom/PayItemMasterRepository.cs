using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
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

        public IEnumerable<PayItemMaster> GetAll(string specYear)
        {
            InitializeSession();
            return specYear == "*"
                ? Session
                    .QueryOver<PayItemMaster>()
                    .List()
                : Session
                    .QueryOver<PayItemMaster>()
                    .JoinQueryOver(i => i.MyMasterFile)
                    .Where(i => i.FileNumber == int.Parse(specYear))
                    .List();
        }

        public PayItemMaster Get(string name)
        {
            InitializeSession();
            var items = Session
                .QueryOver<PayItemMaster>()
                .Where(i => i.RefItemName == name)
                .List();
            if (!items.Any()) return null;
            var d = items.ToDictionary(payItemMaster => payItemMaster.SpecBook.StartsWith("9")
                ? string.Format("0{0}", payItemMaster.SpecBook.PadLeft(2, '0'))
                : string.Format("1{0}", payItemMaster.SpecBook.PadLeft(2, '0')));
            var key = d.Keys.OrderByDescending(i => i).First();
            return d[key];
        }

        public IEnumerable<PayItemMaster> GetAllWithPrices()
        {
            InitializeSession();
            var mfl = Session.QueryOver<MasterFile>().List();
            var syl = mfl.Select(masterFile => masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).StartsWith("9")
                ? string.Format("0{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))
                : string.Format("1{0}", masterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0'))).ToList();
            var sy = syl.OrderByDescending(i => i).First();
            PayItemMaster payItemMaster = null;
            CountyAveragePrice countyAveragePrice = null;
            MarketAreaAveragePrice marketAreaAveragePrice = null;
            County county = null;
            MarketArea marketArea = null;
            MasterFile masterFileAlias = null;
            return Session
                .QueryOver(() => payItemMaster)
                .Left.JoinQueryOver(() => payItemMaster.MyMasterFile, () => masterFileAlias)
                //.Where(() => payItemMaster.SpecBook == sy.Substring(1))
                .Where(() => masterFileAlias.FileNumber == int.Parse(sy.Substring(1)))
                .Left.JoinQueryOver(() => payItemMaster.CountyAveragePrices, () => countyAveragePrice)
                .Left.JoinQueryOver(() => payItemMaster.MarketAreaAveragePrices, () => marketAreaAveragePrice)
                .Left.JoinQueryOver(() => countyAveragePrice.MyCounty, () => county)
                .Left.JoinQueryOver(() => marketAreaAveragePrice.MyMarketArea, () => marketArea)
                .List()
                .Distinct()
                .ToList();
        } 

        public PayItemMaster GetWithHistory(string name)
        {
            InitializeSession();
            var items = Session
                .QueryOver<PayItemMaster>()
                .Where(i => i.RefItemName == name)
                .JoinQueryOver(i => i.ProposalHistories)
                .JoinQueryOver(i => i.BidHistories)
                .List()
                .Distinct()
                .ToList();
            if (!items.Any()) return null;
            var d = items.ToDictionary(payItemMaster => payItemMaster.SpecBook.StartsWith("9")
                ? string.Format("0{0}", payItemMaster.SpecBook.PadLeft(2, '0'))
                : string.Format("1{0}", payItemMaster.SpecBook.PadLeft(2, '0')));
            var key = d.Keys.OrderByDescending(i => i).First();
            return d[key];
        }

        public decimal? GetStatePriceForItem(string item)
        {
            InitializeSession();
            //return Session
            //    .QueryOver<PayItemMaster>()
            //    .Where(i => i.RefItemName == item)
            //    .Select(i => i.StateReferencePrice)
            //    .SingleOrDefault<decimal?>();

            var i = Get(item);
            return i == null ? null : i.StateReferencePrice;
            
        }

        public decimal GetMarketPriceForItem(string item, string countyName)
        {
            var i = Get(item);
            if (i == null) return 0;
            MarketAreaAveragePrice marketAreaAveragePrice = null;
            PayItemMaster payItemMaster = null;
            MarketArea marketArea = null;
            County county = null;
            InitializeSession();
            var ap = Session
                .QueryOver(() => marketAreaAveragePrice)
                .Left.JoinQueryOver(() => marketAreaAveragePrice.MyPayItemMaster, () => payItemMaster)
                .Where(() => payItemMaster.Id == i.Id)
                .Left.JoinQueryOver(() => marketAreaAveragePrice.MyMarketArea, () => marketArea)
                .Left.JoinQueryOver(() => marketArea.Counties, () => county)
                .Where(() => county.Name == countyName)
                .SingleOrDefault();
            return ap == null ? 0 : ap.Price;
        }

        public decimal GetCountyPriceForItem(string item, string countyName)
        {
            var i = Get(item);
            if (i == null) return 0;
            CountyAveragePrice countyAveragePrice = null;
            PayItemMaster payItemMaster = null;
            County county = null;
            InitializeSession();
            var ap = Session
                .QueryOver(() => countyAveragePrice)
                .Left.JoinQueryOver(() => countyAveragePrice.MyPayItemMaster, () => payItemMaster)
                .Where(() => payItemMaster.Id == i.Id)
                .Left.JoinQueryOver(() => countyAveragePrice.MyCounty, () => county)
                .Where(() => county.Name == countyName)
                .SingleOrDefault();
            return ap == null ? 0 : ap.Price;
        }

        public void ResetAveragePricesAndClearBidHistory()
        {
            InitializeSession();
            Session.CreateQuery("delete from AveragePrice").ExecuteUpdate();
            Session.CreateQuery("delete from BidHistory").ExecuteUpdate();
            Session.CreateQuery("delete from ProposalHistory").ExecuteUpdate();
            Session.CreateQuery("update PayItemMaster set StateReferencePrice = 0").ExecuteUpdate();
        }
    }
}