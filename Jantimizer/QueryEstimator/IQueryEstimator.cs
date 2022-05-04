﻿using QueryEstimator.Models;
using QueryEstimator.PredicateEstimators;
using Milestoner;
using Tools.Models.JsonModels;

namespace QueryEstimator
{
    public interface IQueryEstimator<T> 
        where T : notnull
    {
        public IMilestoner Milestoner { get; }
        public EstimatorResult GetQueryEstimation(T query);
    }
}