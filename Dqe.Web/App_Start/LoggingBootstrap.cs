using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Alltech.Logging;
using Dqe.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(
      typeof(LoggingBootstrap), "Start")]
namespace Dqe.Web
{
    public static class LoggingBootstrap
    {

		//TODO: Initialize this list with any values to exclude from usage and post logging. This will match the url on contains
		private static readonly List<string> Excludes = new List<string>{"Security/GetTimeout", "Security/GetCurrentUser"};
		
        public static void Start()
        {
            GlobalFilters.Filters.Add(new LoggingFilter());
			PostAndUsageExcludes.Excludes = Excludes;
            ExceptionHandlerModule.GetAdditionalInformation += GetAdditionalInformation;
        }

        public static Dictionary<string, string> GetAdditionalInformation()
        {
            return new Dictionary<string, string>();
        }
    }
}
