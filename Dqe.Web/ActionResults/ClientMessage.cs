// ReSharper disable InconsistentNaming
namespace Dqe.Web.ActionResults
{
    public class ClientMessage
    {
        public ClientMessage()
        {
            Severity = ClientMessageSeverity.Success;
            ttl = 1500;
            text = "Operation completed successfully";
        }

        public ClientMessageSeverity Severity { get; set; }
        public string text { get; set; }
        public int ttl { get; set; }
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