using System;
using Fdot.Entity.Helpers;

namespace Dqe.Domain.Model.Lre
{
    public class VersionSnapshotId : ValueBasedCompositeId
    {
        public virtual long ProjectSnapshotId { get; set; }

        public virtual DateTime SnapshotDateTime { get; set; }
    }
}