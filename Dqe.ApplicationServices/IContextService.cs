namespace Dqe.ApplicationServices
{
    public interface IContextService
    {
        int UserId { get; set; }
        string ClientId { get; }
        string SiteRootAddress { get; }
    }
}