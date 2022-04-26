﻿using ExperimentSuite.Controllers;
using ExperimentSuite.Helpers;
using ExperimentSuite.UserControls;
using ExperimentSuite.UserControls.SentinelReportViewer;
using Histograms.Caches;
using QueryPlanParser.Caches;
using ResultsSentinel;
using ResultsSentinel.SentinelLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace ExperimentSuite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string QueryPlanCacheFile = "query-plan-cache.json";

        private ExperimentController controller;

        public MainWindow()
        {
            // Sentinels
            new OptimiserResultSentinel();
            new QueryPlanParserResultSentinel();
            new QueryParserResultSentinel();
            new HistogramResultSentinel();

            // Cachers
            new QueryPlanCacher(QueryPlanCacheFile);
            new HistogramCacher();

            controller = new ExperimentController();
            controller.UpdateExperimentProgressBar += UpdateExperimentProgressBar;
            controller.AddNewElement += AddNewElementToTestPanel;
            controller.RemoveElement += RemoveElementFromTestPanel;

            InitializeComponent();
            var iconHandle = Properties.Resources.icon;
            this.Icon = ImageHelper.ByteToImage(iconHandle);
        }

        private void UpdateExperimentProgressBar(double value, double max = 0)
        {
            if (max != 0)
                ExperimentProgressBar.Maximum = max;
            ExperimentProgressBar.Value = value;
        }

        private void AddNewElementToTestPanel(UIElement element, int index = -1)
        {
            if (index == -1)
                TestsPanel.Children.Add(element);
            else
                TestsPanel.Children.Insert(index, element);
        }

        private void RemoveElementFromTestPanel(UIElement element)
        {
            TestsPanel.Children.Remove(element);
        }

        private void StatusTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            SentinelsPanel.IsEnabled = false;
            ToggelGif(true);
            ClearSentinelLogs();
            await controller.RunExperiments();
            RunButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            SentinelsPanel.IsEnabled = true;
            ToggelGif(false);
        }

        private void CacheViewerButton_Click(object sender, RoutedEventArgs e)
        {
            var cacheViewer = new CacheViewer();
            cacheViewer.Show();
        }

        private void OptimiserSentinelCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                if (OptimiserResultSentinel.Instance != null)
                    if (checkBox.IsChecked != null)
                        OptimiserResultSentinel.Instance.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void QueryPlanSentinelCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                if (QueryPlanParserResultSentinel.Instance != null)
                    if (checkBox.IsChecked != null)
                        QueryPlanParserResultSentinel.Instance.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void QuerySentinelCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                if (QueryParserResultSentinel.Instance != null)
                    if (checkBox.IsChecked != null)
                        QueryParserResultSentinel.Instance.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void HistogramSentinelCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
                if (HistogramResultSentinel.Instance != null)
                    if (checkBox.IsChecked != null)
                        HistogramResultSentinel.Instance.IsEnabled = (bool)checkBox.IsChecked;
        }

        private void SentinelViewerButton_Click(object sender, RoutedEventArgs e)
        {
            var newList = new List<IResultSentinel>();
            if (OptimiserResultSentinel.Instance != null)
                newList.Add(OptimiserResultSentinel.Instance);
            if (QueryParserResultSentinel.Instance != null)
                newList.Add(QueryParserResultSentinel.Instance);
            if (QueryPlanParserResultSentinel.Instance != null)
                newList.Add(QueryPlanParserResultSentinel.Instance);
            if (HistogramResultSentinel.Instance != null)
                newList.Add(HistogramResultSentinel.Instance);
            var sentinelWindow = new SentinelReportViewer(newList);
            sentinelWindow.Show();
        }

        private void ClearSentinelLogs()
        {
            if (OptimiserResultSentinel.Instance != null)
                OptimiserResultSentinel.Instance.ClearSentinel();
            if (QueryParserResultSentinel.Instance != null)
                QueryParserResultSentinel.Instance.ClearSentinel();
            if (QueryPlanParserResultSentinel.Instance != null)
                QueryPlanParserResultSentinel.Instance.ClearSentinel();
            if (HistogramResultSentinel.Instance != null)
                HistogramResultSentinel.Instance.ClearSentinel();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (SyncHelper.IsPaused)
                {
                    SyncHelper.IsPaused = false;
                    button.Content = "Pause";
                    ToggelGif(true);
                }
                else
                {
                    SyncHelper.IsPaused = true;
                    button.Content = "Continue";
                    ToggelGif(false);
                }
            }
        }

        private void ToggelGif(bool toState)
        {
            if (toState)
            {
                var loadingIconHandle = Properties.Resources.Loading_icon;
                ImageBehavior.SetAnimatedSource(LoadingImage, ImageHelper.ByteToBmpImage(loadingIconHandle));
            }
            else
            {
                var gifController = ImageBehavior.GetAnimationController(LoadingImage);
                gifController.Dispose();
            }
        }
    }
}
