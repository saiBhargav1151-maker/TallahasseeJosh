using System;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposalSummary
    {
        public virtual int Id { get; set; }

        public virtual string AlternateDescription { get; set; }

        /// <summary>
        /// e.g. AA
        /// </summary>
        public virtual string CategoryAlternateSet { get; set; }

        /// <summary>
        /// e.g. 1
        /// </summary>
        public virtual string CategoryAlternateMember { get; set; }

        /// <summary>
        /// e.g. AA
        /// </summary>
        public virtual string ItemAlternateSet { get; set; }

        /// <summary>
        /// e.g. 1
        /// </summary>
        public virtual string ItemAlternateMember { get; set; }

        public virtual bool IsLowCost { get; set; }

        public virtual decimal Total { get; set; }

        public virtual ReportProposal MyReportProposal { get; protected internal set; }
    }
}