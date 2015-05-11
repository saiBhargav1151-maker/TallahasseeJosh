namespace Dqe.Domain.Model
{
    public enum PriceSetType
    {
        NotSet = 0,
        EstimatorOverride = 1,
        SystemOverride = 2,
        Statewide = 3,
        MarketArea = 4,
        County = 5,
        Parameter = 6,
        Reference = 7,
        Fixed = 9,
        Template = 10
    }
}