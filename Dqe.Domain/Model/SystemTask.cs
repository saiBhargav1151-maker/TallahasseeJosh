using System.ComponentModel.DataAnnotations;

namespace Dqe.Domain.Model
{
    public class SystemTask
    {
        public virtual long Id { get; protected internal set; }

        [StringLength(256)]
        public virtual string TaskId { get; set; }
    }
}