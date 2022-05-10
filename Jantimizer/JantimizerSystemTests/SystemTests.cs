using DatabaseConnector;
using DatabaseConnector.Connectors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Milestoner;
using Milestoner.DataGatherers;
using Milestoner.DepthCalculators;
using Milestoner.Milestoners;
using QueryEstimator;
using QueryEstimator.Models;
using QueryPlanParser;
using QueryPlanParser.Models;
using QueryPlanParser.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;
using Tools.Models.JsonModels;

namespace JantimizerSystemTests
{
    [TestClass]
    public class SystemTests
    {
        // We should expect 100% accuracte results with single joins
        [TestMethod]
        [DataRow("../../../Cases/B_J1-G1_F0_a.json", "../../../Setups/constant.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F0_b.json", "../../../Setups/constant.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F1-L1.json", "../../../Setups/constant.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_a.json", "../../../Setups/constant.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_b.json", "../../../Setups/constant.setup.posgresql.sql")]
        public async Task Constant_MinDepth_Constant_Tests(string caseFileName, string setupFileName)
        {
            // ARRANGE
            var queryFile = new JsonQuery(File.ReadAllText(caseFileName));
            var setupFileTest = File.ReadAllText(setupFileName);

            var properties = new ConnectionProperties(
                new SecretsItem("postgres","password", "localhost", 5432),
                "postgres",
                "systemtests_constant");
            IDbConnector connector = new PostgreSqlConnector(properties);
            IPlanParser parser = new PostgreSqlParser();
            IDataGatherer dataGatherer = new PostgresDataGatherer(properties);
            IDepthCalculator depthCalculator = new ConstantDepth(1);
            IMilestoner milestoner = new MinDepthMilestoner(dataGatherer, depthCalculator);
            IQueryEstimator<JsonQuery> estimator = new JsonQueryEstimator(milestoner, 10);

            if (!await IsPostgresRunning(connector))
                Assert.Inconclusive("Postgres is not running! Ignore this if its run from github actions.");

            await connector.CallQueryAsync(setupFileTest);

            DataSet dbResult = await connector.AnalyseExplainQueryAsync(queryFile.EquivalentSQLQuery);
            AnalysisResult analysisResult = parser.ParsePlan(dbResult);

            await GenerateMilestones(milestoner);
            await BindMilestones(milestoner);

            // ACT
            EstimatorResult jantimatorResult = estimator.GetQueryEstimation(queryFile);

            // ASSERT
            Assert.AreEqual(analysisResult.ActualCardinality, jantimatorResult.EstimatedCardinality);
        }

        // We should expect 100% accuracte results with single joins
        [TestMethod]
        [DataRow("../../../Cases/B_J1-G1_F0_a.json", "../../../Setups/random.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F0_b.json", "../../../Setups/random.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F1-L1.json", "../../../Setups/random.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_a.json", "../../../Setups/random.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_b.json", "../../../Setups/random.setup.posgresql.sql")]
        public async Task Random_MinDepth_Constant_Tests(string caseFileName, string setupFileName)
        {
            // ARRANGE
            var queryFile = new JsonQuery(File.ReadAllText(caseFileName));
            var setupFileTest = File.ReadAllText(setupFileName);

            var properties = new ConnectionProperties(
                new SecretsItem("postgres", "password", "localhost", 5432),
                "postgres",
                "systemtests_random");
            IDbConnector connector = new PostgreSqlConnector(properties);
            IPlanParser parser = new PostgreSqlParser();
            IDataGatherer dataGatherer = new PostgresDataGatherer(properties);
            IDepthCalculator depthCalculator = new ConstantDepth(1);
            IMilestoner milestoner = new MinDepthMilestoner(dataGatherer, depthCalculator);
            IQueryEstimator<JsonQuery> estimator = new JsonQueryEstimator(milestoner, 10);

            if (!await IsPostgresRunning(connector))
                Assert.Inconclusive("Postgres is not running! Ignore this if its run from github actions.");

            await connector.CallQueryAsync(setupFileTest);

            DataSet dbResult = await connector.AnalyseExplainQueryAsync(queryFile.EquivalentSQLQuery);
            AnalysisResult analysisResult = parser.ParsePlan(dbResult);

            await GenerateMilestones(milestoner);
            await BindMilestones(milestoner);

            // ACT
            EstimatorResult jantimatorResult = estimator.GetQueryEstimation(queryFile);

            // ASSERT
            Assert.AreEqual(analysisResult.ActualCardinality, jantimatorResult.EstimatedCardinality);
        }

