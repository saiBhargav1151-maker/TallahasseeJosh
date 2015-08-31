namespace Dqe.Domain.Model
{
    public class CaddSummary : DqeCode
    {
        public override Transformers.DqeCode GetTransformer()
        {
            return new Transformers.CaddSummary()
            {
                Id = Id,
                Name = Name,
                IsActive = IsActive
            };
        }
    }
}