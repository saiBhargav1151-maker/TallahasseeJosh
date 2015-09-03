using System.Collections.Generic;
using Dqe.Domain.Transformers;

namespace Dqe.Domain.Services
{
    public interface IStaffService
    {
        IEnumerable<Staff> GetStaffByName(string name);
        Staff GetStaffById(int id);
        Staff GetStaffByRacf(string id);
        AuthenticationToken AuthenticateUser(string user, string password);
        AuthenticationToken ChangePassword(string user, string password, string newPassword);
    }
}