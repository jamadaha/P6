﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryTestSuite.Models
{
    internal class AnalysisResult
    {
        public string Name { get; set; }

        public decimal EstimatedCost { get; set; }
        public int EstimatedCardinality { get; set; }

        public int ActualCardinality { get; set; }
        public TimeSpan ActualTime { get; set; }

        public List<AnalysisResult> SubQueries = new List<AnalysisResult>();

        public AnalysisResult(string name, decimal estimatedCost, int estimatedCardinality, int actualCardinality, TimeSpan actualTime)
        {
            Name = name;
            EstimatedCost = estimatedCost;
            EstimatedCardinality = estimatedCardinality;
            ActualCardinality = actualCardinality;
            ActualTime = actualTime;
        }
    }
}
