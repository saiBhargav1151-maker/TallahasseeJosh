using System.Security.Principal;

namespace Dqe.ApplicationServices
{
    public class DqeIdentity : IIdentity
    {
        public int Id { get; private set; }
        public int SrsId { get; private set; }
        public string Name { get; private set; }
        public string District { get; private set; }
        public string AuthenticationType { get { return "RACF"; } }
        public bool IsAuthenticated { get { return true; } }

        public DqeIdentity(int id, int srsId, string name, string district)
        {
            Id = id;
            SrsId = srsId;
            Name = name;
            District = district;
        }
    }
}