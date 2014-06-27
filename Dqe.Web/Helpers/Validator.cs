using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;

namespace Dqe.Web.Helpers
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    public static class Validator
    {
        public static void ValidateEntity
            (
            this Controller controller, 
            IValidatableObject o, 
            ITransactionManager transactionManager, 
            string viewModelPrefix = null, 
            bool clearModelState = false
            )
        {
            if (clearModelState) controller.ModelState.Clear();
            var results = o.Validate(new ValidationContext(o, null, null)).ToList();
            if (results.Count == 0) return;
            foreach (var result in results) ConvertToModelError(controller, result, viewModelPrefix);
            if (transactionManager != null) transactionManager.Abort();
        }

        public static void ValidateEntity
            (
            this Controller controller, 
            ValidationResult result, 
            ITransactionManager transactionManager, 
            string viewModelPrefix = null, 
            bool clearModelState = false
            )
        {
            if (clearModelState) controller.ModelState.Clear();
            if (result == null) return;
            ConvertToModelError(controller, result, viewModelPrefix);
            if (transactionManager != null) transactionManager.Abort();
        }

        private static void ConvertToModelError(Controller controller, ValidationResult result, string viewModelPrefix)
        {
            if (string.IsNullOrWhiteSpace(result.MemberNames.FirstOrDefault())) return;
            foreach (var name in result.MemberNames)
            {
                controller
                    .ModelState
                    .AddModelError
                    (
                        string.Format("{0}{1}",
                                      (viewModelPrefix == null)
                                          ? string.Empty
                                          : string.Format("{0}.", viewModelPrefix.TrimEnd('.')),
                                      name),
                        result.ErrorMessage
                    );
            }
        }
    }
}