namespace Dqe.Domain.Transformers
{
    public class ProjectItem : Transformer
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string PayItemNumber { get; set; }
        public string PayItemDescription { get; set; }
    }
}