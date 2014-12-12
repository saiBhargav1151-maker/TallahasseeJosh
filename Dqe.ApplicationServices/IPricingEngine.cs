namespace Dqe.ApplicationServices
{
    public interface IPricingEngine
    {
        void CalculateAveragePrice(BidSet bidSet);
    }
}