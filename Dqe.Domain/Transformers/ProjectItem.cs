namespace Dqe.Domain.Transformers
{
    public class ProjectItem : Transformer
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string UnknownPayItemNumber { get; set; }
        public string UnknownPayItemDescription { get; set; }
    }
}