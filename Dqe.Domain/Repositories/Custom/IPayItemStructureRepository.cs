using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemStructureRepository
    {
        PayItemStructure Get(int id);
        PayItemStructure GetByStructureId(string structureId, int thisPayItemStructureId);
        IEnumerable<PayItemStructure> GetAll();
        IEnumerable<PayItemStructure> GetGroup(int panel);
    }
}