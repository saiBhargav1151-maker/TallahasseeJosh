using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemMasterRepository
    {
        IEnumerable<PayItemMaster> GetAll(string specYear);
        IEnumerable<PayItemMaster> GetAllWithPrices(string specYear);
        IEnumerable<PayItemMaster> GetAllRanged(string specYear, int skip, int take);
        int GetAllCount(string specYear);
        PayItemMaster Get(string name);
        PayItemMaster Get(long id);
        PayItemMaster GetWithHistory(string name);
        IEnumerable<PayItemMaster> GetAllWithPrices();
        decimal? GetStatePriceForItem(string item);
        decimal GetMarketPriceForItem(string item, string countyName);
        decimal GetCountyPriceForItem(string item, string countyName);
        void ResetAveragePricesAndClearBidHistory();
        IEnumerable<Transformers.PayItemMaster> GetHeaders(string val);
        IEnumerable<PayItemMaster> GetByName(string name);
        IEnumerable<PayItemMaster> PayItemSearchByName(string name);
        IEnumerable<PayItemMaster> GetPayItemsWithStructureInfo(string specBook);
        IEnumerable<PayItemMaster> GetFrontLoadedPayItems(int specBook);
    }
}