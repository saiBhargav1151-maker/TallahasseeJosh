// ReSharper disable InconsistentNaming
namespace Dqe.Web.ActionResults
{
    public class ClientMessage
    {
        private int _ttl;

        public ClientMessage()
        {
            Severity = ClientMessageSeverity.Success;
            ttl = 10000;  //miliseconds, set to 10 seconds
            text = "Operation completed successfully";
        }

        public ClientMessageSeverity Severity { get; set; }
        public string text { get; set; }


        public int ttl
        {
            get { return Severity == ClientMessageSeverity.Error ? 0 : _ttl; }
            set { _ttl = value; }
        }

        public string severity
        {
            get
            {
                return Severity == ClientMessageSeverity.Error
                    ? "error"
                    : Severity == ClientMessageSeverity.Information
                        ? "info"
                        : Severity == ClientMessageSeverity.Success
                            ? "success"
                            : "warn";
            }
        }
    }
}
// ReSharper restore InconsistentNaming