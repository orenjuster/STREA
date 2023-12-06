using NinjaTrader.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Strea.Core
{
    internal class StreaCalculator : IStreaCalculator
    {        
        private readonly MarketDepthContext _marketDepthContext;
        private readonly MarketDataContext _marketDataContext;

        public StreaCalculator(MarketDepth<MarketDepthRow> marketDepth)
        {
            var initialSumAsk = CalculateSumMarketDepthRowVolume(marketDepth.Asks);
            var initialSumBid = CalculateSumMarketDepthRowVolume(marketDepth.Bids);

            _marketDepthContext = new MarketDepthContext(initialSumAsk, initialSumBid);
            _marketDataContext = new MarketDataContext();
        }

        public StreaModel CreateSnapshot(MarketDepth<MarketDepthRow> marketDepth)
        {
            long sumAsk = CalculateMarketDepthRowVolumeDeltaFromInitial(_marketDepthContext.InitialSumAsk, marketDepth.Asks);
            long sumBid = CalculateMarketDepthRowVolumeDeltaFromInitial(_marketDepthContext.InitialSumBid, marketDepth.Bids);
            
            long currentDelta = CalculateDelta(sumAsk, sumBid);

            _marketDepthContext.UpdateMinMaxDeltas(currentDelta);

            return new StreaModel
            {
                TimeStamp = DateTime.UtcNow,
                SumAsk = sumAsk,
                SumBid = sumBid,
                Delta = currentDelta,
                MinimumDelta = _marketDepthContext.MinDelta,
                MaximumDelta = _marketDepthContext.MaxDelta
            };
        }

        public StreaLevelOneModel CreateSnapshot(MarketDataEventArgs marketDataEventArgs)
        {
            _marketDataContext.Update(marketDataEventArgs.MarketDataType, marketDataEventArgs.Instrument.MarketData.Last.Volume);
            
            return new StreaLevelOneModel
            {
                Delta = _marketDataContext.Delta,
                MaximumDelta = _marketDataContext.MaxDelta,
                MinimumDelta = _marketDataContext.MinDelta,
                DeltaBidMinusAsk = _marketDataContext.Delta2
            };
        }

        private long CalculateDelta(long sumAsk, long sumBid)
        {
            return sumAsk - sumBid;
        }

        private long CalculateMarketDepthRowVolumeDeltaFromInitial(long initialSum, IEnumerable<MarketDepthRow> marketDepthRows)
            => initialSum - CalculateSumMarketDepthRowVolume(marketDepthRows);

        private long CalculateSumMarketDepthRowVolume(IEnumerable<MarketDepthRow> marketDepthRows)
            => marketDepthRows.Sum(x => x.Volume);
    }
}