﻿using DatabaseConnector;
using QueryTestSuite.Models;

namespace QueryTestSuite.TestRunners
{
    internal class TestSuite
    {
        public IEnumerable<SuiteData> DatabaseModels { get; }
        private DateTime TimeStamp;

        public TestSuite(IEnumerable<SuiteData> databaseModels, DateTime timeStamp)
        {
            DatabaseModels = databaseModels;
            TimeStamp = timeStamp;
        }

        public async Task RunTests(DirectoryInfo dir)
        {
            var testRunners = new List<TestRunner>();
            var testRuns = new List<Task<List<TestCaseResult>>>();

            foreach(SuiteData databaseModel in DatabaseModels)
            {
                var caseDir = new DirectoryInfo(Path.Join(dir.FullName, "Cases/"));

                TestRunner runner = new TestRunner(
                    databaseModel,
                    GetVariant(dir, "setup", databaseModel.Name),
                    GetVariant(dir, "cleanup", databaseModel.Name),
                    GetInvariantsInDir(caseDir).Select(invariant => GetVariant(caseDir, invariant, databaseModel.Name)),
                    TimeStamp
                );
                testRunners.Add(runner);
                testRuns.Add(runner.Run(true));
            }

            await Task.WhenAll(testRuns);
        }

        private FileInfo GetVariant(DirectoryInfo dir, string name, string type)
        {
            var specific = new FileInfo(Path.Combine(dir.FullName, $"{name}.{type}.sql"));

            if(specific.Exists)
                return specific;

            var generic = new FileInfo(Path.Combine(dir.FullName, $"{name}.sql"));

            if(generic.Exists)
                return generic;

            throw new FileNotFoundException($"Could not find '{name}' of type '{type}' in '{dir.FullName}'");
        }

        List<string> GetInvariantsInDir(DirectoryInfo dir)
        {
            // Every filename, until first '.', unique
            return dir.GetFiles()
                .Select(x => x.Name.Split('.')[0])
                .GroupBy(x => x)
                .Select(x => x.First())
                .ToList();
        }
    }
}
