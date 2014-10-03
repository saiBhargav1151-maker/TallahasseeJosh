using System;
using System.ComponentModel.DataAnnotations;

namespace Dqe.Domain.Model.Wt
{
    public class RefItem
    {
        [Required]
        public virtual long Id { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string Name { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string SpecBook { get; set; }

        [StringLength(256)]
        [Required]
        public virtual string Description { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string Unit { get; set; }

        [StringLength(280)]
        public virtual string CalculatedUnit { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string UnitSystem { get; set; }

        [Required]
        public virtual bool LumpSum { get; set; }

        [Required]
        public virtual bool BidAsLumpSum { get; set; }

        [Required]
        public virtual bool SuppDescriptionRequired { get; set; }

        [Required]
        public virtual bool CombineWithLikeItems { get; set; }

        [Required]
        public virtual bool MajorItem { get; set; }

        [Required]
        public virtual bool NonBid { get; set; }

        [Required]
        public virtual bool DbeInterest { get; set; }

        public virtual decimal? Price { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        [StringLength(20)]
        public virtual string CommonUnit { get; set; }

        [StringLength(20)]
        public virtual string ItemType { get; set; }

        [StringLength(20)]
        public virtual string ItemClass { get; set; }

        [StringLength(20)]
        public virtual string ContractClass { get; set; }

        [StringLength(256)]
        public virtual string ShortDescription { get; set; }

        public virtual decimal? ConversionFactorToCommonUnits { get; set; }

        public virtual decimal? DbePercentToApply { get; set; }

        [StringLength(20)]
        public virtual string RecordSource { get; set; }

        [StringLength(20)]
        public virtual string BidRequirementCode { get; set; }

        [Required]
        public virtual bool Administrative { get; set; }

        [Required]
        public virtual bool ExemptFromMaa { get; set; }

        [Required]
        public virtual bool PayPlan { get; set; }

        [Required]
        public virtual bool AutoPaidPercentSchedule { get; set; }

        [Required]
        public virtual bool CoApprovalRequired { get; set; }

        [Required]
        public virtual bool PercentScheduleItem { get; set; }

        [Required]
        public virtual bool FuelAdjustment { get; set; }

        public virtual DateTime? CreatedDate { get; set; }

        [StringLength(256)]
        public virtual string CreatedBy { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        [StringLength(256)]
        public virtual string LastUpdatedBy { get; set; }

        [Required]
        public virtual bool SpecialtyItem { get; set; }

        [Required]
        public virtual bool ExemptFromRetainage { get; set; }

        [Required]
        public virtual bool RegressionInclusion { get; set; }

        [StringLength(20)]
        public virtual string AlternateItemName { get; set; }

        [StringLength(20)]
        public virtual string FuelAdjustmentType { get; set; }

        public virtual DateTime? IlDate1 { get; set; }

        public virtual DateTime? IlDate2 { get; set; }

        public virtual DateTime? IlDate3 { get; set; }

        [StringLength(1)]
        public virtual string IlFlag1 { get; set; }

        public virtual decimal? IlNumber1 { get; set; }

        [StringLength(256)]
        public virtual string Ilsst1 { get; set; }

        [StringLength(256)]
        public virtual string Illst1 { get; set; }
    }
}