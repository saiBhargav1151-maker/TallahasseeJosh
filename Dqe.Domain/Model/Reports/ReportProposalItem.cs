using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Reports
{
    public class ReportProposalItem
    {
        private readonly ICollection<ReportVendorBid> _reportVendorBids = new Collection<ReportVendorBid>(); 

        private decimal _total;

        public virtual long Id { get; set; }

        public virtual string ItemNumber { get; set; }

        public virtual string LineNumber { get; set; }

        public virtual string CategoryDescription { get; set; }

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

        public virtual string Unit { get; set; }

        public virtual decimal Quantity { get; set; }

        public virtual decimal Price { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        public virtual bool IsObsolete { get; set; }

        public virtual decimal AverageBidPrice { get; set; }

        public virtual decimal Magnitude { get; set; }

        public virtual decimal GetTotal()
        {
            return Math.Round(Price * Quantity, 2, MidpointRounding.AwayFromZero);
        }

        public virtual decimal Total
        {
            get { return GetTotal();}
            set { _total = value; }
        }

        public virtual ReportProposal MyReportProposal { get; protected internal set; }

        public virtual void AddReportVendorBid(ReportVendorBid reportVendorBid)
        {
            _reportVendorBids.Add(reportVendorBid);
            reportVendorBid.MyReportProposalItem = this;
        }

        public virtual void RemoveReportVendorBid(ReportVendorBid reportVendorBid)
        {
            _reportVendorBids.Remove(reportVendorBid);
            reportVendorBid.MyReportProposalItem = this;
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
    }
}