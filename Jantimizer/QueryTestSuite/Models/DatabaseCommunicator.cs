﻿using QueryTestSuite.Connectors;
using QueryTestSuite.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryTestSuite.Models
{
    public class DatabaseCommunicator
    {
        public string Name { get; set; }
        public IDbConnector Connector { get; set; }
        public IPlanParser Parser { get; set; }

        public DatabaseCommunicator(string name, IDbConnector connector, IPlanParser parser)
        {
            Name = name;
            Connector = connector;
            Parser = parser;
        }
    }
}
