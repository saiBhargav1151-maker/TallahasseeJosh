using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class PricingParameterRepository:IPricingParameterRepository
    {
        public PricingParameter GetEstimatorDefaultByUserId(DqeUser user)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<EstimatorPricingParameter>()
                .Where(e=>e.User == user)
                .List().SingleOrDefault();
        }

        public PricingParameter GetStateWideDefault()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<StatewidePricingParameter>()
                .List().SingleOrDefault();
        }

        public PricingParameter GetDistrictDefault(string district)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DistrictPricingParameter>()
                .Where(d=>d.District == district)
                .List().SingleOrDefault();
        }
    }
}
