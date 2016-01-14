namespace Dqe.ApplicationServices.Reports
{
    public class DetailCostEstimate : CostEstimateBase
    {
        public decimal ProposalTotal { get; set; }
        public string ItemAlternateCode { get; set; }
        public decimal ProposalAlternateTotal { get; set; }
        public decimal ProposalBaseTotal { get; set; }
        public decimal Total { get; set; }
    }
}
