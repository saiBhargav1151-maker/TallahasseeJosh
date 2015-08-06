using System;
using System.Security;

namespace Dqe.Domain.Model
{
    public class StatewidePricingParameter : PricingParameter
    {
        public override Transformers.PricingParameter GetTransformer()
        {
            return new Transformers.PricingParameter
            {
                Months = Months,
                ContractType = ContractType,
                Quantities = Quantities,
                WorkTypes = WorkTypes,
                PricingModel = PricingModel,
                Bidders = Bidders
            };
        }

        public override void Transform(Transformers.PricingParameter transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }

            Months = transformer.Months;
            ContractType = transformer.ContractType;
            Quantities = transformer.Quantities;
            PricingModel = transformer.PricingModel;
            Bidders = transformer.Bidders;
        }

        public override void CheckWorkTypeSecurity(DqeUser dqeUser)
        {
            if (dqeUser == null) throw new ArgumentNullException("dqeUser");
            if (dqeUser.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", dqeUser.Role));
            }
        }
    }
}