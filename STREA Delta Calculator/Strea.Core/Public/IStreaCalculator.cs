using NinjaTrader.Data;

namespace Strea.Core
{
    public interface IStreaCalculator
    {
        StreaModel CreateSnapshot(MarketDepth<MarketDepthRow> marketDepth);

        StreaLevelOneModel CreateSnapshot(MarketDataEventArgs marketDataEventArgs);
    }
}