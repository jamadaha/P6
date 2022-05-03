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
    public abstract class BaseSegmentBoundsHandler : ISegmentHandler
    {
        public Dictionary<TableAttribute, int> UpperBounds { get; }
        public Dictionary<TableAttribute, int> LowerBounds { get; }
        public IHistogramManager HistogramManager { get; }

        protected BaseSegmentBoundsHandler(Dictionary<TableAttribute, int> upperBounds, Dictionary<TableAttribute, int> lowerBounds, IHistogramManager histogramManager)
        {
            UpperBounds = upperBounds;
            LowerBounds = lowerBounds;
            HistogramManager = histogramManager;
        }

        public List<IHistogramSegmentationComparative> GetAllSegmentsForAttribute(TableAttribute attr)
        {
            return HistogramManager.GetHistogram(attr.Table.TableName, attr.Attribute).Segmentations;
        }

        public void AddOrReduceUpperBound(TableAttribute key, int bound)
        {
            if (GetValueFromDictOrAlt(key, UpperBounds, bound) < bound)
                throw new IndexOutOfRangeException("Bound cannot get larger!");
            AddToDictionaryIfNotThere(key, bound, UpperBounds);
        }

        public void AddOrReduceLowerBound(TableAttribute key, int bound)
        {
            if (GetValueFromDictOrAlt(key, LowerBounds, bound) > bound)
                throw new IndexOutOfRangeException("Bound cannot get larger!");
            AddToDictionaryIfNotThere(key, bound, LowerBounds);
        }

        public int GetUpperBoundOrAlt(TableAttribute key, int alt)
        {
            return GetValueFromDictOrAlt(key, UpperBounds, alt);
        }

        public int GetLowerBoundOrAlt(TableAttribute key, int alt)
        {
            return GetValueFromDictOrAlt(key, LowerBounds, alt);
        }

        private void AddToDictionaryIfNotThere(TableAttribute key, int bound, Dictionary<TableAttribute, int> dict)
        {
            if (dict.ContainsKey(key))
                dict[key] = bound;
            else
                dict.Add(key, bound);
        }

        private int GetValueFromDictOrAlt(TableAttribute key, Dictionary<TableAttribute, int> dict, int alt)
        {
            if (dict.ContainsKey(key))
                return dict[key];
            return alt;
        }
    }
}
