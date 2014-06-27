using System.Collections.Generic;
using System.Dynamic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IUserAccountRepository
    {
        int Count();
        UserAccount Get(int id);
        UserAccount Get(string email);
        IEnumerable<dynamic> GetUserEmailsProjection(int id);
        bool Authenticate(string email, string password);
        bool ValidateAccount(string token);
    }
}