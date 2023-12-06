namespace Strea.Core
{
    internal class MarketDepthContext
    {
        public long InitialSumAsk { get; }

        public long InitialSumBid { get; }
        
        public long MaxDelta { get; private set; }
        
        public long MinDelta { get; private set; }

        public MarketDepthContext(long initialSumAsk, long initialSumBid)
        {
            InitialSumAsk = initialSumAsk;
            InitialSumBid = initialSumBid;
            MaxDelta = 0;
            MinDelta = 0;
        }

        public void UpdateMinMaxDeltas(long currentDelta)
        {
            if (currentDelta > MaxDelta) MaxDelta = currentDelta;
            if (currentDelta < MinDelta) MinDelta = currentDelta;
        }
    }
}