using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class DqeUser : Transformer
    {
        public int SrsId { get; set; }
        public DqeRole Role { get; set; }
        public string RoleAsString { get; set; }
        public string District { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public string CostGroupAuthorization { get; set; }
    }
}