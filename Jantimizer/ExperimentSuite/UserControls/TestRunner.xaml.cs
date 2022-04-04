﻿using ExperimentSuite.Models;
using QueryOptimiser.Models;
using QueryParser.Models;
using QueryPlanParser.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tools.Helpers;
using Tools.Services;

namespace ExperimentSuite.UserControls
{
    /// <summary>
    /// Interaction logic for TestRunner.xaml
    /// </summary>
    public partial class TestRunner : UserControl
    {
        private int collapesedHeight = 30;

        public string RunnerName { get; }
        public SuiteData RunData { get; }
        public FileInfo SettingsFile { get; private set; }
        public FileInfo SetupFile { get; private set; }
        public FileInfo CleanupFile { get; private set; }
        public IEnumerable<FileInfo> CaseFiles { get; private set; }
        public List<TestReport> Results { get; private set; }
        private CSVWriter csvWriter;

        public TestRunner(string name, SuiteData runData, FileInfo settingsFile, FileInfo setupFile, FileInfo cleanupFile, IEnumerable<FileInfo> caseFiles, DateTime timeStamp)
        {
            RunnerName = name;
            RunData = runData;
            SettingsFile = settingsFile;
            SetupFile = setupFile;
            CleanupFile = cleanupFile;
            CaseFiles = caseFiles;
            Results = new List<TestReport>();
            csvWriter = new CSVWriter($"Results/{timeStamp.ToString("yyyy-MM-dd HH.mm.ss")}", $"{RunData.Name}-{RunnerName}.csv");
            InitializeComponent();

            RunnerGrid.Height = collapesedHeight;
            TestNameLabel.Content = RunnerName;
        }

        public async Task<List<TestReport>> Run(bool consoleOutput = true, bool saveResult = true)
        {
            TestNameLabel.Foreground = Brushes.Yellow;
            RunnerGrid.Height = double.NaN;

            PrintTestUpdate("Parsing settings file:", SettingsFile.Name);
            ParseTestSettings(SettingsFile);

            if (RunData.Settings.DoPreCleanup != null && (bool)RunData.Settings.DoPreCleanup)
            {
                PrintTestUpdate("Running Pre-Cleanup", CleanupFile.Name);
                await RunData.Connector.CallQueryAsync(CleanupFile);
            }

            if (RunData.Settings.DoSetup != null && (bool)RunData.Settings.DoSetup)
            {
                PrintTestUpdate("Running Setup", SetupFile.Name);
                await RunData.Connector.CallQueryAsync(SetupFile);
            }

            if (RunData.Settings.DoMakeHistograms != null && (bool)RunData.Settings.DoMakeHistograms)
            {
                PrintTestUpdate("Generating Histograms for:", RunData.Name);
                await HistogramControl.GenerateHistograms(RunData.HistoManager);
            }

            if (RunData.Settings.DoRunTests != null && (bool)RunData.Settings.DoRunTests)
            {
                PrintTestUpdate("Begining Test Run for:", RunData.Name);
                Results = await RunQueriesSerial();
            }

            if (RunData.Settings.DoPostCleanup != null && (bool)RunData.Settings.DoPostCleanup)
            {
                PrintTestUpdate("Running Post-Cleanup", CleanupFile.Name);
                await RunData.Connector.CallQueryAsync(CleanupFile);
            }

            if (RunData.Settings.DoMakeReport != null && (bool)RunData.Settings.DoMakeReport)
            {
                PrintTestUpdate("Making Report", RunData.Name);
                if (consoleOutput)
                {
                    var newMaker = new ReportMaker(Results);
                    ReportPanel.Children.Add(newMaker);

                }
                if (saveResult)
                    SaveResult();
            }

            PrintTestUpdate("Tests finished for:", RunData.Name);

            RunnerGrid.Height = collapesedHeight;
            TestNameLabel.Foreground = Brushes.Green;
            return Results;
        }

        private async Task<List<TestReport>> RunQueriesSerial()
        {
            var testCases = new List<TestReport>();
            SQLFileControl.SQLProgressBar.Maximum = CaseFiles.Count();
            foreach (var queryFile in CaseFiles)
            {
                try
                {
                    await Task.Delay(1);
                    SQLFileControl.UpdateFileLabel(queryFile.Name);
                    SQLFileControl.SQLProgressBar.Value++;
                    DataSet dbResult = await RunData.Connector.AnalyseExplainQueryAsync(queryFile);
                    AnalysisResult analysisResult = RunData.Parser.ParsePlan(dbResult);

                    List<INode> nodes = await RunData.QueryParserManager.ParseQueryAsync(File.ReadAllText(queryFile.FullName), false);
                    OptimiserResult jantimiserResult = RunData.Optimiser.OptimiseQuery(nodes);

                    TestReport testCase = new TestReport(RunData.Name, queryFile.Name, RunnerName, analysisResult.EstimatedCardinality, analysisResult.ActualCardinality, jantimiserResult.EstTotalCardinality);
                    testCases.Add(testCase);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error! The query file [{queryFile}] failed with the following error: {ex.ToString()}");
                }
            }
            SQLFileControl.SQLProgressBar.Value = SQLFileControl.SQLProgressBar.Maximum;
            return testCases;
        }

        private void SaveResult()
        {
            csvWriter.Write<TestReport, TestReportMap>(Results, true);
        }

        private void ParseTestSettings(FileInfo file)
        {
            if (!file.Exists)
                throw new IOException($"Error!, Test setting file `{file.Name}` not found!");                
            RunData.Settings.Update(JsonParsingHelper.ParseJson<TestSettings>(File.ReadAllText(file.FullName)));
        }

        private void PrintTestUpdate(string left, string right)
        {
            StatusTextBox.Text += left + Environment.NewLine;
            FileStatusTextBox.Text += right + Environment.NewLine;
        }

        private void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            if (RunnerGrid.Height == collapesedHeight)
                RunnerGrid.Height = double.NaN;
            else
                RunnerGrid.Height = collapesedHeight;
        }

        private void Textbox_Autoscroll_ToBottom(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }
    }
}
