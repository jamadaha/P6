﻿using CsvHelper;
using CsvHelper.Configuration;
using DatabaseConnector;
using DatabaseConnector.Connectors;
using Histograms;
using Histograms.Managers;
using PrintUtilities;
using QueryOptimiser;
using QueryOptimiser.QueryGenerators;
using QueryParser;
using QueryParser.Models;
using QueryParser.QueryParsers;
using QueryPlanParser.Models;
using QueryTestSuite.Models;
using QueryTestSuite.Services;
using System.Data;

namespace QueryTestSuite.TestRunners
{
    internal class TestRunner
    {
        public DBConnectorParser DatabaseModel { get; }
        public FileInfo SetupFile { get; private set; }
        public FileInfo CleanupFile { get; private set; }
        public IEnumerable<FileInfo> CaseFiles { get; private set; }
        public List<TestCase> Results { get; private set; }
        public IHistogramManager<HistogramEquiDepth, IDbConnector> HistogramManager { get; private set; }
        private CSVWriter csvWriter;

        public TestRunner(DBConnectorParser databaseModel, FileInfo setupFile, FileInfo cleanupFile, IEnumerable<FileInfo> caseFiles, DateTime timeStamp)
        {
            DatabaseModel = databaseModel;
            SetupFile = setupFile;
            CleanupFile = cleanupFile;
            CaseFiles = caseFiles;
            Results = new List<TestCase>();
            csvWriter = new CSVWriter($"Results/{timeStamp.ToString("yyyy/MM/dd/HH.mm.ss")}", "result.csv");
            HistogramManager = new PostgresEquiDepthHistogramManager(databaseModel.Connector.ConnectionString, 10);
        }

        public async Task<List<TestCase>> Run(bool consoleOutput = true, bool saveResult = true)
        {
            PrintUtil.PrintLine($"Running Cleanup: {CleanupFile.Name}", 1, ConsoleColor.Red);
            await DatabaseModel.Connector.CallQuery(CleanupFile);

            PrintUtil.PrintLine($"Running Setup: {SetupFile.Name}", 1, ConsoleColor.Blue);
            await DatabaseModel.Connector.CallQuery(SetupFile);

            PrintUtil.PrintLine($"Generating histograms", 1, ConsoleColor.Blue);
            await HistogramManager.AddHistograms(SetupFile);

            Results = await RunQueriesSerial();

            PrintUtil.PrintLine($"Running Cleanup: {CleanupFile.Name}", 1, ConsoleColor.Red);
            await DatabaseModel.Connector.CallQuery(CleanupFile);

            if (consoleOutput)
                WriteResultToConsole();
            if (saveResult)
                SaveResult();


            return Results;
        }

        private async Task<List<TestCase>> RunQueriesSerial()
        {
            PrintUtil.PrintLine($"Running tests for [{DatabaseModel.Name}] connector", 2, ConsoleColor.Green);
            var testCases = new List<TestCase>();
            int count = 0;
            int max = CaseFiles.Count();
            foreach (FileInfo queryFile in CaseFiles)
            {
                try
                {
                    PrintUtil.PrintProgressBar(count, max, 50, true, 2);
                    PrintUtil.Print($"\t [File: {queryFile.Name}]    ", 0, ConsoleColor.Blue);
                    PrintUtil.Print($"\t Executing SQL statement...             ", 0);
                    DataSet dbResult = await DatabaseModel.Connector.AnalyseQuery(queryFile);
                    AnalysisResult analysisResult = DatabaseModel.Parser.ParsePlan(dbResult);

                    IQueryOptimiser<HistogramEquiDepth, IDbConnector> optimiser = new QueryOptimiserEquiDepth(
                        new PostgresGenerator(),
                        HistogramManager);
                    IParserManager manager = new ParserManager(new List<IQueryParser>() { 
                        new JoinQueryParser()    
                    });
                    List<INode> nodes = manager.ParseQuery(File.ReadAllText(queryFile.FullName), false);

                    AnalysisResult jantimiserResult = new AnalysisResult(
                        "Jantimiser",
                        0,
                        optimiser.OptimiseQueryCardinality(nodes),
                        0,
                        new TimeSpan());
                    
                    TestCase testCase = new TestCase(queryFile, analysisResult, jantimiserResult);
                    testCases.Add(testCase);
                }
                catch (Exception ex)
                {
                    PrintUtil.PrintLine($"Error! The query file [{queryFile}] failed with the following error:", 1);
                    PrintUtil.PrintLine(ex.ToString(), 1);
                }
                count++;
            }
            PrintUtil.PrintProgressBar(max, max, 50, true, 2);
            PrintUtil.PrintLine(" Finished!                                                             ", 0, ConsoleColor.Green);
            return testCases;
        }

        private void WriteResultToConsole()
        {
            PrintUtil.PrintLine($"Displaying report for [{DatabaseModel.Name}] analysis", 2, ConsoleColor.Green);
            PrintUtil.PrintLine(FormatList("Category", "Case Name", "P. Db Rows", "P. Jantimiser Rows", "Actual Rows", "DB Acc (%)", "Jantimiser Acc (%)"), 2, ConsoleColor.DarkGray);
            foreach (var testCase in Results)
            {
                PrintUtil.PrintLine(FormatList(
                    testCase.Category, 
                    testCase.Name, 
                    testCase.TestResult.EstimatedCardinality.ToString(),
                    testCase.JantimiserResult.EstimatedCardinality.ToString(),
                    testCase.TestResult.ActualCardinality.ToString(), 
                    GetAccuracy(testCase.TestResult.ActualCardinality, testCase.TestResult.EstimatedCardinality),
                    GetAccuracy(testCase.TestResult.ActualCardinality, testCase.JantimiserResult.EstimatedCardinality)
                    ), 2, ConsoleColor.Blue);
            }
        }

        private string GetAccuracy(ulong actualValue, ulong predictedValue)
        {
            if (actualValue == 0 && predictedValue == 0)
                return "100   %";
            if (actualValue == 0)
                return "inf   %";
            if (actualValue != 0 && predictedValue == 0)
                return "inf   %";
            if (actualValue < predictedValue)
            {
                decimal value = ((decimal)actualValue / (decimal)predictedValue) * 100;
                return string.Format("{0, -5} %", Math.Round(value, 2));
            }
            if (actualValue > predictedValue)
            {
                decimal value = ((decimal)predictedValue / (decimal)actualValue) * 100;
                return string.Format("{0, -5} %", Math.Round(value, 2));
            }
            return "100   %";
        }

        private string FormatList(string category, string caseName, string predicted, string actual, string jantimiser, string dBAccuracy, string jantimiserAccuracy)
        {
            return string.Format("{0,-30} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10}", category, caseName, predicted, actual, jantimiser, dBAccuracy, jantimiserAccuracy);
        }

        private void SaveResult()
        {
            csvWriter.Write();
        }
    }
}
