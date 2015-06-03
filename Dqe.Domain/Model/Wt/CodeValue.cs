using System;

namespace Dqe.Domain.Model.Wt
{
    public class CodeValue
    {
        public virtual long Id { get; set; }

        public virtual string CodeValueName { get; set; }

        public virtual string Description { get; set; }

        public virtual DateTime? ObsoleteDate { get; set; }

        public virtual string RecordSource { get; set; }

        public virtual DateTime? CreatedDate { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        public virtual string LastUpdatedBy { get; set; }

        public virtual CodeTable MyCodeTable { get; set; }
    }
}