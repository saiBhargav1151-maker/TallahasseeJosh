using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IPayItemStructureRepository
    {
        PayItemStructure Get(long id);
        PayItemStructure GetByStructureId(string structureId, long? thisPayItemStructureId);
        IEnumerable<PayItemStructure> GetAll(bool viewAll, int range);
        IEnumerable<Transformers.PayItemStructure> GetAllHeaders(string range);
        Transformers.PayItemStructure Get(int specYear, string itemName);
    }
}