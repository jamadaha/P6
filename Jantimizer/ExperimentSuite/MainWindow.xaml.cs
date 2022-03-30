﻿using ExperimentSuite.Models;
using ExperimentSuite.Models.ExperimentParsing;
using ExperimentSuite.SuiteDatas;
using ExperimentSuite.UserControls;
using System;
using System.Collections.Generic;
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

namespace ExperimentSuite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WriteToStatus("Setting up suite datas...");
            var pgDataDefault = SuiteDataSets.GetPostgresSD_Default();
            var pgDataEquiDepth = SuiteDataSets.GetPostgresSD_EquiDepth();
            var pgDataEquiDepthVariance = SuiteDataSets.GetPostgresSD_EquiDepthVariance();
            var myDataDefault = SuiteDataSets.GetMySQLSD_Default(pgDataDefault.QueryParserManager.QueryParsers[0]);
            var myDataEquiDepth = SuiteDataSets.GetMySQLSD_EquiDepth(pgDataEquiDepth.QueryParserManager.QueryParsers[0]);
            var myDataEquiDepthVariance = SuiteDataSets.GetMySQLSD_EquiDepthVariance(pgDataEquiDepthVariance.QueryParserManager.QueryParsers[0]);
            var connectorSet = new List<SuiteData>() { pgDataDefault, myDataDefault, pgDataEquiDepth, myDataEquiDepth, pgDataEquiDepthVariance, myDataEquiDepthVariance };

            DateTime runTime = DateTime.UtcNow;

            WriteToStatus("Parsing experiment list...");
            var experimentsFile = IOHelper.GetFile("../../../experiments.json");
            var testsPath = IOHelper.GetDirectory("../../../Tests");
            var res = JsonSerializer.Deserialize(File.ReadAllText(experimentsFile.FullName), typeof(ExperimentList));
            if (res is ExperimentList expList)
            {
                foreach (var experiment in expList.Experiments)
                {
                    if (experiment.RunExperiment)
                    {
                        TestsPanel.Children.Add(getSeperator(experiment.ExperimentName));

                        WriteToStatus($"Running experiment {experiment.ExperimentName}");
                        await RunExperimentQueue(
                            GetRunDataFromList(experiment.PreRunData, connectorSet, testsPath, runTime),
                            experiment.RunParallel);
                        await RunExperimentQueue(
                            GetRunDataFromList(experiment.RunData, connectorSet, testsPath, runTime),
                            experiment.RunParallel);
                    }
                    WriteToStatus($"Experiment {experiment.ExperimentName} finished!");
                }
            }
            WriteToStatus("All experiments complete!");
        }

        private void WriteToStatus(string text)
        {
            StatusTextbox.Text += $"{text}{Environment.NewLine}";
        }

        private Dictionary<string, List<Func<Task>>> GetRunDataFromList(List<TestRunData> runData, List<SuiteData> connectorSet, DirectoryInfo bastTestPath, DateTime timestamp)
        {
            Dictionary<string, List<Func<Task>>> returnTasks = new Dictionary<string, List<Func<Task>>>();
            foreach (TestRunData data in runData)
            {
                foreach (SuiteData suitData in connectorSet)
                {
                    if (data.ConnectorName == suitData.Name && data.ConnectorID == suitData.ID)
                    {
                        foreach (string testFile in data.TestFiles)
                        {
                            var newDir = IOHelper.GetDirectory(bastTestPath, testFile);

                            IOHelper.CreateDirIfNotExist(newDir.FullName, "Cases/");
                            var caseDir = IOHelper.GetDirectory(newDir.FullName, "Cases/");

                            TestRunner runner = new TestRunner(
                                testFile,
                                suitData,
                                IOHelper.GetFileVariant(newDir, "testSettings", suitData.Name.ToLower(), "json"),
                                IOHelper.GetFileVariantOrNone(newDir, "setup", suitData.Name.ToLower(), "sql"),
                                IOHelper.GetFileVariantOrNone(newDir, "cleanup", suitData.Name.ToLower(), "sql"),
                                IOHelper.GetInvariantsInDir(caseDir).Select(invariant => IOHelper.GetFileVariant(caseDir, invariant, suitData.Name.ToLower(), "sql")),
                                timestamp
                            );
                            TestsPanel.Children.Add(runner);

                            Func<Task> runFunc = async () => await runner.Run(true);

                            if (returnTasks.ContainsKey(testFile))
                                returnTasks[testFile].Add(runFunc);
                            else
                                returnTasks.Add(testFile, new List<Func<Task>>() { runFunc });
                        }
                    }
                }
            }
            return returnTasks;
        }

        private async Task RunExperimentQueue(Dictionary<string, List<Func<Task>>> dict, bool runParallel = true)
        {
            if (runParallel)
            {
                foreach (string key in dict.Keys)
                {
                    List<Task> results = new List<Task>();
                    foreach (Func<Task> funcs in dict[key])
                    {
                        results.Add(funcs.Invoke());
                    }
                    await Task.WhenAll(results.ToArray());
                }
            }
            else
                foreach (string key in dict.Keys)
                    foreach (Func<Task> funcs in dict[key])
                        await funcs.Invoke();
        }

        private void StatusTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }

        private Label getSeperator(string text)
        {
            var newLabel = new Label();
            newLabel.Content = text;
            newLabel.FontSize = 20;
            return newLabel;
        }
    }
}