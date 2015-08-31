using Alltech.Logging;
using System.Web.Mvc;

namespace Dqe.Web.Controllers
{
    [RoutePrefix("AlltechLogging")]
    public class AlltechLoggingController : Controller
    {
        [HttpPost]
        [Route("Log")]
        public ActionResult Log(ClientSideLoggingInfo info)
        {
            try
            {
                if (!ClientSideExceptionNotificationService.IsLoggingEnabled)
                {
                    // Allow web.config setting to turn off logging on clients since they use the value returned in loggingDisabled
                    return Json(new { loggingDisabled = true }, "application/json");
                }

                ClientSideExceptionNotificationService.SendNotificiation(info);
            }
            catch
            {
                // Purposefully doing nothing here.  This method if for logging client side errors - if they can't be logged something 
                // might be seriously wrong and we do not want to flood our logging mechanism.
            }


            return Json(new { loggingDisabled = false }, "application/json");
        }
    }
}