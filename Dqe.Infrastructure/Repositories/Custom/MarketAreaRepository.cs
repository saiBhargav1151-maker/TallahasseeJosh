using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class MarketAreaRepository : BaseRepository, IMarketAreaRepository
    {

        public MarketAreaRepository() { }

        internal MarketAreaRepository(ISession session)
        {
            Session = session;
        }

        public County GetCountyByCode(string code)
        {
            InitializeSession();
            return Session
                .QueryOver<County>()
                .Where(i => i.Code == code)
                .SingleOrDefault();
        }

        public IEnumerable<County> GetAllCounties()
        {
            InitializeSession();
            return Session
                .QueryOver<County>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<MarketArea> GetAllMarketAreas()
        {
            InitializeSession();
            return Session
                .QueryOver<MarketArea>()
                .Left.JoinQueryOver(i => i.Counties)
                .List()
                .Distinct();
        }

        public MarketArea GetByName(string name)
        {
            InitializeSession();
            return Session
                .QueryOver<MarketArea>()
                .Where(i => i.Name == name.Trim())
                .SingleOrDefault();
        }

        public IEnumerable<County> GetUnassignedCounties()
        {
            InitializeSession();
            return Session
                .QueryOver<County>()
                .Where(i => i.MyMarketArea == null)
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public MarketArea GetMarketAreaById(long id)
        {
            InitializeSession();
            return Session.Get<MarketArea>(id);
        }

        public County GetCountyById(long id)
        {
            InitializeSession();
            return Session.Get<County>(id);
        }
    }
}