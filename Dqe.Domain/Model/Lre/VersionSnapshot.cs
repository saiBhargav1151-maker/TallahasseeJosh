namespace Dqe.Domain.Model.Lre
{
    public class VersionSnapshot
    {
        public virtual VersionSnapshotId Id { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual string LabelCode { get; set; }

        public virtual ProjectSnapshot MyProjectSnapshot { get; protected internal set; }
    }
}