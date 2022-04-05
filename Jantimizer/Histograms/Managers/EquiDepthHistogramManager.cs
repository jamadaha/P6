﻿using Histograms.DataGatherers;
using Histograms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Caches;
using Tools.Models;

namespace Histograms.Managers
{
    public class EquiDepthHistogramManager : BaseHistogramManager, IDepthHistogramManager
    {
        public int Depth { get; }

        public EquiDepthHistogramManager(IDataGatherer dataGatherer, int depth) : base(dataGatherer)
        {
            Depth = depth;
        }

        protected override async Task AddHistogramForAttribute(string attributeName, string tableName)
        {
            var cacheHisto = await DataGatherer.GetHistogramFromCacheOrNull(tableName, attributeName);
            if (cacheHisto == null)
            {
                IDepthHistogram newHistogram = new HistogramEquiDepth(tableName, attributeName, Depth);
                newHistogram.GenerateHistogramFromSortedGroups(await DataGatherer.GetSortedGroupsFromDb(tableName, attributeName));
                string columnHash = await DataGatherer.GetTableAttributeColumnHash(tableName, attributeName);
                HistogramCache.Instance.AddToCacheIfNotThere(new string[] { tableName, attributeName, columnHash }, newHistogram);
                AddHistogram(newHistogram);
            }
            else
                AddHistogram(cacheHisto);
        }
    }
}