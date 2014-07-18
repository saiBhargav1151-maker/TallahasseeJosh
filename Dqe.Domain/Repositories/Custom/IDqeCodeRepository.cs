using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IDqeCodeRepository
    {
        DqeCode Get(int id);
        //IEnumerable<PrimaryUnit> GetPrimaryUnits();
        //IEnumerable<SecondaryUnit> GetSecondaryUnits();
    }
}