using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dqe.Domain.Transformers;

namespace Dqe.Domain.Model
{
    public abstract class Entity<T> where T : Transformer
    {
        [Range(1, int.MaxValue)]
        public virtual int Id { get; protected internal set; }

        public abstract T GetTransformer();

        public abstract void Transform(T transformer, DqeUser account);

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }
}