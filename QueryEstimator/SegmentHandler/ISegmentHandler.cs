﻿using Histograms;
using Histograms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Models.JsonModels;

namespace QueryEstimator.SegmentHandler
{
    public interface ISegmentHandler
    {
        public Dictionary<TableAttribute, int> UpperBounds { get; }
        public Dictionary<TableAttribute, int> LowerBounds { get; }
        public IHistogramManager HistogramManager { get; }

        public List<IHistogramSegmentationComparative> GetAllSegmentsForAttribute(TableAttribute attr);
    }
}