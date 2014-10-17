namespace Dqe.Domain.Transformers
{
    public class ProjectItem : Transformer
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public string PayItemNumber { get; set; }
        public string PayItemDescription { get; set; }
        public string CalculatedUnit { get; set; }
        public string Unit { get; set; }
        public bool IsLumpSum { get; set; }
        public string ItemClass { get; set; }
        public string ContractClass { get; set; }
        public bool CombineWithLikeItems { get; set; }
        public long WtId { get; set; }
    }
}