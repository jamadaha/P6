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

namespace QueryOptimiser.Cost.Nodes
{
    public interface IFilterEstimate : INodeEstimate<FilterNode>
    {
        public long GetBucketEstimate(ComparisonType.Type comparisonType, IComparable constant, IHistogramBucket bucket);
    }
}
