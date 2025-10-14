using System;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class Project : Transformer
    {
        public string ProjectNumber { get; set; }
        public int Snapshot { get; protected internal set; }
        public string SnapshotComment { get; set; }
        public long WtId { get; set; }
        public long WtLsDbId { get; set; }
        public string LsDbCode { get; set; }
        public int Version { get; protected internal set; }
        public DateTime? LoadedFromWtOn { get; protected internal set; }
        public string District { get; set; }
        public bool OwnerHasCustody { get; protected internal set; }
        //public DateTime? LettingDate { get; set; }
        public string Description { get; set; }
        public string DesignerName { get; set; }
        public int? PseeContactSrsId { get; set; }
        public DateTime LastUpdated { get; protected internal set; }
        public DateTime Created { get; protected internal set; }
        public SnapshotLabel Label { get; protected internal set; }

        /// <summary>
        /// I believe this column has been utilized to instead represent "See DQE Estimates" Flag in LRE
        /// </summary>
        public string QuantityComplete { get; protected internal set; }
    }
}