namespace Dqe.Domain.Model.Lre
{
    public class PayItemDistrict
    {
        public virtual PayItemDistrictId Id { get; set; }

        public virtual PayItemCounty MyPayItemCounty { get; set; }

        public virtual decimal UnitPrice { get; set; }

        public virtual string LockCode { get; set; }
    }
}