namespace Dqe.ApplicationServices.Reports
{
    public class CategoryFundingSummary : CostEstimateBase
    {
        public string FundClass { get; set; }
        public string FederalAidNumber { get; set; }
        public string Category { get; set; }
        public long Cost { get; set; }
        public long ConstEngr { get; set; }
        public long Total { get; set; }
        public long Funding { get; set; }

    }
}
