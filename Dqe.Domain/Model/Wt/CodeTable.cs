using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class CodeTable
    {
        private readonly ICollection<CodeValue> _codeValues;

        public CodeTable()
        {
            _codeValues = new Collection<CodeValue>();
        }

        [Required]
        public virtual long Id { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string CodeTableName { get; set; }

        [StringLength(256)]
        [Required]
        public virtual string Description { get; set; }

        [StringLength(20)]
        public virtual string RecordSource { get; set; }

        public virtual DateTime? CreatedDate { get; set; }

        [StringLength(256)]
        public virtual string CreatedBy { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        [StringLength(256)]
        public virtual string LastUpdatedBy { get; set; }

        public virtual IEnumerable<CodeValue> CodeValues
        {
            get { return _codeValues.ToList().AsReadOnly(); }
        }

        public virtual void AddCodeValue(CodeValue codeValue)
        {
            _codeValues.Add(codeValue);
        }
    }
}
