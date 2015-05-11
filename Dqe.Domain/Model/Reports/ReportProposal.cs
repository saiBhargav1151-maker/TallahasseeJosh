using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposal
    {
        private readonly ICollection<ReportProposalSummary> _reportProposalSummaries = new Collection<ReportProposalSummary>();
        private readonly ICollection<ReportProposalItem> _reportProposalItems = new Collection<ReportProposalItem>(); 
        private readonly ICollection<ReportProject> _reportProjects = new Collection<ReportProject>();
        private readonly ICollection<ReportProposalMilestone> _reportProposalMilestones = new Collection<ReportProposalMilestone>();
        private readonly ICollection<ReportProposalVendor> _reportProposalVendors = new Collection<ReportProposalVendor>(); 
        private decimal _total;

        public virtual long Id { get; set; }

        public virtual string ProposalNumber { get; set; }

        public virtual ReportProposalLevel ProposalLevel { get; set; }

        public virtual long ConstructionDays { get; set; }

        public virtual decimal CostPerDay { get; set; }

        public virtual string Description { get; set; }

        public virtual string WorkType { get; set; }

        public virtual string County { get; set; }

        /// <summary>
        /// do not know where this comes from
        /// </summary>
        public virtual bool HasOversight { get; set; }

        public virtual string LastUpdatedUser { get; set; }

        public virtual decimal GetTotal()
        {
            return _reportProposalSummaries.Where(i => i.IsLowCost).Sum(i => i.Total);
        }

        public virtual decimal Total
        {
            get { return GetTotal(); }
            set { _total = value; }
        }

        public virtual ReportLetting MyReportLetting { get; protected internal set; }

        public virtual void AddReportProject(ReportProject reportProject)
        {
            _reportProjects.Add(reportProject);
            reportProject.MyReportProposal = this;
        }

        public virtual void AddReportProposalSummary(ReportProposalSummary reportProposalSummary)
        {
            _reportProposalSummaries.Add(reportProposalSummary);
            reportProposalSummary.MyReportProposal = this;
        }

        public virtual void AddReportProposalItem(ReportProposalItem reportProposalItem)
        {
            _reportProposalItems.Add(reportProposalItem);
            reportProposalItem.MyReportProposal = this;
        }

        public virtual void AddReportProposalMilestone(ReportProposalMilestone reportProposalMilestone)
        {
            _reportProposalMilestones.Add(reportProposalMilestone);
            reportProposalMilestone.MyReportProposal = this;
        }

        public virtual void AddReportProposalVendor(ReportProposalVendor reportProposalVendor)
        {
            _reportProposalVendors.Add(reportProposalVendor);
            reportProposalVendor.MyReportProposal = this;
        }

        public virtual IEnumerable<ReportProposalSummary> ReportProposalSummaries
        {
            get
            {
                return _reportProposalSummaries
                    .OrderBy(i => i.CategoryAlternateSet)
                    .ThenBy(i => i.CategoryAlternateMember)
                    .ThenBy(i => i.ItemAlternateSet)
                    .ThenBy(i => i.ItemAlternateMember)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual IEnumerable<ReportProposalItem> ReportProposalItems
        {
            get
            {
                return _reportProposalItems
                    .OrderBy(i => i.CategoryDescription)
                    .ThenBy(i => i.ItemNumber)
                    .ThenBy(i => i.CategoryAlternateSet)
                    .ThenBy(i => i.CategoryAlternateMember)
                    .ThenBy(i => i.ItemAlternateSet)
                    .ThenBy(i => i.ItemAlternateMember)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual IEnumerable<ReportProject> ReportProjects
        {
            get
            {
                return _reportProjects
                    .OrderBy(i => i.ProjectNumber)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual IEnumerable<ReportProposalMilestone> ReportProposalMilestones
        {
            get 
            { 
                return _reportProposalMilestones
                    .ToList().AsReadOnly(); 
            }
        }

        public virtual IEnumerable<ReportProposalVendor> ReportProposalVendors
        {
            get
            {
                return _reportProposalVendors
                    .ToList().AsReadOnly();
            }
        }
    }
}