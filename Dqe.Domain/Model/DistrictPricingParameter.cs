using System;
using System.Security;

namespace Dqe.Domain.Model
{
    public class DistrictPricingParameter : PricingParameter
    {
        public virtual string District { get; set; }
        public override Transformers.PricingParameter GetTransformer()
        {
            return new Transformers.DistrictPricingParameter
            {
                Months = Months,
                ContractType = ContractType,
                Quantities = Quantities,
                WorkTypes = WorkTypes,
                PricingModel = PricingModel,
                Bidders = Bidders,
                District = District
            };
        }

        public override void Transform(Transformers.PricingParameter transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.DistrictAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }

            var districtTransformer = (Transformers.DistrictPricingParameter)transformer;

            Months = districtTransformer.Months;
            ContractType = districtTransformer.ContractType;
            Quantities = districtTransformer.Quantities;
            PricingModel = districtTransformer.PricingModel;
            Bidders = districtTransformer.Bidders;
            District = districtTransformer.District;
        }

        public override void CheckWorkTypeSecurity(DqeUser dqeUser)
        {
            if (dqeUser == null) throw new ArgumentNullException("dqeUser");
            if (dqeUser.Role != DqeRole.DistrictAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", dqeUser.Role));
            }
        }
    }
}