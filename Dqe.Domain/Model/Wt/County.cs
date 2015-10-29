namespace Dqe.Domain.Model.Wt
{
    public class County
    {
        public virtual long Id { get; set; }

        public virtual decimal Percentage { get; set; }

        public virtual bool PrimaryCounty { get; set; }

        public virtual Project MyProject { get; set; }

        public virtual RefCounty MyRefCounty { get; set; }
    }
}