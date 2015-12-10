using Fdot.Entity.Helpers;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemCountyId : ValueBasedCompositeId
    {
        public virtual string PayItemId { get; set; }

        public virtual string County { get; set; }
    }
}