namespace Dqe.Domain.Model.Lre
{
    public class PayItemPayItemGroup
    {
        public virtual PayItemPayItemGroupId Id { get; set; }

        public virtual PayItem MyPayItem { get; protected internal set; }

        public virtual PayItemGroup MyPayItemGroup { get; protected internal set; }

        public virtual decimal Quantity { get; set; }

        public virtual string Status { get; set; }
    }
}