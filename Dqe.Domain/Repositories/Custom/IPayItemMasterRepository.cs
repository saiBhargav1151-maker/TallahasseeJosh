using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemMasterRepository
    {
        IEnumerable<PayItemMaster> GetAll(string specYear);
        PayItemMaster Get(string name);
        PayItemMaster GetWithHistory(string name);
        IEnumerable<PayItemMaster> GetAllWithPrices();
        decimal? GetStatePriceForItem(string item);
        decimal GetMarketPriceForItem(string item, string countyName);
        decimal GetCountyPriceForItem(string item, string countyName);
        void ResetAveragePricesAndClearBidHistory();
    }
}