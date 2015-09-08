namespace Dqe.Domain.Transformers
{
    public abstract class DqeWebLink : Transformer
    {
        public string Name { get; set; }
        public string WebLink { get; set; }
    }
}