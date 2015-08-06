namespace Dqe.Domain.Transformers
{
    public abstract class DqeCode : Transformer
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}