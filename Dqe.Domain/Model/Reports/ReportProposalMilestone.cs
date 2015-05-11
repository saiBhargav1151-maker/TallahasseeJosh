using System;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposalMilestone
    {
        private decimal _total;

        public virtual long Id { get; set; }

        public virtual string Description { get; set; }

        public virtual string Unit { get; set; }

        public virtual long ConstructionDays { get; set; }

        public virtual decimal CostPerDay { get; set; }

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
    }
}
