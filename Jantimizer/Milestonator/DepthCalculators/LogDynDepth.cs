﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Milestoner.DepthCalculators
{
    public class LogDynDepth : BaseDepthCalculator, IDepthCalculator
    {
        public double LogBase { get; set; }
        public double Multiplier { get; set; }

        public LogDynDepth(bool shouldUseUniqueValues, double logBase, double multiplier) : base (shouldUseUniqueValues)
        {
            LogBase = logBase;
            Multiplier = multiplier;
        }

        private double LogN(long x, double logBase)
        {
            return Math.Log2(x)/Math.Log2(logBase);
        }

        protected override double DepthFunction(long x)
        {
            return LogN(x, LogBase) * Multiplier;
        }
    }
}
