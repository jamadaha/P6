﻿using Histograms;
using Histograms.Models;
using QueryEstimator.Models;
using QueryEstimator.Models.BoundResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Helpers;
using Tools.Models.JsonModels;

namespace QueryEstimator.PredicateBounders
{
    public class SimpleFilterBounder : BasePredicateBounder<IComparable>
    {
        public SimpleFilterBounder(Dictionary<TableAttribute, int> upperBounds, Dictionary<TableAttribute, int> lowerBounds, IHistogramManager histogramManager) : base(upperBounds, lowerBounds, histogramManager)
        {
        }

        public override IPredicateBoundResult<IComparable> Bound(TableAttribute source, IComparable compare, ComparisonType.Type type)
        {
            var allSourceSegments = GetAllSegmentsForAttribute(source);
            int currentSourceLowerBound = GetLowerBoundOrAlt(source, 0);
            int newSourceLowerBound = currentSourceLowerBound;
            int currentSourceUpperBound = GetUpperBoundOrAlt(source, allSourceSegments.Count - 1);
            int newSourceUpperBound = currentSourceUpperBound;

            var compType = compare.GetType();
            var valueType = allSourceSegments[currentSourceLowerBound].LowestValue.GetType();
            if (compType != valueType)
                compare = (IComparable)Convert.ChangeType(compare, valueType);

            bool foundAny = false;
            bool exitSentinel = false;
            for (int i = currentSourceLowerBound; i <= currentSourceUpperBound; i++)
            {
                switch (type)
                {
                    case ComparisonType.Type.More:
                        if (allSourceSegments[i].LowestValue.IsLargerThan(compare))
                        {
                            exitSentinel = true;
                            break;
                        }
                        newSourceLowerBound = i;
                        break;
                    case ComparisonType.Type.EqualOrMore:
                        if (allSourceSegments[i].LowestValue.IsLargerThanOrEqual(compare))
                        {
                            exitSentinel = true;
                            break;
                        }
                        newSourceLowerBound = i;
                        break;
                    case ComparisonType.Type.Less:
                        if (allSourceSegments[i].LowestValue.IsLargerThan(compare))
                        {
                            exitSentinel = true;
                            break;
                        }
                        newSourceUpperBound = i;
                        break;
                    case ComparisonType.Type.EqualOrLess:
                        if (allSourceSegments[i].LowestValue.IsLargerThanOrEqual(compare))
                        {
                            exitSentinel = true;
                            break;
                        }
                        newSourceUpperBound = i;
                        break;
                    case ComparisonType.Type.Equal:
                        if (allSourceSegments[i].LowestValue.IsLargerThanOrEqual(compare))
                        {
                            newSourceUpperBound = i;
                            foundAny = true;
                            exitSentinel = true;
                            break;
                        }
                        if (!foundAny)
                            newSourceLowerBound = i;
                        break;
                }
                if (exitSentinel)
                    break;
            }

            AddToUpperBoundIfNotThere(source, newSourceUpperBound);
            AddToLowerBoundIfNotThere(source, newSourceLowerBound);

            return new PredicateBoundResult<IComparable>(this, source, compare, type, newSourceUpperBound, newSourceLowerBound);
        }
    }
}