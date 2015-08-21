using System;

namespace Dqe.Domain.Model.Wt
{
    public class RefItem
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string SpecBook { get; set; }

        public virtual string Description { get; set; }

        public virtual string Unit { get; set; }

        public virtual string CalculatedUnit { get; set; }

        public virtual string UnitSystem { get; set; }

        public virtual bool LumpSum { get; set; }

        public virtual bool BidAsLumpSum { get; set; }

        public virtual bool SuppDescriptionRequired { get; set; }

        public virtual bool CombineWithLikeItems { get; set; }

        public virtual bool MajorItem { get; set; }

        public virtual bool NonBid { get; set; }

        public virtual bool DbeInterest { get; set; }

        public virtual decimal? Price { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        public virtual string CommonUnit { get; set; }

        public virtual string ItemType { get; set; }

        public virtual string ItemClass { get; set; }

        public virtual string ContractClass { get; set; }

        public virtual string ShortDescription { get; set; }

        public virtual decimal? ConversionFactorToCommonUnits { get; set; }

        public virtual decimal? DbePercentToApply { get; set; }

        public virtual string RecordSource { get; set; }

        public virtual string BidRequirementCode { get; set; }

        public virtual bool Administrative { get; set; }

        public virtual bool ExemptFromMaa { get; set; }

        public virtual bool PayPlan { get; set; }

        public virtual bool AutoPaidPercentSchedule { get; set; }

        public virtual bool CoApprovalRequired { get; set; }

        public virtual bool PercentScheduleItem { get; set; }

        public virtual bool FuelAdjustment { get; set; }

        public virtual DateTime? CreatedDate { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        public virtual string LastUpdatedBy { get; set; }

        public virtual bool SpecialtyItem { get; set; }

        public virtual bool ExemptFromRetainage { get; set; }

        public virtual bool RegressionInclusion { get; set; }

        public virtual string AlternateItemName { get; set; }

        public virtual string FuelAdjustmentType { get; set; }

        public virtual DateTime? IlDate1 { get; set; }

        public virtual DateTime? IlDate2 { get; set; }

        public virtual DateTime? IlDate3 { get; set; }

        public virtual string IlFlag1 { get; set; }

        public virtual decimal? IlNumber1 { get; set; }

        public virtual string Ilsst1 { get; set; }

        public virtual string Illst1 { get; set; }

        public virtual string Itmqtyprecsn { get; set; }
    }
}