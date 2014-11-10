namespace Dqe.Domain.Model
{
    public class CostGroup : DqeCode
    {
        public override Transformers.DqeCode GetTransformer()
        {
            return new Transformers.CostGroup
            {
                Id = Id,
                Name = Name,
                IsActive = IsActive
            };
        }
    }
}