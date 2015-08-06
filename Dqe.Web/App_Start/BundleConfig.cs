using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Dqe.Web
{
    public class BundleConfig
    {

        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/controllers").IncludeDirectory("~/Scripts/app/controllers", "*.js"));
            bundles.Add(new ScriptBundle("~/bundles/directives").IncludeDirectory("~/Scripts/app/directives", "*.js"));
            bundles.Add(new ScriptBundle("~/bundles/services").IncludeDirectory("~/Scripts/app/services", "*.js"));
            //bundles.Add(new ScriptBundle("~/bundles/utilities").IncludeDirectory("~/Scripts/app/utilities", "*.js"));
            //bundles.Add(new ScriptBundle("~/bundles/app").IncludeDirectory("~/Scripts/app", "*.js"));

            
            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}