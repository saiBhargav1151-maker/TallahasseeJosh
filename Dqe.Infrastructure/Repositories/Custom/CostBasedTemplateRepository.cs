using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class CostBasedTemplateRepository : BaseRepository, ICostBasedTemplateRepository
    {
        public CostBasedTemplateRepository() { }

        internal CostBasedTemplateRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<CostBasedTemplate> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<CostBasedTemplate>()
                .Fetch(c=>c.DocumentVersions).Eager
                .OrderBy(i => i.Name).Asc
                .List().Distinct();
        }

        public CostBasedTemplate Get(long id)
        {
            InitializeSession();
            return Session.Get<CostBasedTemplate>(id);
        }
    }
}
