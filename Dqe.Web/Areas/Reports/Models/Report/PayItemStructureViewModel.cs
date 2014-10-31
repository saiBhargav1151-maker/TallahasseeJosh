using System.Collections.Generic;

namespace Dqe.Web.Areas.Reports.Models.Report
{
    public class PayItemStructureViewModel
    {
        public string PayItemStructureId { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Accuracy { get; set; }
        public bool PlanQuantity { get; set; }
        public string Notes { get; set; }
        public string RelatedItemsRequired { get; set; }
        public string RelatedItemsRecommended { get; set; }
        public string PlanSummaryBox { get; set; }
        public string DesignFormsAndDocumentation { get; set; }
        public string ConstructionFormsAndDocumentation { get; set; }
        public string ReferencesPpmChapter { get; set; }
        public string ReferencesTrnsport { get; set; }
        public string ReferencesOther { get; set; }
        public string ReferencesStandards { get; set; }
        public string ReferencesSpecifications { get; set; }
        public string ReferencesPrepDocManualChapters { get; set; }
        public string TransportCategory { get; set; }
        public string StuructureCode { get; set; }
        public string StructureNotes { get; set; }
        public IEnumerable<PayItemViewModel> PayItems { get; set; }
    }
}