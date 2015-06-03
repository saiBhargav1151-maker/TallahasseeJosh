namespace Dqe.Domain.Model.Reports
{
    public class ReportVendorBid
    {
        public virtual long Id { get; set; }

        public virtual decimal BidPrice { get; set; }

        public virtual bool LowCost { get; set; }

        public virtual string Tolerance { get; set; }

        public virtual ReportProposalVendor MyReportProposalVendor { get; protected internal set; }

        public virtual ReportProposalItem MyReportProposalItem { get; protected internal set; }
    }
}
