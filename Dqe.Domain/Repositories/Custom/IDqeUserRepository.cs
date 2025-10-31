using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IDqeUserRepository
    {
        IEnumerable<DqeUser> GetAll(long currentUserId, string district, bool includeCurrentUser = false);
        IEnumerable<DqeUser> GetAll();
        DqeUser Get(long id);
        DqeUser GetBySrsId(int id);
        DqeUser GetBySrsId(int id, bool useActiveCriteria);
        DqeUser GetSystemAccount();
        IEnumerable<DqeUser> GetAllSystemAdministrators();
    }
}