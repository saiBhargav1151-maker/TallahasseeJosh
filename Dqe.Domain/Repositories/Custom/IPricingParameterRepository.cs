using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPricingParameterRepository
    {
        PricingParameter GetEstimatorDefaultByUserId(DqeUser user);
        PricingParameter GetStateWideDefault();
        PricingParameter GetDistrictDefault(string district);
    }
}
