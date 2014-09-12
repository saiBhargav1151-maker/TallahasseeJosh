using System;
using Dqe.Domain.Model;

namespace Dqe.Domain.Transformers
{
    public class Project : Transformer
    {
        public string ProjectNumber { get; set; }
        public int Version { get; protected internal set; }
        public DateTime? LoadedFromWtOn { get; protected internal set; }
        public string District { get; set; }
        public string County { get; set; }
        public DateTime? LettingDate { get; set; }
        public string Description { get; set; }
        public int? DesignerSrsId { get; set; }
        public int? PseeContactSrsId { get; set; }
        public DateTime LastUpdated { get; protected internal set; }
        public DateTime Created { get; protected internal set; }
        public SnapshotType Snapshot { get; protected internal set; }
    }
}