using System;
using System.ComponentModel.DataAnnotations;
using System.Security;
using DataAnnotationsExtensions;

namespace Dqe.Domain.Model
{
    public abstract class DqeWebLink : Entity<Transformers.DqeWebLink>
    {
        [Required]
        [StringLength(255)]
        public virtual string Name { get; protected internal set; }

        [Url]
        [Required]
        [StringLength(1000)]
        [Display(Name = "Web Link")]
        public virtual string WebLink { get; protected internal set; }

        public override void Transform(Transformers.DqeWebLink transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator && account.Role != DqeRole.PayItemAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Name = transformer.Name;
            WebLink = transformer.WebLink;
        }
    }
}