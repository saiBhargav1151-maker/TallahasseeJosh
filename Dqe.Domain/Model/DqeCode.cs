using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Dqe.Domain.Model
{
    public abstract class DqeCode : Entity<Transformers.DqeCode>
    {
        [Required]
        [StringLength(255)]
        public virtual string Name { get; protected internal set; }

        public virtual bool IsActive { get; protected internal set; }

        public override void Transform(Transformers.DqeCode transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Name = transformer.Name;
            IsActive = transformer.IsActive;
        }
    }
}