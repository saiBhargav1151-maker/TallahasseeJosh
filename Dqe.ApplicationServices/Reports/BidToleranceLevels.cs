namespace Dqe.ApplicationServices.Reports
{
    public class BidToleranceLevels
    {
        public string LettingNumber { get; set; }
        public string Contract { get; set; }
        public string OversightIndicator { get; set; }
        public decimal AdvertisedEstimate { get; set; }
        public decimal OfficialEstimate { get; set; }
        public decimal LowTolerance { get; set; }
        public decimal HighTolerance { get; set; }
    }
}
