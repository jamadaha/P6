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

[assembly: InternalsVisibleTo("QueryOptimiserTest")]

namespace QueryOptimiser.Cost.Nodes
{
    internal class JoinEstimateEquiDepthVariance : BaseJoinCost
    {
        public override long GetBucketEstimate(ComparisonType.Type predicate, IHistogramBucket bucket, IHistogramBucket comparisonBucket)
        {
            if (bucket is HistogramBucketVariance vBucket && comparisonBucket is HistogramBucketVariance vComparisonBucket)
            {
                double bucketCertainty;
                if (vBucket.Range == 0 || vBucket.StandardDeviation == 0)
                    bucketCertainty = 1;
                else
                    bucketCertainty = vBucket.StandardDeviation / vBucket.Range;
                double comparisonBucketCertainty;
                if (vComparisonBucket.Range == 0 || vComparisonBucket.StandardDeviation == 0)
                    comparisonBucketCertainty = 1;
                else
                    comparisonBucketCertainty = vComparisonBucket.StandardDeviation / vComparisonBucket.Range;
                double certainty = bucketCertainty / comparisonBucketCertainty;
                if (certainty > 1)
                    certainty = 1 / certainty;
                long estimate = (long)(certainty * vBucket.Count);
                if (estimate == 0)
                    return 1;
                else
                    return estimate;
            } else
            {
                return new JoinEstimateEquiDepth().GetBucketEstimate(predicate, bucket, comparisonBucket);
            }
        }
    }
}
