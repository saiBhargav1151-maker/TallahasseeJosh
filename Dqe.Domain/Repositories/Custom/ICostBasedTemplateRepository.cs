using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface ICostBasedTemplateRepository
    {
        IEnumerable<CostBasedTemplate> GetAll();
        CostBasedTemplate Get(long id);
    }
}
