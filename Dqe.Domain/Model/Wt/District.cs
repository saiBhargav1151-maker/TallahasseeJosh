namespace Dqe.Domain.Model.Wt
{
    public class District
    {
        public virtual long Id { get; set; }

        public virtual bool PrimaryDistrict { get; set; }

        public virtual Project MyProject { get; set; }

        public virtual RefDistrict MyRefDistrict { get; set; }
    }
}