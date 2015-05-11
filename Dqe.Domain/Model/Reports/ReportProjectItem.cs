using System;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProjectItem
    {
        private decimal _total;

        public virtual long Id { get; set; }

        public virtual string ItemNumber { get; set; }

        public virtual string LineNumber { get; set; }

        public virtual string Description { get; set; }

        public virtual string SupplementalDescription { get; set; }

        /// <summary>
        /// ContractClass from PayItemMaster
        /// </summary>
        public virtual string WorkClass { get; set; }

        /// <summary>
        /// Ilflg1 from PayItemMaster
        /// </summary>
        public virtual string TechSpec { get; set; }

        /// <summary>
        /// NonBid on PayItemMaster = true
        /// </summary>
        public virtual bool DoNotBid { get; set; }

        /// <summary>
        /// e.g. AA
        /// </summary>
        public virtual string ItemAlternateSet { get; set; }

        /// <summary>
        /// e.g. 1
        /// </summary>
        public virtual string ItemAlternateMember { get; set; }

        public virtual bool IsLowCost { get; set; }

        public virtual string Unit { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual decimal Price { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        public virtual bool IsObsolete { get; set; }

        public virtual decimal GetTotal()
        {
            return Math.Round(Price*Quantity, 2);
        }

        public virtual decimal Total
        {
            get { return GetTotal(); }
            set { _total = value; }
        }

        public virtual ReportCategory MyReportCategory { get; protected internal set; }
    }
}