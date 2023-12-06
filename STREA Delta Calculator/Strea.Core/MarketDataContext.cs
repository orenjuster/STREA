using NinjaTrader.Data;

namespace Strea.Core
{
    internal class MarketDataContext
    {
        private long _askSum;
        private long _bidSum;

        public long MinDelta { get; private set; }

        public long MaxDelta { get; private set; }

        public long Delta => _askSum - _bidSum;

        public long Delta2 => _bidSum - _askSum;

        public MarketDataContext()
        {
            _askSum = 0;
            _bidSum = 0;
            MinDelta = 0;
            MaxDelta = 0;
        }

        public void Update(MarketDataType marketDataType, long lastVolume)
        {
            if (marketDataType == MarketDataType.Ask)
            {
                _askSum += lastVolume;
            }
            else if (marketDataType == MarketDataType.Bid)
            {
                _bidSum += lastVolume;
            }
            else return;

            var delta = Delta;

            if (delta > MaxDelta) MaxDelta = delta;
            if (delta < MinDelta) MinDelta = delta;
        }
    }
}