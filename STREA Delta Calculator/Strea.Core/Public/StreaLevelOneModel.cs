namespace Strea.Core
{
    public class StreaLevelOneModel
    {
        public double Delta { get; set; }

        public double MaximumDelta { get; set; }

        public double MinimumDelta { get; set; }

        public long DeltaBidMinusAsk { get; internal set; }
    }
}