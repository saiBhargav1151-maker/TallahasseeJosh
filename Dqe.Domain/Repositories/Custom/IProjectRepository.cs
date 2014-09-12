using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProjectRepository
    {
        Project Get(int id);
        IEnumerable<Project> GetAllByNumber(string number);
        int GetMaxVersionForOwner(string number, DqeUser owner);
    }
}