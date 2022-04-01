﻿using Histograms.DataGatherers;
using Histograms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Models;

namespace Histograms.Managers
{
    public abstract class BaseEquiDepthHistogramManager : BaseHistogramManager, IDepthHistogramManager
    {
        public int Depth { get; }

        protected BaseEquiDepthHistogramManager(IDataGatherer dataGatherer, int depth) : base(dataGatherer)
        {
            Depth = depth;
        }

        protected override async Task AddHistogramForAttribute(string attributeName, string tableName)
        {
            IDepthHistogram newHistogram = new HistogramEquiDepth(tableName, attributeName, Depth);
            newHistogram.GenerateHistogramFromSortedGroups(await DataGatherer.GetSortedGroupsFromDb(tableName, attributeName));
            await Task.Delay(1);
            AddHistogram(newHistogram);
        }
    }
}
