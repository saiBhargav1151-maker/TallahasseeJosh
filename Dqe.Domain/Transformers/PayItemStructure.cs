using System;
using System.Collections;
using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class PayItemStructure : Transformer
    {
        //public string StructureId { get; set; }
        //public string Title { get; set; }
        //public DateTime EffectiveDate { get; set; }
        //public DateTime? ObsoleteDate { get; set; }
        //public bool IsPlanQuantity { get; set; }
        //public bool IsDoNotBid { get; set; }
        //public bool IsFixedPrice { get; set; }
        //public int SrsId { get; set; }
        //public decimal? FixedAmount { get; set; }
        //public int Accuracy { get; set; }
        //public string OtherReferenceNotes { get; set; }
        //public string RequiredPayItems { get; set; }
        //public string RecommendedPayItems { get; set; }
        //public string Notes { get; set; }
        //public string Details { get; set; }
        //public string PendingInformation { get; set; }
        //public string EssHistory { get; set; }
        //public DateTime? BoeRecentChangeDate { get; set; }
        //public string BoeRecentChangeDescription { get; set; }
        //public string StructureDescription { get; set; }
        //public string PlanSummary { get; set; }
        //public bool IsMonitored { get; set; }
        //public int? MonitorSrsId { get; set; }
        //public SecondaryUnitType? SecondaryUnit { get; set; }
        //public PrimaryUnitType PrimaryUnit { get; set; }
        //public int Id { get; set; }
        public string StructureId { get; set; }
        public string Title { get; set; }
        //public string PrimaryUnit { get; set; }
        //public string SecondaryUnit { get; set; }
        //public string Accuracy { get; set; }
        public bool? IsPlanQuantity { get; set; }
        public string Notes { get; set; }
        public string Details { get; set; }
        public string PlanSummary { get; set; }
        public string DesignFormsText { get; set; }
        public string ConstructionFormsText { get; set; }
        public string PpmChapterText { get; set; }
        public string TrnsportText { get; set; }
        public string OtherText { get; set; }
        public string StandardsText { get; set; }
        public string SpecificationsText { get; set; }
        public string StructureDescription { get; set; }
        public int SrsId { get; set; }
        public bool IsMonitored { get; set; }
        public int? MonitorSrsId { get; set; }
        public string PendingInformation { get; set; }
        public string EssHistory { get; set; }
        public DateTime? BoeRecentChangeDate { get; set; }
        public string BoeRecentChangeDescription { get; set; }
        public IList<string> Units { get; set; }
        public string RequiredItems { get; set; }
        public string RecommendedItems { get; set; }
    }
}