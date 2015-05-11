using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using Alltech.Logging;

[assembly: WebActivatorEx.PreApplicationStartMethod(
      typeof($rootnamespace$.LoggingBootstrap), "Start")]
namespace $rootnamespace$
{
    public static class LoggingBootstrap
    {

		//TODO: Initialize this list with any values to exclude from usage and post logging. This will match the url on contains
		private static readonly List<string> Excludes = new List<string>();
		
        public static void Start()
        {
            GlobalFilters.Filters.Add(new LoggingFilter());
			PostAndUsageExcludes.Excludes = LoggingBootstrap.Excludes;
            ExceptionHandlerModule.GetAdditionalInformation += GetAdditionalInformation;
        }

        public static Dictionary<string, string> GetAdditionalInformation(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
