using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dqe.Domain.Messaging;

namespace Dqe.Domain.Model
{
    public class UserAccount : IValidatableObject
    {
        private readonly IMessenger _messenger;

        public UserAccount(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public virtual int Id { get; protected set; }

        [Required]
        [StringLength(255)]
        public virtual string Email { get; set; }

        [Required]
        [StringLength(25)]
        public virtual string AccountPassword { get; set; }

        [Required]
        [StringLength(25)]
        public virtual string FirstName { get; set; }

        [Required]
        [StringLength(35)]
        public virtual string LastName { get; set; }

        [Required]
        [StringLength(15)]
        public virtual string AccountRole { get; set; }

        public virtual string UnverifiedAccountToken { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}