using System;

namespace Dqe.ApplicationServices.Reports
{
    public class ItemAverageUnitCost
    {
        public string ContractType { get; set; }
        public string Item { get; set; }
        public string Contract { get; set; }
        public string FinancialProject { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public decimal Quantity { get; set; }
        public decimal AwardedPrice { get; set; }
        public decimal ExtendedPrice { get; set; }
        public DateTime LettingDate { get; set; }
    }
}
