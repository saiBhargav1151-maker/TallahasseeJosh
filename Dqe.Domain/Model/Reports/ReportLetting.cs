using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportLetting
    {
        private readonly ICollection<ReportProposal> _reportProposals = new Collection<ReportProposal>();
        private readonly ICollection<ReportLettingSummary> _reportLettingSummaries = new Collection<ReportLettingSummary>();
        private decimal _total;
 
        public virtual long Id { get; set; }

        public virtual DateTime LettingDate { get; set; }

        public virtual string LettingName { get; set; }

        public virtual decimal GetTotal()
        {
            return _reportProposals.Sum(i => i.Total);
        }

        public virtual decimal Total
        {
            get { return GetTotal();}
            set { _total = value; }
        }

        public virtual void AddReportProposal(ReportProposal reportProposal)
        {
            _reportProposals.Add(reportProposal);
            reportProposal.MyReportLetting = this;
        }

        public virtual void AddReportLettingSummary(ReportLettingSummary reportLettingSummary)
        {
            _reportLettingSummaries.Add(reportLettingSummary);
            reportLettingSummary.MyReportLetting = this;
        }

        public virtual void RemoveReportProposal(ReportProposal reportProposal)
        {
            _reportProposals.Remove(reportProposal);
        }

        public virtual IEnumerable<ReportProposal> ReportProposals
        {
            get
            {
                return _reportProposals
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual IEnumerable<ReportLettingSummary> ReportLettingSummaries
        {
            get
            {
                return _reportLettingSummaries
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}
