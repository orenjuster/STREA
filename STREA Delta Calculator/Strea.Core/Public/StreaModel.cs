using System;

namespace Strea.Core
{
    public class StreaModel
    {
        public long SumAsk { get; set; }

        public long SumBid { get; set; }

        public long Delta { get; set; }

        public long MaximumDelta { get; set; }

        public long MinimumDelta { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}