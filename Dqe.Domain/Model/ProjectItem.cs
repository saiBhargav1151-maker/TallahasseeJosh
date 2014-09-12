using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Dqe.Domain.Model
{
    public class ProjectItem : Entity<Transformers.ProjectItem>
    {
        [Required]
        public virtual EstimateGroup MyEstimateGroup { get; protected internal set; }

        public virtual decimal Quantity { get; protected internal set; }

        public virtual decimal Price { get; protected internal set; }

        [StringLength(12)]
        public virtual string PayItemNumber { get; protected internal set; }

        [StringLength(256)]
        public virtual string PayItemDescription { get; protected internal set; }

        public override Transformers.ProjectItem GetTransformer()
        {
            return new Transformers.ProjectItem
            {
                Id = Id,
                Quantity = Quantity,
                Price = Price,
                PayItemDescription = PayItemDescription,
                PayItemNumber = PayItemNumber
            };
        }

        public override void Transform(Transformers.ProjectItem transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.DistrictAdministrator && account.Role != DqeRole.Estimator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Quantity = transformer.Quantity;
            Price = transformer.Price;
            PayItemNumber = transformer.PayItemNumber;
            PayItemDescription = transformer.PayItemDescription;
        }
    }
}