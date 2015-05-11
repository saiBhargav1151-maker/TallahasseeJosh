using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProject
    {
        private readonly ICollection<ReportProjectSummary> _reportProjectSummaries = new Collection<ReportProjectSummary>();
        private readonly ICollection<ReportCategory> _reportCategories = new Collection<ReportCategory>();
        private decimal _total;

        public virtual long Id { get; set; } 

        public virtual string ProjectNumber { get; set; }

        public virtual string Description { get; set; }

        public virtual string County { get; set; }

        public virtual string District { get; set; }

        /// <summary>
        /// this is from project generic field PJDT1
        /// </summary>
        public virtual DateTime? LettingDate { get; set; }

        public virtual decimal GetTotal()
        {
            return _reportCategories.Where(i => i.IsLowCost).Sum(i => i.Total);
        }

        public virtual decimal Total
        {
            get { return GetTotal(); }
            set { _total = value; }
        }

        public virtual IEnumerable<ReportCategory> ReportCategories
        {
            get { return _reportCategories
                .OrderBy(i => i.Description)
                .ThenBy(i => i.CategoryAlternateSet)
                .ThenBy(i => i.CategoryAlternateMember)
                .ToList()
                .AsReadOnly(); }
        }

        public virtual ReportProposal MyReportProposal { get; protected internal set; }

        public virtual void AddCategory(ReportCategory reportCategory)
        {
            _reportCategories.Add(reportCategory);
            reportCategory.MyReportProject = this;
        }

        public virtual void AddReportProjectSummary(ReportProjectSummary reportProjectSummary)
        {
            _reportProjectSummaries.Add(reportProjectSummary);
            reportProjectSummary.MyReportProject = this;
        }

        public virtual IEnumerable<ReportProjectSummary> ReportProjectSummaries
        {
            get
            {
                return _reportProjectSummaries
                    .OrderBy(i => i.CategoryAlternateSet)
                    .ThenBy(i => i.CategoryAlternateMember)
                    .ThenBy(i => i.ItemAlternateSet)
                    .ThenBy(i => i.ItemAlternateMember)
                    .ToList()
                    .AsReadOnly();
            }
        }
    }
}