using Dqe.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.Domain.Services
{
    public static class Helper
    {

        public static string GetRoleDisplayLabel(DqeRole role)
        {
            string displayLabel = "";
            switch (role)
            {
                case DqeRole.System:
                    displayLabel = "System";
                    break;
                case DqeRole.Administrator:
                    displayLabel = "CO Administrator";
                    break;
                case DqeRole.DistrictAdministrator:
                    displayLabel = "District Administrator";
                    break;
                case DqeRole.PayItemAdministrator:
                    displayLabel = "Pay Item Administrator";
                    break;
                case DqeRole.CostBasedTemplateAdministrator:
                    displayLabel = "Cost-Based Template Administrator";
                    break;
                case DqeRole.Estimator:
                    displayLabel = "Estimator";
                    break;
                case DqeRole.DistrictReviewer:
                    displayLabel = "District Reviewer";
                    break;
                case DqeRole.StateReviewer:
                    displayLabel = "State Reviewer";
                    break;
                case DqeRole.Coder:
                    displayLabel = "Coder";
                    break;
                case DqeRole.MaintenanceDistrictAdmin:
                    displayLabel = "Maintenance District Admin";
                    break;
                case DqeRole.MaintenanceEstimator:
                    displayLabel = "Maintenance Estimator";
                    break;
                case DqeRole.AdminReadOnly:
                    displayLabel = "Admin Read-Only";
                    break;
                default:
                    displayLabel = "No role";
                    break;
            }
            return displayLabel;
        }
    }
}
