using System;
using System.Collections;

namespace Dqe.ApplicationServices
{
    public class Bid
    {
        public DateTime LettingDate { get; set; }
        public bool IsAwarded { get; set; }
        public bool IsLowCost { get; set; }
        public bool IsBlank { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TimeFactor { get; set; }
        public decimal TimeWeight { get; set; }
        public bool Included { get; set; }
    }
}