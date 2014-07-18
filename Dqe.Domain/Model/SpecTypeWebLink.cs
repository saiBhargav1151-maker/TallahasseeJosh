namespace Dqe.Domain.Model
{
    public class SpecTypeWebLink : DqeWebLink
    {
        public override Transformers.DqeWebLink GetTransformer()
        {
            return new Transformers.SpecTypeWebLink
            {
                Id = Id,
                Name = Name,
                WebLink = WebLink
            };
        }
    }
}