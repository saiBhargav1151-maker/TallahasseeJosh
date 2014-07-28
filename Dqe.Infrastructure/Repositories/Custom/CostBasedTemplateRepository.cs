using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class CostBasedTemplateRepository : ICostBasedTemplateRepository
    {
        public IEnumerable<CostBasedTemplate> GetAll()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<CostBasedTemplate>()
                .Fetch(c=>c.DocumentVersions).Eager
                .OrderBy(i => i.Name).Asc
                .List().Distinct();
        }

        public CostBasedTemplate Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<CostBasedTemplate>(id);
        }
    }
}
