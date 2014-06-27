using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Dqe.Web.Helpers
{
    public static class Utilities
    {
        public static string PackGuid(Guid guid)
        {
            // This produces a 22-character string representation of a GUID. It contains
            // only alphanumerics, - and _. These characters aren't affected by URL encoding,
            // which makes them a tad easier to pass around in query strings and suchlike.
            // The Base64 encoded version of the GUID always ends in two equals signs,
            // so removing them loses no uniqueness.
            return Convert.ToBase64String(guid.ToByteArray())
                .Replace('+', '_')
                .Replace('/', '-')
                .Replace("=", "");
        }

        public static void AddModelError<TModel>(this ModelStateDictionary modelState, 
            Expression<Func<TModel, object>> expression, 
            string errorMessage)
        {
            var propName = ExpressionHelper.GetExpressionText(expression);
            modelState.AddModelError(propName, errorMessage);
        }
    }
}