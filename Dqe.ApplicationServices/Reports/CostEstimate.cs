using System.Collections.Generic;

namespace Dqe.ApplicationServices.Reports
{
    public class CostEstimate : CostEstimateBase
    {
        public string FuelAdjustment { get; set; }
        public string TotalLength { get; set; }
        public string TotalBridgeLength { get; set; }
        public List<string> Counties { get; set; } 
        public string FederalAidNumber { get; set; }
        public string DesignedBy { get; set; }
        public string ProposedLettingDate { get; set; }
        public string Category { get; set; }
        public string CategoryDescription { get; set; }
        public string FundingSource { get; set; }
        public string ConstructionClass { get; set; }
        public string MaintenanceActivity { get; set; }
        public string RoadSectionNumber { get; set; }
        public string StructureWorkClass { get; set; }
        public string BridgeId { get; set; }
        public string BridgeSpans { get; set; }
        public string BridgeLength { get; set; }
        public string BridgeWidth { get; set; }
        public string BridgeType { get; set; }
        public string WorkType { get; set; }
        public string CategoryLength { get; set; }
        public string CategoryWidth { get; set; }
        public string LineNumber { get; set; }
        public string ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public decimal EstimatedQuantity { get; set; }
        public string ItemUnit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal GroupAmount { get; set; }
        public bool Included { get; set; }
        public decimal FinancialProjectTotal { get; set; }
        public string AbSite { get; set; }
        public int EstimatedDays { get; set; }
        public decimal CostPerDay { get; set; }
        public decimal TimeTotal { get; set; }
        public decimal ProposalStandardTotal { get; set; }
        public decimal ProposalTimeTotal { get; set; }
        public decimal ProposalTotal { get; set; }
    }
}