        // We should expect 100% accuracte results with single joins
        [TestMethod]
        [DataRow("../../../Cases/B_J1-G1_F0_a.json", "../../../Setups/clumped.possible.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F0_b.json", "../../../Setups/clumped.possible.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F1-L1.json", "../../../Setups/clumped.possible.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_a.json", "../../../Setups/clumped.possible.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_b.json", "../../../Setups/clumped.possible.setup.posgresql.sql")]
        public async Task Clumped_Possible_MinDepth_Constant_Tests(string caseFileName, string setupFileName)
        {
            // ARRANGE
            var queryFile = new JsonQuery(File.ReadAllText(caseFileName));
            var setupFileTest = File.ReadAllText(setupFileName);

            var properties = new ConnectionProperties(
                new SecretsItem("postgres", "password", "localhost", 5432),
                "postgres",
                "systemtests_clumped_possible");
            IDbConnector connector = new PostgreSqlConnector(properties);
            IPlanParser parser = new PostgreSqlParser();
            IDataGatherer dataGatherer = new PostgresDataGatherer(properties);
            IDepthCalculator depthCalculator = new ConstantDepth(1);
            IMilestoner milestoner = new MinDepthMilestoner(dataGatherer, depthCalculator);
            IQueryEstimator<JsonQuery> estimator = new JsonQueryEstimator(milestoner, 10);

            if (!await IsPostgresRunning(connector))
                Assert.Inconclusive("Postgres is not running! Ignore this if its run from github actions.");

            await connector.CallQueryAsync(setupFileTest);

            DataSet dbResult = await connector.AnalyseExplainQueryAsync(queryFile.EquivalentSQLQuery);
            AnalysisResult analysisResult = parser.ParsePlan(dbResult);

            await GenerateMilestones(milestoner);
            await BindMilestones(milestoner);

            // ACT
            EstimatorResult jantimatorResult = estimator.GetQueryEstimation(queryFile);

            // ASSERT
            Assert.AreEqual(analysisResult.ActualCardinality, jantimatorResult.EstimatedCardinality);
        }

        // We should expect 100% accuracte results with single joins
        [TestMethod]
        [DataRow("../../../Cases/B_J1-G1_F0_a.json", "../../../Setups/clumped.difficult.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F0_b.json", "../../../Setups/clumped.difficult.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-G1_F1-L1.json", "../../../Setups/clumped.difficult.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_a.json", "../../../Setups/clumped.difficult.setup.posgresql.sql")]
        [DataRow("../../../Cases/B_J1-L1_F0_b.json", "../../../Setups/clumped.difficult.setup.posgresql.sql")]
        public async Task Clumped_Difficult_MinDepth_Constant_Tests(string caseFileName, string setupFileName)
        {
            // ARRANGE
            var queryFile = new JsonQuery(File.ReadAllText(caseFileName));
            var setupFileTest = File.ReadAllText(setupFileName);

            var properties = new ConnectionProperties(
                new SecretsItem("postgres", "password", "localhost", 5432),
                "postgres",
                "systemtests_clumped_difficult");
            IDbConnector connector = new PostgreSqlConnector(properties);
            IPlanParser parser = new PostgreSqlParser();
            IDataGatherer dataGatherer = new PostgresDataGatherer(properties);
            IDepthCalculator depthCalculator = new ConstantDepth(1);
            IMilestoner milestoner = new MinDepthMilestoner(dataGatherer, depthCalculator);
            IQueryEstimator<JsonQuery> estimator = new JsonQueryEstimator(milestoner, 10);

            if (!await IsPostgresRunning(connector))
                Assert.Inconclusive("Postgres is not running! Ignore this if its run from github actions.");

            await connector.CallQueryAsync(setupFileTest);

            DataSet dbResult = await connector.AnalyseExplainQueryAsync(queryFile.EquivalentSQLQuery);
            AnalysisResult analysisResult = parser.ParsePlan(dbResult);

            await GenerateMilestones(milestoner);
            await BindMilestones(milestoner);

            // ACT
            EstimatorResult jantimatorResult = estimator.GetQueryEstimation(queryFile);

            // ASSERT
            Assert.AreEqual(analysisResult.ActualCardinality, jantimatorResult.EstimatedCardinality);
        }

        #region Private Test Methods

        private static bool? haveChecked;
        private async Task<bool> IsPostgresRunning(IDbConnector connector)
        {
            if (haveChecked != null)
                return (bool)haveChecked;
            haveChecked = await connector.CheckConnectionAsync();
            return (bool)haveChecked;
        }

        private async Task GenerateMilestones(IMilestoner milestoner)
        {
            milestoner.ClearMilestones();

            List<Func<Task>> milestoneTasks = await milestoner.AddMilestonesFromDBTasks();

            List<Task> results = new List<Task>();
            foreach (Func<Task> funcs in milestoneTasks)
            {
                results.Add(funcs.Invoke());
            }
            while (results.Any())
            {
                var finishedTask = await Task.WhenAny(results);
                results.Remove(finishedTask);
                await finishedTask;
            }
        }

        private async Task BindMilestones(IMilestoner milestoner)
        {
            List<Func<Task>> compareTasks = milestoner.CompareMilestonesWithDBDataTasks();

            List<Task> results = new List<Task>();
            foreach (Func<Task> funcs in compareTasks)
            {
                results.Add(funcs.Invoke());
            }
            while (results.Any())
            {
                var finishedTask = await Task.WhenAny(results);
                results.Remove(finishedTask);
                await finishedTask;
            }
        }

        #endregion
    }
}