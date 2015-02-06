using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProject
    {
        private readonly ICollection<ReportCategory> _reportCategories = new Collection<ReportCategory>(); 

        public virtual int Id { get; set; } 

        public virtual string ProjectNumber { get; set; }

        public virtual string Description { get; set; }

        /// <summary>
        /// this is from project generic field PJDT1
        /// </summary>
        public virtual DateTime? LettingDate { get; set; }

        public virtual decimal Total { get { return _reportCategories.Where(i => i.IsLowCost).Sum(i => i.Total); } }

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
    }
}