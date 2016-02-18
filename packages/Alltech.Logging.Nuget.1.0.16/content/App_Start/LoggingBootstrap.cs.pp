using Alltech.Logging;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof($rootnamespace$.LoggingBootstrap), "Start")]
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

        public static Dictionary<string, string> GetAdditionalInformation()
        {
            throw new NotImplementedException();
        }
    }
}