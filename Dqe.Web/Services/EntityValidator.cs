using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Dqe.ApplicationServices;
using Dqe.Web.ActionResults;

namespace Dqe.Web.Services
{
    public class EntityValidator
    {
        public static DqeResult Validate(ITransactionManager transactionManager, object entity)
        {
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationResults, true))
            {
                transactionManager.Abort();
                var messages = validationResults.Select(validationResult => new ClientMessage { Severity = ClientMessageSeverity.Error, text = validationResult.ErrorMessage, ttl = 0 }).ToList();
                return new DqeResult(new object[] { }, messages, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
    }
}