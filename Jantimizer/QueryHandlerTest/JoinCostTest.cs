using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using QueryOptimiser;
using Histograms;
using Histograms.Managers;
using QueryParser;
using QueryParser.QueryParsers;
using QueryParser.Models;
using QueryOptimiserTest;

namespace CostTest;

[TestClass]
public class JoinCostTest
{
    #region Equality

    [TestMethod]
    [DataRow(10, 100, 100, 10000, 10)]
    [DataRow(20, 100, 100, 10000, 10)]
    [DataRow(10, 10, 100, 1000, 10)]
    [DataRow(10, 100, 100, 10000, 20)]
    [DataRow(100, 1, 1, 1, 10)]
    public void EqualitySameValue(int value, int aAmount, int bAmount, int expectedHits, int depth)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateConstHistogram("A", "ID", depth, aAmount, value);
        var bGram = Utilities.CreateConstHistogram("B", "ID", depth, bAmount, value);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>(){new JoinQueryParser()});
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID = B.ID) JOIN C ON B.ID = C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    
    [TestMethod]
    [DataRow(0, 100, 0, 100, 10, 10000)]
    [DataRow(0, 100, 99, 199, 10, 100)]
    [DataRow(0, 100, 101, 201, 10, 0)]
    [DataRow(0, 100, 50, 150, 10, 50*50)]
    [DataRow(50, 150, 0, 100, 10, 50*50)]
    [DataRow(0, 100, 50, 150, 20, 60*60)]
    public void EqualityOverlap(int aMin, int aMax, int bMin, int bMax, int depth, int expectedHits) {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateIncreasingHistogram("A", "ID", depth, aMin, aMax);
        var bGram = Utilities.CreateIncreasingHistogram("B", "ID", depth, bMin, bMax);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>(){new JoinQueryParser()});
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID = B.ID) JOIN C ON B.ID = C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    #endregion
    #region Less
    [TestMethod]
    [DataRow(10, 20, 100, 100, 10, 100*100)]
    [DataRow(20, 20, 100, 100, 10, 0)]
    [DataRow(30, 20, 100, 100, 10, 0)]
    [DataRow(10, 20, 50, 100, 10, 50*100)]
    [DataRow(20, 20, 50, 100, 10, 0)]
    [DataRow(30, 20, 50, 100, 10, 0)]
    [DataRow(10, 20, 100, 50, 10, 50 * 100)]
    [DataRow(20, 20, 100, 50, 10, 0)]
    [DataRow(30, 20, 100, 50, 10, 0)]
    [DataRow(10, 20, 100, 100, 20, 100 * 100)]
    [DataRow(20, 20, 100, 100, 20, 0)]
    [DataRow(30, 20, 100, 100, 20, 0)]
    public void LessConstantValue(int aValue, int bValue, int aAmount, int bAmount, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateConstHistogram("A", "ID", depth, aAmount, aValue);
        var bGram = Utilities.CreateConstHistogram("B", "ID", depth, bAmount, bValue);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID < B.ID) JOIN C ON B.ID < C.ID");
        
        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    [TestMethod]
    [DataRow(0, 100, 0, 100, 10, 100*100)]
    [DataRow(0, 100, 100, 200, 10, 100*100)]
    [DataRow(100, 200, 0, 100, 10, 0)]
    public void LessIncreasingValue(int aMin, int aMax, int bMin, int bMax, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateIncreasingHistogram("A", "ID", depth, aMin, aMax);
        var bGram = Utilities.CreateIncreasingHistogram("B", "ID", depth, bMin, bMax);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID < B.ID) JOIN C ON B.ID < C.ID");
        
        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    #endregion
    #region More
    [TestMethod]
    [DataRow(10, 20, 100, 100, 10, 0)]
    [DataRow(20, 20, 100, 100, 10, 0)]
    [DataRow(30, 20, 100, 100, 10, 100 * 100)]
    [DataRow(10, 20, 50, 100, 10, 0)]
    [DataRow(20, 20, 50, 100, 10, 0)]
    [DataRow(30, 20, 50, 100, 10, 50 * 100)]
    [DataRow(10, 20, 100, 50, 10, 0)]
    [DataRow(20, 20, 100, 50, 10, 0)]
    [DataRow(30, 20, 100, 50, 10, 50 * 100)]
    [DataRow(10, 20, 100, 100, 20, 0)]
    [DataRow(20, 20, 100, 100, 20, 0)]
    [DataRow(30, 20, 100, 100, 20, 100 * 100)]
    public void MoreConstantValue(int aValue, int bValue, int aAmount, int bAmount, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateConstHistogram("A", "ID", depth, aAmount, aValue);
        var bGram = Utilities.CreateConstHistogram("B", "ID", depth, bAmount, bValue);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID > B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    [TestMethod]
    [DataRow(0, 100, 0, 100, 10, 100 * 100)]
    [DataRow(0, 100, 100, 200, 10, 0)]
    [DataRow(100, 200, 0, 100, 10, 100 * 100)]
    public void MoreIncreasingValue(int aMin, int aMax, int bMin, int bMax, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateIncreasingHistogram("A", "ID", depth, aMin, aMax);
        var bGram = Utilities.CreateIncreasingHistogram("B", "ID", depth, bMin, bMax);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID > B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    #endregion
    #region EqualOrLess
    [TestMethod]
    [DataRow(10, 20, 100, 100, 10, 100 * 100)]
    [DataRow(20, 20, 100, 100, 10, 100 * 100)]
    [DataRow(30, 20, 100, 100, 10, 0)]
    [DataRow(10, 20, 50, 100, 10, 50 * 100)]
    [DataRow(20, 20, 50, 100, 10, 50 * 100)]
    [DataRow(30, 20, 50, 100, 10, 0)]
    [DataRow(10, 20, 100, 50, 10, 50 * 100)]
    [DataRow(20, 20, 100, 50, 10, 50 * 100)]
    [DataRow(30, 20, 100, 50, 10, 0)]
    [DataRow(10, 20, 100, 100, 20, 100 * 100)]
    [DataRow(20, 20, 100, 100, 20, 100 * 100)]
    [DataRow(30, 20, 100, 100, 20, 0)]
    public void EqualOrLessConstantValue(int aValue, int bValue, int aAmount, int bAmount, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateConstHistogram("A", "ID", depth, aAmount, aValue);
        var bGram = Utilities.CreateConstHistogram("B", "ID", depth, bAmount, bValue);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID <= B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    [TestMethod]
    [DataRow(0, 100, 0, 100, 10, 100 * 100)]
    [DataRow(0, 100, 100, 200, 10, 100 * 100)]
    [DataRow(100, 200, 0, 100, 10, 0)]
    public void EqualOrLessIncreasingValue(int aMin, int aMax, int bMin, int bMax, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateIncreasingHistogram("A", "ID", depth, aMin, aMax);
        var bGram = Utilities.CreateIncreasingHistogram("B", "ID", depth, bMin, bMax);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID <= B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    #endregion
    #region EqualOrMore
    [TestMethod]
    [DataRow(10, 20, 100, 100, 10, 0)]
    [DataRow(20, 20, 100, 100, 10, 100 * 100)]
    [DataRow(30, 20, 100, 100, 10, 100 * 100)]
    [DataRow(10, 20, 50, 100, 10, 0)]
    [DataRow(20, 20, 50, 100, 10, 50 * 100)]
    [DataRow(30, 20, 50, 100, 10, 50 * 100)]
    [DataRow(10, 20, 100, 50, 10, 0)]
    [DataRow(20, 20, 100, 50, 10, 50 * 100)]
    [DataRow(30, 20, 100, 50, 10, 50 * 100)]
    [DataRow(10, 20, 100, 100, 20, 0)]
    [DataRow(20, 20, 100, 100, 20, 100 * 100)]
    [DataRow(30, 20, 100, 100, 20, 100 * 100)]
    public void EqualOrMoreConstantValue(int aValue, int bValue, int aAmount, int bAmount, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateConstHistogram("A", "ID", depth, aAmount, aValue);
        var bGram = Utilities.CreateConstHistogram("B", "ID", depth, bAmount, bValue);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID >= B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    [TestMethod]
    [DataRow(0, 100, 0, 100, 10, 100 * 100)]
    [DataRow(0, 100, 100, 200, 10, 0)]
    [DataRow(100, 200, 0, 100, 10, 100 * 100)]
    public void EqualOrMoreIncreasingValue(int aMin, int aMax, int bMin, int bMax, int depth, int expectedHits)
    {
        var histogramManager = new Histograms.Managers.PostgresEquiDepthHistogramManager("SomeConnectionString", depth);
        var aGram = Utilities.CreateIncreasingHistogram("A", "ID", depth, aMin, aMax);
        var bGram = Utilities.CreateIncreasingHistogram("B", "ID", depth, bMin, bMax);
        histogramManager.AddHistogram(aGram);
        histogramManager.AddHistogram(bGram);

        QueryParser.ParserManager PM = new ParserManager(new List<IQueryParser>() { new JoinQueryParser() });
        var nodes = PM.ParseQuery("SELECT * FROM (A JOIN B ON A.ID >= B.ID) JOIN C ON B.ID < C.ID");

        int cost = QueryOptimiser.JoinCost.CalculateJoinCost((JoinNode) nodes[0], histogramManager);

        Assert.AreEqual(expectedHits, cost);
    }
    #endregion
}