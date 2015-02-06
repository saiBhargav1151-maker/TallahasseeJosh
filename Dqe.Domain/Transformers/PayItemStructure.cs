using System;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class PayItemStructure : Transformer
    {
        public string StructureId { get; set; }
        public string Title { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ObsoleteDate { get; set; }
        public bool IsPlanQuantity { get; set; }
        public bool IsDoNotBid { get; set; }
        public bool IsFixedPrice { get; set; }
        public int SrsId { get; set; }
        public decimal? FixedAmount { get; set; }
        public int Accuracy { get; set; }
        public string OtherReferenceNotes { get; set; }
        public string RequiredPayItems { get; set; }
        public string RecommendedPayItems { get; set; }
        public string Notes { get; set; }
        public string Details { get; set; }
        public string PendingInformation { get; set; }
        public string EssHistory { get; set; }
        public DateTime? BoeRecentChangeDate { get; set; }
        public string BoeRecentChangeDescription { get; set; }
        public string StructureDescription { get; set; }
        public string PlanSummary { get; set; }
        public bool IsMonitored { get; set; }
        public int? MonitorSrsId { get; set; }
        public SecondaryUnitType? SecondaryUnit { get; set; }
        public PrimaryUnitType PrimaryUnit { get; set; }
    }
}