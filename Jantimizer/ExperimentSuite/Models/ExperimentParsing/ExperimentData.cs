﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentSuite.Models.ExperimentParsing
{
    internal class ExperimentData
    {
        public string ExperimentName { get; set; }
        public bool RunParallel { get; set; }
        public bool RunExperiment { get; set; }
        public List<TestRunData> RunData { get; set; }
        public List<TestRunData> PreRunData { get; set; }

        public ExperimentData(string experimentName, bool runParallel, bool runExperiment, List<TestRunData> runData, List<TestRunData> preRunData)
        {
            ExperimentName = experimentName;
            RunParallel = runParallel;
            RunExperiment = runExperiment;
            RunData = runData;
            PreRunData = preRunData;
        }
    }
}