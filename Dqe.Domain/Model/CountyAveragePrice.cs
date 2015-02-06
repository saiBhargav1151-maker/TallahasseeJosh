namespace Dqe.Domain.Model
{
    public class CountyAveragePrice : AveragePrice
    {
        public virtual County MyCounty { get; protected internal set; }
    }
}