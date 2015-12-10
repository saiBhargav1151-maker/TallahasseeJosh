using System;
using System.Security;

namespace Dqe.Domain.Model
{
    public class CostGroupPayItem : Entity<Transformers.CostGroupPayItem>
    {
        public virtual CostGroup MyCostGroup { get; protected internal set; }

        public virtual PayItemMaster MyPayItem { get; protected internal set; }

        public virtual decimal ConversionFactor { get; protected internal set; }

        public override Transformers.CostGroupPayItem GetTransformer()
        {
            return new Transformers.CostGroupPayItem
            {
                Id = Id,
                ConversionFactor = ConversionFactor
            };
        }

        public override void Transform(Transformers.CostGroupPayItem transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.Administrator && account.Role != DqeRole.System)
            {
                throw new SecurityException("Not authorized");
            }
            ConversionFactor = transformer.ConversionFactor;
        }
    }
}
