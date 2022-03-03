﻿using QueryTestSuite.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryTestSuite.Parsers
{
    public class PostgreSqlParser : BasePlanParser
    {
        public override AnalysisResult GetAnalysis(DataSet planData)
        {
            Queue<AnalysisResultWithIndent> resultQueue;

            try
            {
                var resultRows = ParseQueryAnalysisRows(planData.Tables[0].Rows);
                resultQueue = new Queue<AnalysisResultWithIndent>(resultRows);
            }
            catch (NoNullAllowedException)
            {
                throw new NoNullAllowedException($"Unexpected null-value from PostGre Analysis");
            }

            return RunAnalysis(resultQueue);
        }

        private AnalysisResult RunAnalysis(Queue<AnalysisResultWithIndent> rows)
        {
            AnalysisResultWithIndent? row = rows.Dequeue();
            var analysisRes = row.AnalysisResult;

            // Recursively add subqueries
            while (rows.Count > 0)
            {
                if (rows.Peek().indentation > row.indentation)
                    analysisRes.SubQueries.Add(RunAnalysis(rows));
                else
                    break;
            }

            return analysisRes;
        }

        private IEnumerable<AnalysisResultWithIndent> ParseQueryAnalysisRows(DataRowCollection rows)
        {
            foreach (DataRow row in rows)
            {
                var rowStr = row["QUERY PLAN"].ToString();
                if (string.IsNullOrEmpty(rowStr))
                    throw new NoNullAllowedException($"Unexpected null-value: {{{rowStr}}}");

                var parsed = ParseQueryAnalysisRow(rowStr);
                if (parsed != null)
                    yield return parsed;
            }
        }



        private static Regex RowParserRegex = new Regex(@"^(?<indentation> *(?:->)? *)(?<name>[^(\n]+)\(cost=(?<costMin>\d+.\d+)\.\.(?<costMax>\d+.\d+) rows=(?<estimatedRows>\d+) width=(?<width>\d+)\) \(actual time=(?<timeMin>\d+.\d+)\.\.(?<timeMax>\d+.\d+) rows=(?<actualRows>\d+) loops=(?<loops>\d+)\)", RegexOptions.Compiled);

        /// <summary>
        /// Returns null if the row isn't a new subquery, e.g. if the row is the condition on a join.
        /// </summary>
        /// <param name="rowStr">The content from a single row after running EXPLAIN ANALYZE</param>
        /// <returns>AnalysisResult, IndentationSize</returns>
        private AnalysisResultWithIndent? ParseQueryAnalysisRow(string rowStr)
        {
            // Regex Matching
            var match = RowParserRegex.Match(rowStr);

            if (!match.Success)
                return null;

            var rowData = match.Groups;

            // Data Parsing
            int indentation = rowData["indentation"].ToString().Length;

            // Create Result
            var analysisResult = new AnalysisResult(
                rowData["name"].ToString().Trim(),
                decimal.Parse(rowData["costMin"].Value),
                ulong.Parse(rowData["estimatedRows"].Value),
                ulong.Parse(rowData["actualRows"].Value),
                TimeSpanFromMs(decimal.Parse(rowData["timeMax"].Value))
            );

            // Return
            return new AnalysisResultWithIndent(analysisResult, indentation);
        }
    }
}
