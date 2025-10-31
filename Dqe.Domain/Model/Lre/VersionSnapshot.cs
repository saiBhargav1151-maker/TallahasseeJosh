using System;

namespace Dqe.Domain.Model.Lre
{
    public class VersionSnapshot
    {
        public virtual VersionSnapshotId Id { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual int ProjectVersionNumber { get; set; }

        /// <summary>
        /// 00 - 70
        /// </summary>
        public virtual string LabelCode { get; set; }

        public virtual string VersionStatusCode { get; set; }

        public virtual decimal MaintenanceTrafficCost { get; set; }

        public virtual decimal MobileCost { get; set; }

        public virtual decimal ScopeCreepAmount { get; set; }

        public virtual decimal DesignBuildAmount { get; set; }

        public virtual string LastUpdatedBy { get; set; }

        public virtual DateTime LastUpdatedOn { get; set; }

        public virtual string PrimaryVersionCode { get; set; }

        public virtual string CreatedCode { get; set; }

        public virtual string Description { get; set; }

        public virtual ProjectSnapshot MyProjectSnapshot { get; set; }
    }
}