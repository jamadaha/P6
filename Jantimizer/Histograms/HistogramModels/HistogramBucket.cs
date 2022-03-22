﻿namespace Histograms
{
    public class HistogramBucket : IHistogramBucket
    {
        public IComparable ValueStart { get; }
        private IComparable _valueEnd;
        public IComparable ValueEnd { get => _valueEnd; internal set {
                if (value.CompareTo(ValueStart) < 0)
                    throw new IndexOutOfRangeException("Bucket end value cannot be lower than start value!");
                _valueEnd = value;
            } }
        private int count;
        public int Count { get => count; internal set
            { 
                if (value < 0)
                    throw new IndexOutOfRangeException("Count for a bucket cannot be less than 0!");
                count = value;
            } }

// Suppress warning for _valueEnd "not being set"
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HistogramBucket(IComparable valueStart, IComparable valueEnd, int count)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if(valueStart is null)
                throw new ArgumentNullException(nameof(valueStart));
            if(valueEnd is null)
                throw new ArgumentNullException(nameof(valueEnd));

            ValueStart = valueStart;
            ValueEnd = valueEnd;
            Count = count;
        }

        public override string? ToString()
        {
            return $"Start: [{ValueStart}], End: [{ValueEnd}], Count: [{Count}]";
        }
    }
}
