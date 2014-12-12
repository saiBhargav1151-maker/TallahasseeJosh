namespace Dqe.Domain.Model
{
    public class ItemSet
    {
        public string Set { get; set; }
        public string Member { get; set; }
        public decimal Total { get; set; }
        public bool Included { get; internal set; }
    }
}