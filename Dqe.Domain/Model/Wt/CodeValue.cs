using System;
using System.ComponentModel.DataAnnotations;

namespace Dqe.Domain.Model.Wt
{
    public class CodeValue
    {
        [Required]
        public virtual long Id { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string CodeValueName { get; set; }

        [StringLength(256)]
        [Required]
        public virtual string Description { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        [StringLength(20)]
        public virtual string RecordSource { get; set; }

        public virtual DateTime? CreatedDate { get; set; }

        [StringLength(256)]
        public virtual string CreatedBy { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        [StringLength(256)]
        public virtual string LastUpdatedBy { get; set; }

        public virtual CodeTable MyCodeTable { get; set; }
    }
}