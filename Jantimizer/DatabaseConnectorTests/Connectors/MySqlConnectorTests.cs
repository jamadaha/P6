﻿using DatabaseConnector;
using DatabaseConnector.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.Models;

namespace DatabaseConnectorTests.Connectors
{
    [TestClass]
    public class MySqlConnectorTests
    {
        #region Constructor

        [TestMethod]
        [DataRow("connnnn","abc",1234,"aa","bb","cc","dd")]
        public void Can_Set_Constructor(string connectionString, string server, int port, string username, string password, string database, string schema)
        {
            // ARRANGE
            var properties = new ConnectionProperties(new SecretsItem(username, password, server, port), database, schema);

            // ACT
            IDbConnector connector = new DatabaseConnector.Connectors.MySqlConnector(properties);

            // ASSERT
            Assert.AreEqual(properties, connector.ConnectionProperties);
        }

        #endregion

        #region CheckConnection

        [TestMethod]
        public void Cant_CheckConnection_IfNotPossible()
        {
            // ARRANGE
            var properties = new ConnectionProperties(new SecretsItem("a", "b", "-1", -1), "-1", "-1");
            IDbConnector connector = new DatabaseConnector.Connectors.MySqlConnector(properties);

            // ACT
            var result = connector.CheckConnection();

            // ASSERT
            Assert.IsFalse(result);
        }

        #endregion

        #region CallQuery

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Cant_CallQuery_IfConnectionStringIncorrect()
        {
            // ARRANGE
            var properties = new ConnectionProperties(new SecretsItem("a","b", "-1", -1),"-1","-1");
            IDbConnector connector = new DatabaseConnector.Connectors.MySqlConnector(properties);

            // ACT
            try
            {
                var result = await connector.CallQueryAsync("abc");
            }
            catch(DatabaseConnectorErrorLogException ex)
            {
                throw ex.ActualException;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "MySQL timed out too many times with the SSL error!")]
        public async Task Cant_CallQuery_IfServerNotExist()
        {
            // ARRANGE
            var properties = new ConnectionProperties(new SecretsItem("a", "b", "nonexistinghost", 3306), "aaa", "bbb");
            DatabaseConnector.Connectors.MySqlConnector connector = new DatabaseConnector.Connectors.MySqlConnector(properties);
            connector._timeoutMs = 1;
            connector._maxTimeout = 1;

            // ACT
            try
            {
                var result = await connector.CallQueryAsync("abc");
            }
            catch (DatabaseConnectorErrorLogException ex)
            {
                throw ex.ActualException;
            }
        }

        #endregion
    }
}
