using System;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class PayItemMaster : Transformer
    {
        //public int Id { get; set; }

        public string SpecBook { get; set; }

        public string RefItemName { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public string CalculatedUnit { get; set; }

        public string UnitSystem { get; set; }

        public int SrsId { get; set; }

        public bool LumpSum { get; set; }

        public bool IsFixedPrice { get; set; }

        public bool BidAsLumpSum { get; set; }

        public bool SuppDescriptionRequired { get; set; }

        public bool CombineWithLikeItems { get; set; }

        public bool IsFrontLoadedItem { get; set; }

        public bool MajorItem { get; set; }

        public bool NonBid { get; set; }

        public bool DbeInterest { get; set; }

        public decimal? RefPrice { get; set; }

        public DateTime? ObsoleteDate { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public decimal ConcreteFactor { get; set; }

        public decimal AsphaltFactor { get; set; }

        public bool IsFederalFunded { get; set; }

        public string FactorNotes { get; set; }

        public string CommonUnit { get; set; }

        public string ItemType { get; set; }

        public string ItemClass { get; set; }

        public string ContractClass { get; set; }

        public string ShortDescription { get; set; }

        public decimal? ConversionFactorToCommonUnit { get; set; }

        public decimal? DbePercentToApply { get; set; }

        public string RecordSource { get; set; }

        public string BidRequirementCode { get; set; }

        public bool Administrative { get; set; }

        public bool ExemptFromMaa { get; set; }

        public bool PayPlan { get; set; }

        public bool AutoPaidPercentSchedule { get; set; }

        public bool CoApprovalRequired { get; set; }

        public bool PercentScheduleItem { get; set; }

        public bool FuelAdjustment { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public string LastUpdatedBy { get; set; }

        public bool SpecialtyItem { get; set; }

        public bool ExemptFromRetainage { get; set; }

        public bool RegressionInclusion { get; set; }

        public string AlternateItemName { get; set; }

        public string FuelAdjustmentType { get; set; }

        //public DateTime? Ildt1 { get; set; }

        public DateTime? Ildt2 { get; set; }

        public DateTime? OpenedDate { get; set; }

        public string Ilflg1 { get; set; }

        public decimal? Ilnum1 { get; set; }

        public string Ilsst1 { get; set; }

        public string Illst1 { get; set; }

        public string Itmqtyprecsn { get; set; }

        public decimal? StateReferencePrice { get; set; }
    }
}