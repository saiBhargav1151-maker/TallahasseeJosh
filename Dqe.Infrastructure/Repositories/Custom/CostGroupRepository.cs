using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class CostGroupRepository : BaseRepository, ICostGroupRepository
    {
        public CostGroupRepository() { }

        internal CostGroupRepository(ISession session)
        {
            Session = session;
        }

        public CostGroup Get(long id)
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .Where(i => i.Id == id)
                .SingleOrDefault();
        }

        public IEnumerable<CostGroup> GetAll()
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .OrderBy(i => i.Description).Asc
                .List();
        }

        public IEnumerable<CostGroup> GetAllCostGroupsWithPayItems()
        {
            InitializeSession();
            CostGroup costGroup = null;
            CostGroupPayItem costGroupPayItem = null;
            PayItemMaster payItemMaster = null;

            return Session
                .QueryOver(() => costGroup)
                .Left.JoinQueryOver(() => costGroup.PayItems, () => costGroupPayItem)
                .Left.JoinQueryOver(() => costGroupPayItem.MyPayItem, () => payItemMaster)
                .OrderBy(i => i.Description).Asc
                .List()
                .Distinct();
        }

        public CostGroup GetByName(string name)
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroup>()
                .Where(i => i.Name == name)
                .SingleOrDefault();
        }

        public CostGroupPayItem GetCostGroupPayItem(int payItemId)
        {
            InitializeSession();
            return Session
                .QueryOver<CostGroupPayItem>()
                .Where(i => i.Id == payItemId)
                .SingleOrDefault();
        }
    }
}