﻿using DatabaseConnector;
using Histograms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Models;

namespace Histograms.Managers
{
    public abstract class BaseHistogramManager : IHistogramManager<IHistogram, IDbConnector>
    {
        public IDbConnector DbConnector { get; protected set; }
        public List<IHistogram> Histograms { get; }
        public List<string> Tables => Histograms.Select(x => x.TableName).Distinct().ToList();
        public List<string> Attributes
        {
            get
            {
                var returnList = new List<string>();
                foreach (var histogram in Histograms)
                    returnList.Add($"{histogram.TableName}.{histogram.AttributeName}");
                return returnList;
            }
        }
        public int Depth { get; }

        public BaseHistogramManager(int depth)
        {
            Histograms = new List<IHistogram>();
            Depth = depth;
        }

        public void AddHistogram(IHistogram histogram)
        {
            if (string.IsNullOrWhiteSpace(histogram.TableName))
                throw new ArgumentException("Table name cannot be empty!");
            if (string.IsNullOrWhiteSpace(histogram.AttributeName))
                throw new ArgumentException("Attribute name cannot be empty!");
            Histograms.Add(histogram);
        }

        public abstract Task<List<Task>> AddHistogramsFromDB();

        public void ClearHistograms()
        {
            Histograms.Clear();
        }

        public IHistogram GetHistogram(string table, string attribute)
        {
            foreach (var gram in Histograms)
                if (gram.TableName.Equals(table) && gram.AttributeName.Equals(attribute))
                    return gram;

            throw new ArgumentException("No histogram found");
        }
        public List<IHistogram> GetHistogramsByTable(string table)
        {
            List<IHistogram> grams = new List<IHistogram>();
            foreach (var gram in Histograms)
                if (gram.TableName.Equals(table))
                    grams.Add(gram);

            return grams;
        }
        public List<IHistogram> GetHistogramsByAttribute(string attribute)
        {
            List<IHistogram> grams = new List<IHistogram>();
            foreach (var gram in Histograms)
                if (gram.AttributeName.Equals(attribute))
                    grams.Add(gram);

            return grams;
        }
    }
}
