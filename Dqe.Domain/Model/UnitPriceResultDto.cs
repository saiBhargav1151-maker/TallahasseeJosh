using System;

namespace Dqe.Domain.Model
{
    public class ProposalItemDTO
    {
        public string ri { get; set; }  // RefItem.Name
        public long Id { get; set; }  // ProposalItem.Id
        public decimal Quantity { get; set; }  
        /*public decimal UnitPrice { get; set; }*/
        /*public decimal ExtendedAmount { get; set; }*/
        public string p { get; set; }  // ProposalNumber
        public string ProposalType { get; set; }
        public string ContractType { get; set; }
        public string ContractWorkType { get; set; }
        public int? m { get; set; }  // NumOfUnit (can be null)
        public string c { get; set; }  // County Description
        public string d { get; set; }  // District Description
        public DateTime? l { get; set; } 
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
        public virtual DateTime? ExecutedDate { get; set; }
        public virtual DateTime ObsoleteDate { get; set; }
        public virtual string BidType { get; set; }
        public virtual int? VendorRanking { get; set; }
        public string CategoryDescription { get; set; }
        public string WorkMixDescription { get; set; }
        public long riId { get; set; }
        public long ProjectId { get; set; }
        public string LeadProjectNumber { get; set; }

        // NHCCI Inflation Adjustment Properties
        public decimal? InflationAdjustedPrice { get; set; }
        public decimal? InflationFactor { get; set; }  
        public decimal? InflationPercentIncrease { get; set; } 
        public string NHCCIQuarter { get; set; }
        public string ProposalStatus { get; set; } 
    }
    public class PayItemDTO
    {
        public string Name { get; set; }  
        public string Description { get; set; }
    }


}
