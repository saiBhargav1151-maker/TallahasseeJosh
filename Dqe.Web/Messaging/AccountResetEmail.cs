using System.Text;
using Dqe.ApplicationServices;

namespace Dqe.Web.Messaging
{
    public class AccountResetEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _password;
        private readonly IContextService _contextService;

        public AccountResetEmail(string to, string password, IContextService contextService)
        {
            _to = to;
            _password = password;
            _contextService = contextService;
        }

        public override string To { get { return _to; } }

        public override string Subject { get { return "Account Reset"; } }

        public override string Body
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Your password is {0}.\n\r", _password);
                sb.Append("Please click on the following link to sign in to your account.\n\r");
                sb.AppendFormat("{0}/Account/User/Authenticate/\n\r", _contextService.SiteRootAddress);
                return sb.ToString();
            }
        }
    }
}