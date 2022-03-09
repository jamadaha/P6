﻿using CsvHelper;
using CsvHelper.Configuration;
using QueryTestSuite.Models;
using System.Globalization;

namespace QueryTestSuite.TestRunners
{
    internal class TestRunner
    {
        public DatabaseCommunicator DatabaseModel { get; }
        public FileInfo SetupFile { get; private set; }
        public FileInfo CleanupFile { get; private set; }
        public IEnumerable<FileInfo> CaseFiles { get; private set; }
        public List<TestCase> Results { get; private set; }
        private DateTime TimeStamp;

        public TestRunner(DatabaseCommunicator databaseModel, FileInfo setupFile, FileInfo cleanupFile, IEnumerable<FileInfo> caseFiles, DateTime timeStamp)
        {
            DatabaseModel = databaseModel;
            SetupFile = setupFile;
            CleanupFile = cleanupFile;
            CaseFiles = caseFiles;
            Results = new List<TestCase>();
            TimeStamp = timeStamp;
        }

        public async Task<List<TestCase>> Run(bool consoleOutput = true, bool saveResult = true)
        {
            Console.WriteLine($"Running Cleanup: {CleanupFile}");
            await DatabaseModel.Connector.CallQuery(CleanupFile);

            Console.WriteLine($"Running Setup: {SetupFile}");
            await DatabaseModel.Connector.CallQuery(SetupFile);

            Results = await RunQueriesSerial();

            Console.WriteLine($"Running Cleanup: {CleanupFile}");
            await DatabaseModel.Connector.CallQuery(CleanupFile);

            if (consoleOutput)
                WriteResultToConsole();
            if (saveResult)
                SaveResult();


            return Results;
        }

        private async Task<List<TestCase>> RunQueriesSerial()
        {
            var testCases = new List<TestCase>();
            foreach (FileInfo queryFile in CaseFiles)
            {
                Console.WriteLine($"Running {queryFile}");
                AnalysisResult analysisResult = DatabaseModel.Parser.ParsePlan(await DatabaseModel.Connector.AnalyseQuery(queryFile));
                TestCase testCase = new TestCase(queryFile, analysisResult);
                testCases.Add(testCase);
            }
            return testCases;
        }

        private void WriteResultToConsole()
        {
            foreach(var testCase in Results)
                Console.WriteLine($"{DatabaseModel.Name} | {testCase.Category} | {testCase.Name} | Database predicted cardinality: [{(testCase.AnalysisResult.EstimatedCardinality)}], actual: [{testCase.AnalysisResult.ActualCardinality}]");
        }

        private void SaveResult()
        {
            string directory = $"Results\\{TimeStamp.ToString("yyyy/MM/dd/HH.mm.ss")}";
            string resultFileName = directory + "\\result.csv";
            Directory.CreateDirectory(directory);
            FileStream stream;
            CsvConfiguration config;
            if (File.Exists(resultFileName))
            {
                stream = File.Open(resultFileName, FileMode.Append);
                config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = false,
                };

            } else
            {
                stream = File.Open(resultFileName, FileMode.Create);
                config = new CsvConfiguration(CultureInfo.InvariantCulture);
            }
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(Results);
                writer.Flush();
            }

        }

    }
}
