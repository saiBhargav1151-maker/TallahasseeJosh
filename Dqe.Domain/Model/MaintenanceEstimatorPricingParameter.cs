using System;
using System.Security;

namespace Dqe.Domain.Model
{
    public class MaintenanceEstimatorPricingParameter : PricingParameter
    {
        public virtual DqeUser User { get; set; }
        public override Transformers.PricingParameter GetTransformer()
        {
            return new Transformers.EstimatorPricingParameter()
            {
                Months = Months,
                ContractType = ContractType,
                Quantities = Quantities,
                WorkTypes = WorkTypes,
                PricingModel = PricingModel,
                Bidders = Bidders,
                User = User
            };
        }

        public override void Transform(Transformers.PricingParameter transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.MaintenanceEstimator && account.Role != DqeRole.MaintenanceDistrictAdmin)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }

            var maintenanceEstimatorTransformer = (Transformers.MaintenanceEstimatorPricingParameter)transformer;

            Months = maintenanceEstimatorTransformer.Months;
            ContractType = maintenanceEstimatorTransformer.ContractType;
            Quantities = maintenanceEstimatorTransformer.Quantities;
            PricingModel = maintenanceEstimatorTransformer.PricingModel;
            Bidders = maintenanceEstimatorTransformer.Bidders;
            User = maintenanceEstimatorTransformer.User;
        }

        public override void CheckWorkTypeSecurity(DqeUser dqeUser)
        {
            if (dqeUser == null) throw new ArgumentNullException("dqeUser");
            if (dqeUser.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", dqeUser.Role));
            }
        }
    }
}