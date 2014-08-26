using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PricingParameterRepository : BaseRepository, IPricingParameterRepository
    {
        public PricingParameterRepository() { }

        internal PricingParameterRepository(ISession session)
        {
            Session = session;
        }

        public PricingParameter GetEstimatorDefaultByUserId(DqeUser user)
        {
            InitializeSession();
            return Session
                .QueryOver<EstimatorPricingParameter>()
                .Where(e=>e.User == user)
                .List().SingleOrDefault();
        }

        public PricingParameter GetStateWideDefault()
        {
            InitializeSession();
            return Session
                .QueryOver<StatewidePricingParameter>()
                .List().SingleOrDefault();
        }

        public PricingParameter GetDistrictDefault(string district)
        {
            InitializeSession();
            return Session
                .QueryOver<DistrictPricingParameter>()
                .Where(d=>d.District == district)
                .List().SingleOrDefault();
        }
    }
}
