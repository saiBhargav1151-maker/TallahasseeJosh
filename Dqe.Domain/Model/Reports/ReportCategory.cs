using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportCategory
    {
        private readonly ICollection<ReportProjectItem> _reportProjectItems = new Collection<ReportProjectItem>();
        private decimal _total;

        public virtual long Id { get; set; }

        public virtual string Description { get; set; }

        public virtual string AlternateDescription { get; set; }

        /// <summary>
        /// e.g. AA
        /// </summary>
        public virtual string CategoryAlternateSet { get; set; }

        /// <summary>
        /// e.g. 1
        /// </summary>
        public virtual string CategoryAlternateMember { get; set; }

        public virtual bool IsLowCost { get; set; }

        public virtual decimal GetTotal()
        {
            return _reportProjectItems.Where(i => i.IsLowCost).Sum(i => i.Total);
        }

        public virtual decimal Total
        {
            get { return GetTotal(); }
            set { _total = value; }
        }

        public virtual IEnumerable<ReportProjectItem> ReportProjectItems
        {
            get
            {
                return _reportProjectItems
                    .OrderBy(i => i.ItemNumber)
                    .ThenBy(i => i.ItemAlternateSet)
                    .ThenBy(i => i.ItemAlternateMember)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual ReportProject MyReportProject { get; protected internal set; }

        public virtual void AddProjectItem(ReportProjectItem reportProjectItem)
        {
            _reportProjectItems.Add(reportProjectItem);
            reportProjectItem.MyReportCategory = this;
        }
    }
}