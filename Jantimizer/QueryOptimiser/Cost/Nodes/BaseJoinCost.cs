﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryParser.Models;
using Histograms;
using Histograms.Models;
using DatabaseConnector;
using QueryOptimiser.Models;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QueryOptimiserTest")]

namespace QueryOptimiser.Cost.Nodes
{
    internal abstract class BaseJoinCost : INodeCost<JoinNode>
    {
        public abstract long GetBucketEstimate(ComparisonType.Type predicate, IHistogramBucket bucket, IHistogramBucket comparisonBucket);
    }
}
