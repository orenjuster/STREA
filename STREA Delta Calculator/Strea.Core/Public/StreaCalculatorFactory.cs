using NinjaTrader.Data;

namespace Strea.Core
{
    public class StreaCalculatorFactory
    {
        public static IStreaCalculator Create(MarketDepth<MarketDepthRow> marketDepth)
        {
            return new StreaCalculator(marketDepth);
        }
    }
}
