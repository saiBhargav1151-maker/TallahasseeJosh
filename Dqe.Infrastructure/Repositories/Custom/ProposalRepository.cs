using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class ProposalRepository : BaseRepository, IProposalRepository
    {
        public ProposalRepository() { }

        internal ProposalRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<Proposal> GetByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Proposal>()
                .Where(i => i.ProposalNumber == number)
                .List();
        }
    }
}