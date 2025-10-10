using System.Collections.Generic;

namespace Dqe.Domain.Model
{
    /// <summary>
    /// Roles with more properties.MB
    /// </summary>
    public class DqeRoleModelItem
    {
        public char Id { get; set; }
        public DqeRole Role { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// This refers to 01-07 + Turnpike
        /// </summary>
        public bool DistrictRole { get; set; }

        /// <summary>
        /// This refers to the CO (district
        /// </summary>
        public bool CoRole { get; set; }

        public bool? Viewable { get; set; } = null;
    }

    /// <summary>
    /// Roles with more properties.MB
    /// </summary>
    public class DqeRoleModelList{
        public DqeRoleModelList()
        {
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "System",
                Role = DqeRole.System,
                Id = 'S',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "System (CO) Administrator",
                Role = DqeRole.Administrator,
                Id = 'A',
                DistrictRole = false,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "District Administrator",
                Role = DqeRole.DistrictAdministrator,
                Id = 'D',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "Pay Item Administrator",
                Role = DqeRole.PayItemAdministrator,
                Id = 'P',
                DistrictRole = false,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "Cost-Based Template Administrator",
                Role = DqeRole.CostBasedTemplateAdministrator,
                Id = 'T',
                DistrictRole = false,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "Estimator",
                Role = DqeRole.Estimator,
                Id = 'E',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "District Reviewer",
                Role = DqeRole.DistrictReviewer,
                Id = 'R',
                DistrictRole = true,
                CoRole = false,
            });

            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "State Reviewer",
                Role = DqeRole.StateReviewer,
                Id = 'B',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "Coder",
                Role = DqeRole.Coder,
                Id = 'C',
                DistrictRole = false,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "MaintenanceDistrictAdmin",
                Role = DqeRole.MaintenanceDistrictAdmin,
                Id = 'F',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "MaintenanceEstimator",
                Role = DqeRole.MaintenanceEstimator,
                Id = 'M',
                DistrictRole = true,
                CoRole = true,
            });
            AllDqeRoles.Add(new DqeRoleModelItem()
            {
                DisplayName = "AdminReadOnly",
                Role = DqeRole.AdminReadOnly,
                Id = 'O',
                DistrictRole = false,
                CoRole = true,
            });
        }
        public List<DqeRoleModelItem> AllDqeRoles { get; set; } = new List<DqeRoleModelItem>();

    }

    //public static class ExtensionMethods
    //{
    //    public static string DisplayValue(this DqeRole e)
    //    {
    //        switch (e)
    //        {
    //            case DqeRole.System:
    //                return "System";
    //            case DqeRole.Administrator:
    //                return "Administrator";
    //            case DqeRole.DistrictAdministrator:
    //                return "District Coordinator";
    //            case DqeRole.PayItemAdministrator:
    //                return "Pay Item Administrator";
    //            case DqeRole.CostBasedTemplateAdministrator:
    //                return "Cost Based Template Administrator";
    //            case DqeRole.Estimator:
    //                return "Estimator";
    //            case DqeRole.Reviewer:
    //                return "Reviewer";
    //            default:
    //                return string.Empty;
    //        }
    //    }
    //}


    /// <summary>
    ///Enum, must have a char value associated with it, it is what is stored in the DB
    /// </summary>
    public enum DqeRole
    {
        /// <summary>
        /// System 
    /// </summary>
    System = 'S',

        /// <summary>
        /// CO Administrator
        /// </summary>
        Administrator = 'A',

        /// <summary>
        /// District Admin
        /// </summary>
        DistrictAdministrator = 'D',

        /// <summary>
        /// CO ONLY
        /// 
        /// </summary>
        PayItemAdministrator = 'P',

        /// <summary>
        /// CO ONLY
        /// </summary>
        CostBasedTemplateAdministrator = 'T',

        /// <summary>
        /// CO or District users
        /// </summary>
        Estimator = 'E',

        /// <summary>
        /// District Only users
        /// </summary>
        DistrictReviewer = 'R',

        /// <summary>
        /// State Reviewer, can be any district user
        /// We need seperate DistrictReviewer Role because we want to be able to assign district users to this role also. MB.
        /// </summary>
        StateReviewer = 'B',

        /// <summary>
        /// CO only,  because they will need to view other districts sometimes and we don't have ability to assign multiple districts
        /// </summary>
        Coder = 'C',

        /// <summary>
        /// CO or District - same as MaintenanceEstimator but has admin security and can assign projects with override checkout
        /// </summary>
        MaintenanceDistrictAdmin = 'F',

        /// <summary>
        /// CO or District
        /// </summary>
        MaintenanceEstimator = 'M',

        /// <summary>
        /// CO Only. Spelled with an "Oh" not a zero
        /// </summary>
        AdminReadOnly = 'O'
    }

}