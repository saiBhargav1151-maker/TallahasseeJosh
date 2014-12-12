using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemMasterRepository
    {
        IEnumerable<PayItemMaster> GetAll();
        decimal? GetStatePriceForItem(string item);
        decimal GetMarketPriceForItem(string item, string countyName);
        decimal GetCountyPriceForItem(string item, string countyName);
    }
}