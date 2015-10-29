using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Lre;

namespace Dqe.Domain.Fdot
{
    public interface ILreService
    {
        IEnumerable<Model.Lre.Project> GetProjects(string projectName);
        ProjectSnapshot GetProjectSnapshot(long id);
        IEnumerable<PayItemGroup> GetLrePickLists();
        PayItem GetLrePayItem(string payItemName);
        void UpdateRefItem(PayItemMaster payItemMaster, DqeUser user);
        void UpdateRefItem(PayItemMaster payItemMaster, dynamic lrePickLists, DqeUser user);
        void SetDqeSnapshotInLre(Model.Project p, DqeUser account, SnapshotLabel label, decimal amount);
        void UpdateLrePrices(IEnumerable<PayItemMaster> items);
    }
}
