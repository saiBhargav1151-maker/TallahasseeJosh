using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposalMilestone
    {
        private readonly ICollection<ReportMilestoneBid> _reportMilestoneBids = new Collection<ReportMilestoneBid>(); 

        private decimal _total;

        public virtual long Id { get; set; }

        public virtual long WtId { get; set; }

        public virtual string Description { get; set; }

        public virtual string Unit { get; set; }

        public virtual long ConstructionDays { get; set; }

        public virtual decimal CostPerDay { get; set; }

        public virtual decimal AverageBidPrice { get; set; }

        public virtual decimal Magnitude { get; set; }

        public virtual decimal Total
        {
            get { return GetTotal(); }
            set { _total = value; }
        }

        public virtual decimal GetTotal()
        {
            return Math.Round(CostPerDay * ConstructionDays, 2);
        }

        public virtual ReportProposal MyReportProposal { get; set; }

        public virtual void AddReportMilestoneBid(ReportMilestoneBid reportMilestoneBid)
        {
            _reportMilestoneBids.Add(reportMilestoneBid);
            reportMilestoneBid.MyReportProposalMilestone = this;
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
