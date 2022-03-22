﻿using DatabaseConnector;
using DatabaseConnector.Connectors;
using QueryParser;
using QueryParser.Models;
using QueryParser.QueryParsers;
using System.Data;
using System.Text;
using Tools.Models;

namespace Histograms.Managers
{
    public class MySQLEquiDepthHistogramManager : IHistogramManager<IHistogram, IDbConnector>
    {
        public IDbConnector DbConnector { get; }
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

        public MySQLEquiDepthHistogramManager(ConnectionProperties connectionProperties, int depth)
        {
            DbConnector = new DatabaseConnector.Connectors.MySqlConnector(connectionProperties);
            Histograms = new List<IHistogram>();
            Depth = depth;
        }

        public void ClearHistograms()
        {
            Histograms.Clear();
        }

        public void AddHistogram(IHistogram histogram)
        {
            if (string.IsNullOrWhiteSpace(histogram.TableName))
                throw new ArgumentException("Table name cannot be empty!");
            if (string.IsNullOrWhiteSpace(histogram.AttributeName))
                throw new ArgumentException("Attribute name cannot be empty!");
            Histograms.Add(histogram);
        }

        public async Task<List<Task>> AddHistogramsFromDB()
        {
            ClearHistograms();
            List<Task> tasks = new List<Task>();
            IEnumerable<DataRow> allTables = (await GetTablesInSchema()).Rows.Cast<DataRow>();
            foreach (var table in allTables)
            {
                Task t = Task.Run(async () =>
                {
                    string tableName = $"{table["TABLE_NAME"]}".ToLower();
                    foreach (DataRow row in (await GetAttributenamesForTable(tableName)).Rows)
                        await AddHistogramForAttribute(row, tableName);
                });
                tasks.Add(t);
            }
            return tasks;
        }

        private async Task<DataTable> GetTablesInSchema()
        {
            var returnRows = await DbConnector.CallQuery($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = database();");
            if (returnRows.Tables.Count > 0)
                return returnRows.Tables[0];
            return new DataTable();
        }

        private async Task<DataTable> GetAttributenamesForTable(string tableName)
        {
            var returnRows = await DbConnector.CallQuery($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = database() AND TABLE_NAME = '{tableName}';");
            if (returnRows.Tables.Count > 0)
                return returnRows.Tables[0];
            return new DataTable();
        }

        private async Task AddHistogramForAttribute(DataRow row, string tableName)
        {
            string attributeName = $"{row["COLUMN_NAME"]}".ToLower();
            var attributeValues = await DbConnector.CallQuery($"SELECT {attributeName} FROM {tableName}");
            if (attributeValues.Tables.Count > 0)
            {
                IHistogram newHistogram = new HistogramEquiDepth(tableName, attributeName, Depth);
                newHistogram.GenerateHistogram(attributeValues.Tables[0], attributeName);
                Histograms.Add(newHistogram);
            }
        }

        public override string? ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Recorded Histograms:");
            foreach (var histogram in Histograms)
                sb.AppendLine(histogram.ToString());
            return sb.ToString();
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
