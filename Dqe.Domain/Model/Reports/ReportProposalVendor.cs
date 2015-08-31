using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposalVendor
    { 
        private readonly ICollection<ReportVendorBid> _reportVendorBids = new Collection<ReportVendorBid>();
        private readonly ICollection<ReportMilestoneBid> _reportMilestoneBids = new Collection<ReportMilestoneBid>(); 

        public virtual long Id { get; set; }

        public virtual string Name { get; set; }

        public virtual string BidType { get; set; }

        public virtual string BidStatus { get; set; }

        public virtual bool Awarded { get; set; }

        public virtual decimal Total { get; set; }

        public virtual ReportProposal MyReportProposal { get; protected internal set; }

        public virtual void AddReportVendorBid(ReportVendorBid reportVendorBid)
        {
            _reportVendorBids.Add(reportVendorBid);
            reportVendorBid.MyReportProposalVendor = this;
        }

        public virtual void AddReportMilestoneBid(ReportMilestoneBid reportMilestoneBid)
        {
            _reportMilestoneBids.Add(reportMilestoneBid);
            reportMilestoneBid.MyReportProposalVendor = this;
        }

        public virtual IEnumerable<ReportVendorBid> ReportVendorBids
        {
            get
            {
                return _reportVendorBids
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual IEnumerable<ReportMilestoneBid> ReportMilestoneBids
        {
            get
            {
                return _reportMilestoneBids
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}
