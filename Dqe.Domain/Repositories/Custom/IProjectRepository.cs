using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IProjectRepository
    {
        Project GetByNumber(string number);
    }
}