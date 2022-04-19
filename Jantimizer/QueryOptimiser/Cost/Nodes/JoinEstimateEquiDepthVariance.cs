﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryParser.Models;
using Histograms;
using Histograms.Models;
using DatabaseConnector;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("QueryOptimiserTest")]

namespace QueryOptimiser.Cost.Nodes
{
    public class JoinEstimateEquiDepthVariance : INodeCost<JoinNode>
    {
        public long GetBucketEstimate(ComparisonType.Type predicate, IHistogramBucket bucket, IHistogramBucket comparisonBucket)
        {
            HistogramBucketVariance vBucket = (HistogramBucketVariance)bucket;
            HistogramBucketVariance vComparisonBucket = (HistogramBucketVariance)comparisonBucket;

            double bucketCertainty = GetBucketCertainty(vBucket);
            double comparisonBucketCertainty = GetBucketCertainty(vComparisonBucket);

            double certainty = GetTotalCertainty(bucketCertainty, comparisonBucketCertainty);

            long estimate = (long)(certainty * vBucket.Count);
            if (estimate == 0)
                return 1;
            else
                return estimate;
        }

        internal double GetBucketCertainty(HistogramBucketVariance bucket)
        {
            double bucketCertainty;
            if (bucket.Range == 0 || bucket.StandardDeviation == 0)
                bucketCertainty = 1;
            else
                bucketCertainty = bucket.StandardDeviation / bucket.Range;
            return bucketCertainty;
        }

        internal double GetTotalCertainty(double bucketCertainty, double comparisonBucketCertainty)
        {
            if (comparisonBucketCertainty == 0)
                throw new ArgumentOutOfRangeException("Error, cannot divide certainty by 0!");
            double certainty = bucketCertainty / comparisonBucketCertainty;
            if (certainty > 1)
                certainty = 1 / certainty;
            return certainty;
        }
    }
}
