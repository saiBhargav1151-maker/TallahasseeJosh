using System.Text;
using Dqe.ApplicationServices;

namespace Dqe.Web.Messaging
{
    public class AccountVerificationEmail : EmailMessage
    {
        private readonly string _to;
        private readonly string _token;
        private readonly IContextService _contextService;

        public AccountVerificationEmail(string to, string token, IContextService contextService)
        {
            _to = to;
            _token = token;
            _contextService = contextService;
        }

        public override string To { get { return _to; } }

        public override string Subject { get { return "Account Verification"; } }

        public override string Body
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Please click on the following link to activate your account.\n\r");
                sb.AppendFormat("{0}/Account/User/ValidateAccount/{1}\n\r", _contextService.SiteRootAddress , _token);
                return sb.ToString();
            }
        }
    }
}