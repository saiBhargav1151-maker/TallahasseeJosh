using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dqe.Domain.Model
{
    public class ProposalItemDTO
    {
        public string ri { get; set; }  // RefItem.Name
        public long Id { get; set; }  // ProposalItem.Id
        public decimal Quantity { get; set; }  // Quantity is often decimal in bid data
        /*public decimal UnitPrice { get; set; }*/
        /*public decimal ExtendedAmount { get; set; }*/
        public string p { get; set; }  // ProposalNumber
        public string ProposalType { get; set; }
        public string ContractType { get; set; }
        public string ContractWorkType { get; set; }
        public int? m { get; set; }  // NumOfUnit (can be null)
        public string c { get; set; }  // County Description
        public string d { get; set; }  // District Description
        public DateTime? l { get; set; }  // LettingDate
        public decimal b { get; set; }  // BidPrice
        public string BidStatus { get; set; }
        public decimal PvBidTotal { get; set; }
        public string ProjectNumber { get; set; }
        public string Description { get; set; }
        public string SupplementalDescription { get; set; }
        public string CalculatedUnit { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public string VendorName { get; set; }
        public string FullNameDescription { get; set; }
        public long Duration { get; set; }
        public virtual DateTime ExecutedDate { get; set; }
        public virtual DateTime ObsoleteDate { get; set; }
        public virtual string BidType { get; set; }
        public virtual int? VendorRanking { get; set; }
        public string CategoryDescription { get; set; }
        public string WorkMixDescription { get; set; }
        public long riId { get; set; }
        public long ProjectId { get; set; }
        public string LeadProjectNumber { get; set; }

        // NHCCI Inflation Adjustment Properties
        public decimal? InflationAdjustedPrice { get; set; }  // Inflation-adjusted unit price
        public decimal? InflationFactor { get; set; }  // Inflation factor (Latest Index / Letting Date Index)
        public decimal? InflationPercentIncrease { get; set; }  // Percentage increase due to inflation
        public string NHCCIQuarter { get; set; }  // NHCCI quarter for the letting date
    }
    public class PayItemDTO
    {
        public string Name { get; set; }         // "0110 86"
        public string Description { get; set; }
    }


}
