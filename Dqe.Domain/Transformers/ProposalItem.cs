namespace Dqe.Domain.Transformers
{
    public class ProposalItem : Transformer
    {
        public decimal Quantity { get; set; }
        public string PayItemNumber { get; set; }
        public long WtId { get; set; }
    }
}