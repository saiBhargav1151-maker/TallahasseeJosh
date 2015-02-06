using System;
using System.Collections;

namespace Dqe.ApplicationServices
{
    public class Bid
    {
        public int Id { get; set; }
        public DateTime LettingDate { get; set; }
        public decimal Quantity { get; set; }
        public string County { get; set; }
        public bool IsAwarded { get; set; }
        public bool IsLowCost { get; set; }
        public bool IsBlank { get; set; }
        public decimal Price { get; set; }
        public decimal BidTotal { get; set; }
        public decimal LocationWeight { get; set; }
        public decimal QuantityWeight { get; set; }
        public decimal TimeWeight { get; set; }
        public bool Included { get; set; }
        public decimal AbsoluteDeviation { get; set; }
    }
}