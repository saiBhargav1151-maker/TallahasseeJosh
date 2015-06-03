using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface ISystemTaskRepository
    {
        SystemTask GetByTaskId(string taskId);
        IEnumerable<SystemTask> GetAll();
    }
}