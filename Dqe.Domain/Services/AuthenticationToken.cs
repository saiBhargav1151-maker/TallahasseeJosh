namespace Dqe.Domain.Services
{
    public class AuthenticationToken
    {
        public bool IsAuthenticated { get; set; }
        public bool IsPasswordExpired { get; set; }
        public string Message { get; set; }
    }
}