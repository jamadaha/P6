﻿using DatabaseConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Histograms.Models;

namespace Histograms
{
    public interface IHistogramManager<HistogramType, ConnectorType> 
        where HistogramType : IHistogram 
        where ConnectorType : IDbConnector
    {
        public ConnectorType DbConnector { get; }
        public List<IHistogram> Histograms { get; }
        public List<string> Tables { get; }
        public List<string> Attributes { get; }

        public Task<List<Task>> AddHistogramsFromDB();
        public void AddHistogram(IHistogram histogram);
        public void ClearHistograms();

        public IHistogram GetHistogram(string table, string attribute);
        public List<IHistogram> GetHistogramsByTable(string table);
        public List<IHistogram> GetHistogramsByAttribute(string attribute);
    }
}
