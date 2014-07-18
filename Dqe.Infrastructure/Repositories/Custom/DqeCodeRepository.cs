using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeCodeRepository : IDqeCodeRepository
    {
        public DqeCode Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<DqeCode>(id);
        }

        //public IEnumerable<PrimaryUnit> GetPrimaryUnits()
        //{
        //    return UnitOfWorkProvider
        //        .Marshaler
        //        .CurrentSession
        //        .QueryOver<PrimaryUnit>()
        //        .OrderBy(i => i.Name).Asc
        //        .List();
        //}

        //public IEnumerable<SecondaryUnit> GetSecondaryUnits()
        //{
        //    return UnitOfWorkProvider
        //        .Marshaler
        //        .CurrentSession
        //        .QueryOver<SecondaryUnit>()
        //        .OrderBy(i => i.Name).Asc
        //        .List();
        //}
    }
}