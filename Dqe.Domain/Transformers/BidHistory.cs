namespace Dqe.Domain.Transformers
{
    public class BidHistory : Transformer
    {
        public decimal Price { get; set; }

        public bool IsAwarded { get; set; }

        public bool IsLowCost { get; set; }

        public bool IsEstimate { get; set; }

        public decimal BidTotal { get; set; }
    }
}