using Fdot.Entity.Helpers;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemPayItemGroupId : ValueBasedCompositeId
    {
        public virtual string PayItemGroupId { get; set; }

        public virtual string PayItemId { get; set; }
    }
}