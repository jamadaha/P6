﻿using QueryPlanParser.Exceptions;
using QueryPlanParser.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryPlanParser.Parsers
{
    public class MySQLParser : BasePlanParser
    {
        public override AnalysisResult ParsePlan(DataSet planData)
        {
            Queue<AnalysisResultWithIndent> resultQueue;

            string[] explainRows = GetExplainRows(planData.Tables[0].Rows);
            var resultRows = ParseQueryAnalysisRows(explainRows);
            resultQueue = new Queue<AnalysisResultWithIndent>(resultRows);

            return RunAnalysis(resultQueue);
        }


        private AnalysisResult RunAnalysis(Queue<AnalysisResultWithIndent> rows)
        {
            AnalysisResultWithIndent? row = rows.Dequeue();
            var analysisRes = row.AnalysisResult;

            // Recursively add subqueries
            while (rows.Count > 0)
            {
                if (rows.Peek().Indentation > row.Indentation)
                    analysisRes.SubQueries.Add(RunAnalysis(rows));
                else
                    break;
            }

            return analysisRes;
        }

        private IEnumerable<AnalysisResultWithIndent> ParseQueryAnalysisRows(string[] rows)
        {
            foreach (string rowStr in rows)
            {
                var parsed = ParseQueryAnalysisRow(rowStr);
                if (parsed != null)
                    yield return parsed;
            }
        }

        private string[] GetExplainRows(DataRowCollection rows)
        {
            if (rows.Count > 0 && rows.Contains("EXPLAIN")) {
                object varObject = rows[0]["EXPLAIN"];
                if (varObject != null)
                    return varObject.ToString()!.Trim().Split('\n');
            }
            throw new BadQueryPlanInputException("Database did not return a correct query plan!");
        }

        private static Regex RowParserRegex = new Regex(@"^(?<indentation> *(?:->)? *)(?<name>[^(\n]+)(?:\((?<predicate>[^)]+)?\) *)?\(cost=(?<cost>\d+.\d+) rows=(?<estimatedRows>\d+)\) \(actual time=(?<timeMin>\d+.\d+)\.\.(?<timeMax>\d+.\d+) rows=(?<actualRows>\d+) loops=(?<loops>\d+)\)", RegexOptions.Compiled);

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
                decimal.Parse(rowData["cost"].Value),
                ulong.Parse(rowData["estimatedRows"].Value),
                ulong.Parse(rowData["actualRows"].Value),
                TimeSpanFromMs(decimal.Parse(rowData["timeMax"].Value))
            );

            // Return
            return new AnalysisResultWithIndent(analysisResult, indentation);
        }
    }
}
