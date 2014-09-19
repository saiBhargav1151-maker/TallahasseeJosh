using System;
using System.ComponentModel.DataAnnotations;

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
            throw new NotImplementedException();
        }

        public override void Transform(Transformers.ProjectItem transformer, DqeUser account)
        {
            throw new NotImplementedException();
        }
    }
}