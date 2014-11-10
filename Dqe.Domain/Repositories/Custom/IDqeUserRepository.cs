using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IDqeUserRepository
    {
        IEnumerable<DqeUser> GetAll(int currentUserId, string district);
        IEnumerable<DqeUser> GetAll();
        DqeUser Get(int id);
        DqeUser GetBySrsId(int id);
        DqeUser GetSystemAccount();
    }
}