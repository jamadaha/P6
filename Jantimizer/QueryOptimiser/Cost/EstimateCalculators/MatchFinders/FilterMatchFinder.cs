﻿using Histograms.Models;
using QueryOptimiser.Cost.Nodes;
using QueryOptimiser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Models.JsonModels;

namespace QueryOptimiser.Cost.EstimateCalculators.MatchFinders
{
    public class FilterMatchFinder : BaseMatchFinder<IFilterEstimate>
    {
        public FilterMatchFinder(IFilterEstimate estimator) : base(estimator)
        {
        }

        internal List<IntermediateBucket> GetMatches(FilterNode node, List<IHistogramBucket> buckets)
        {
            switch (node.GetComType())
            {
                case ComparisonType.Type.Equal:
                    return GetEqualityMatches(node, buckets);
                case ComparisonType.Type.Less:
                case ComparisonType.Type.More:
                case ComparisonType.Type.EqualOrMore:
                case ComparisonType.Type.EqualOrLess:
                    return GetInEqualityMatches(node, buckets);
                default:
                    throw new ArgumentException($"Invalid comparison type {node.ToString()}");
            }
        }

        internal List<IntermediateBucket> GetEqualityMatches(FilterNode node, List<IHistogramBucket> buckets)
        {
            List<IntermediateBucket> intermediateBuckets = new List<IntermediateBucket>();
            bool matches = false;
            for (int i = 0; i < buckets.Count; i++)
            {
                if (DoesOverlap(node.Constant, buckets[i]))
                {
                    matches = true;
                    intermediateBuckets.Add(MakeNewIntermediateBucket(node.FilterAttribute, MatchType.Overlap, node.GetComType(), node.Constant, buckets[i]));
                }
                else if (matches)
                    break;
            }
            return intermediateBuckets;
        }

        internal List<IntermediateBucket> GetInEqualityMatches(FilterNode node, List<IHistogramBucket> buckets)
        {
            List<IntermediateBucket> intermediateBuckets = new List<IntermediateBucket>();
            for (int i = 0; i < buckets.Count; i++)
            {
                MatchType type = DoesMatch(node.GetComType(), node.Constant, buckets[i]);
                if (type == MatchType.Match || type == MatchType.Overlap)
                    intermediateBuckets.Add(MakeNewIntermediateBucket(node.FilterAttribute, type, node.GetComType(), node.Constant, buckets[i]));
            }
            return intermediateBuckets;
        }

        internal IntermediateBucket MakeNewIntermediateBucket(TableAttribute tableAttribute, MatchType matchType, ComparisonType.Type comType, IComparable constant, IHistogramBucket bucket)
        {
            return MakeNewIntermediateBucket(
                            new List<TableAttribute>() { tableAttribute },
                            new List<BucketEstimate>() { GetEstimate(matchType, comType, constant, bucket) }
                        );
        }

        internal BucketEstimate GetEstimate(MatchType matchType, ComparisonType.Type comType, IComparable constant, IHistogramBucket bucket)
        {
            if (matchType == MatchType.Match)
                return new BucketEstimate(bucket, bucket.Count);
            else if (matchType == MatchType.Overlap)
                return new BucketEstimate(bucket, Estimator.GetBucketEstimate(comType, constant, bucket));
            else
                throw new ArgumentException($"Invalid matchType{matchType}");
        }
    }
}
