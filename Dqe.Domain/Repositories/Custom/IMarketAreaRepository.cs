using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IMarketAreaRepository
    {
        County GetCountyByCode(string code);
        IEnumerable<County> GetAllCounties();
        IEnumerable<MarketArea> GetAllMarketAreas();
        MarketArea GetByName(string name);
        IEnumerable<County> GetUnassignedCounties();
        MarketArea GetMarketAreaById(long id);
        County GetCountyById(long id);
    }
}