using System.Web;
using System.Windows.Forms;

namespace Dqe.Web.Services
{
    public static class AppPath
    {
        public static string ApplicationPath
        {
            get
            {
                if (isInWeb)
                    return HttpRuntime.AppDomainAppPath;

                return Application.StartupPath;
            }
        }

        private static bool isInWeb
        {
            get
            {
                return HttpContext.Current != null;
            }
        }
    }
}