﻿using System.Data;
using System.Text;

namespace Histograms
{
    /// <summary>
    /// Contains n buckets, which each represent the same m number of elements.
    /// </summary>
    public class HistogramEquiDepth : IHistogram
    {
        public List<IHistogramBucket> Buckets { get; }
        public string TableName { get; }
        public string AttributeName { get; }

        public int Depth { get; }

        public HistogramEquiDepth(string tableName, string attributeName, int depth)
        {
            TableName = tableName;
            AttributeName = attributeName;
            Depth = depth;
            Buckets = new List<IHistogramBucket>();
        }

        public void GenerateHistogram(DataTable table, string key)
        {
            List<int> sorted = table.AsEnumerable().Select(x => (int)x[key]).OrderByDescending(x => -x).ToList();
            GenerateHistogramFromSorted(sorted);
        }

        public void GenerateHistogram(List<int> column)
        {
            List<int> sorted = column.OrderByDescending(x => -x).ToList();
            GenerateHistogramFromSorted(sorted);
        }

        private void GenerateHistogramFromSorted(List<int> sorted)
        {
            for (int bStart = 0; bStart < sorted.Count; bStart += Depth)
            {
                IHistogramBucket newBucket = new HistogramBucket(sorted[bStart], 1);
                for (int bIter = bStart + 1; bIter < bStart + Depth && bIter < sorted.Count; bIter++)
                    newBucket.Count++;
                Buckets.Add(newBucket);
            }
        }

        public override string? ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t Data for attribute {TableName}.{AttributeName}:");
            foreach(var bucket in Buckets)
            {
                sb.AppendLine($"\t\t{bucket}");
            }
            return sb.ToString();
        }
    }
}