using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Hql.Ast.ANTLR;

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

        public IEnumerable<PayItemMaster> GetAllWithPrices(string specYear)
        {
            PayItemMaster payItemMaster = null;
            CountyAveragePrice countyAveragePrice = null;
            MarketAreaAveragePrice marketAreaAveragePrice = null;
            InitializeSession();
            return Session
                    .QueryOver(() => payItemMaster)
                    .Left.JoinQueryOver(() => payItemMaster.CountyAveragePrices, () => countyAveragePrice)
                    .Left.JoinQueryOver(() => countyAveragePrice.MyCounty)
                    .Left.JoinQueryOver(() => payItemMaster.MarketAreaAveragePrices, () => marketAreaAveragePrice)
                    .Left.JoinQueryOver(() => marketAreaAveragePrice.MyMarketArea)
                    .JoinQueryOver(() => payItemMaster.MyMasterFile)
                    .Where(i => i.FileNumber == int.Parse(specYear))
                    .List()
                    .Distinct();
        }

        public int GetAllCount(string specYear)
        {
            InitializeSession();
            return Session
                .QueryOver<PayItemMaster>()
                .JoinQueryOver(i => i.MyMasterFile)
                .Where(i => i.FileNumber == int.Parse(specYear))
                .RowCount();
        }

        public IEnumerable<PayItemMaster> GetAllRanged(string specYear, int skip, int take)
        {
            InitializeSession();
            return Session
                    .QueryOver<PayItemMaster>()
                    .OrderBy(i => i.RefItemName).Asc
                    .JoinQueryOver(i => i.MyMasterFile)
                    .Where(i => i.FileNumber == int.Parse(specYear))
                    .Skip(skip)
                    .Take(take)
                    .List();
        }

        public IEnumerable<PayItemMaster> GetByName(string name)
        {
            InitializeSession();
            return Session
                    .QueryOver<PayItemMaster>()
                    .Where(i => i.RefItemName == name)
                    .List();
        }

        public IEnumerable<Domain.Transformers.PayItemMaster> GetHeaders(string val)
        {
            InitializeSession();
            //var specYears = Session
            //    .QueryOver<MasterFile>()
            //    .Select(i => i.FileNumber)
            //    .List<int>();
            //var years = specYears.Select(i => i.ToString());
            //var d = years.ToDictionary(y => y.StartsWith("9")
            //    ? string.Format("0{0}",y.PadLeft(2, '0'))
            //    : string.Format("1{0}", y.PadLeft(2, '0')));
            //var key = d.Keys.OrderByDescending(i => i).First().Substring(1);
            PayItemMaster payItemMaster = null;
            PayItemStructure payItemStructure = null;
            MasterFile masterFile = null;
            return Session
                .QueryOver(() => payItemMaster)
                .WhereRestrictionOn(() => payItemMaster.RefItemName).IsLike(val.ToUpper(), MatchMode.Start)
                .JoinQueryOver(() => payItemMaster.MyMasterFile, () => masterFile)
                .Left.JoinQueryOver(() => payItemMaster.MyPayItemStructure, () => payItemStructure)
                .Where(() => payItemStructure.Id == null)
                .List()
                .Select(i => new Domain.Transformers.PayItemMaster
                {
                    Id = i.Id,
                    RefItemName = i.RefItemName,
                    SpecBook = i.MyMasterFile.FileNumber.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                });
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

        public PayItemMaster Get(long id)
        {
            InitializeSession();
            return Session
                .QueryOver<PayItemMaster>()
                .Where(i => i.Id == id)
                .SingleOrDefault();
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
                //.Left.JoinQueryOver(() => payItemMaster.CountyAveragePrices, () => countyAveragePrice)
                //.Left.JoinQueryOver(() => payItemMaster.MarketAreaAveragePrices, () => marketAreaAveragePrice)
                //.Left.JoinQueryOver(() => countyAveragePrice.MyCounty, () => county)
                //.Left.JoinQueryOver(() => marketAreaAveragePrice.MyMarketArea, () => marketArea)
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

        public IEnumerable<PayItemMaster> PayItemSearchByName(string name)
        {
            InitializeSession();

            PayItemMaster payItemMaster = null;
            CostGroupPayItem costGroupPayItem = null;

            return Session
                .QueryOver(() => payItemMaster)
                .WhereRestrictionOn(i => i.RefItemName).IsInsensitiveLike(name, MatchMode.Start)
                .Left.JoinQueryOver(() => payItemMaster.CostGroups, () => costGroupPayItem)
                .List()
                .OrderBy(i => i.RefItemName)
                .ToList();
        }

        public IEnumerable<PayItemMaster> GetPayItemsWithStructureInfo(string specBook)
        {
            InitializeSession();

            MasterFile masterFile = null;
            PayItemMaster payItemMaster = null;
            PayItemStructure payItemStructure = null;

            var mf = Session
                .QueryOver(() => masterFile)
                .Where(i => i.FileNumber == Convert.ToInt32(specBook))
                .JoinQueryOver(() => masterFile.PayItemMasters, () => payItemMaster)
                .Left.JoinQueryOver(() => payItemMaster.MyPayItemStructure, () => payItemStructure)
                .SingleOrDefault();

            return mf.PayItemMasters.ToList();
        }

        public IEnumerable<PayItemMaster> GetFrontLoadedPayItems(int specBook)
        {
            InitializeSession();

            MasterFile masterFile = null;
            PayItemMaster payItemMaster = null;

            var mf = Session
                .QueryOver(() => masterFile)
                .Where(i => i.FileNumber == specBook)
                .JoinQueryOver(() => masterFile.PayItemMasters, () => payItemMaster)
                .Where(i => i.IsFrontLoadedItem)
                .SingleOrDefault();

            return mf != null ? mf.PayItemMasters.ToList() : new List<PayItemMaster>();
        }
    }
}