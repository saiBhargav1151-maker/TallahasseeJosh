using Fdot.Entity.Helpers;

namespace Dqe.Domain.Model.Lre
{
    public class PayItemDistrictId : ValueBasedCompositeId
    {
        public virtual string PayItemId { get; set; }

        public virtual string County { get; set; }

        public virtual string District { get; set; }
    }
}