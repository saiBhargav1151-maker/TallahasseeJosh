namespace Dqe.Domain.Model
{
    public enum PriceSetType
    {
        NotSet = 'N',
        EstimatorOverride = 'O',
        SystemOverride = 'X',
        Statewide = 'S',
        MarketArea = 'M',
        County = 'C',
        Parameter = 'P',
        Reference = 'R',
        Fixed = 'F',
        Template = 'T'
    }
}