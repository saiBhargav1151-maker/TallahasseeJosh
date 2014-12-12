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

        public Proposal GetWtByNumber(string number)
        {
            InitializeSession();
            return Session
                .QueryOver<Proposal>()
                .Where(i => i.ProposalNumber == number)
                .Where(i => i.ProposalSource == ProposalSourceType.Wt)
                .Left.JoinQueryOver(i => i.Projects)
                .Left.JoinQueryOver(i => i.CustodyOwner)
                .SingleOrDefault();
        }

        public ProjectItem GetProjectItemByWtId(long id, DqeUser custodyUser)
        {
            InitializeSession();
            var userId = custodyUser.Id;
            ProjectItem projectItem = null;
            EstimateGroup estimateGroup = null;
            ProjectEstimate projectEstimate = null;
            ProjectVersion projectVersion = null;
            Project project = null;
            DqeUser versionOwner = null;
            DqeUser custodyOwner = null;
            return Session
                .QueryOver(() => projectItem)
                .Where(() => projectItem.WtId == id)
                .JoinQueryOver(() => projectItem.MyEstimateGroup, () => estimateGroup)
                .JoinQueryOver(() => estimateGroup.MyProjectEstimate, () => projectEstimate)
                .Where(() => projectEstimate.IsWorkingEstimate)
                .JoinQueryOver(() => projectEstimate.MyProjectVersion, () => projectVersion)
                .JoinQueryOver(() => projectVersion.VersionOwner, () => versionOwner)
                .Where(() => versionOwner.Id == userId)
                .JoinQueryOver(() => projectVersion.MyProject, () => project)
                .JoinQueryOver(() => project.CustodyOwner, () => custodyOwner)
                .Where(() => custodyOwner.Id == userId)
                .SingleOrDefault();
        }
    }
}