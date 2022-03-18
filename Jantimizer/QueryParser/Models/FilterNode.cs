﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryParser.Models
{
    public class FilterNode
    {
        public TableReferenceNode TableReference { get; set; }
        public string AttributeName { get; set; }
        public ComparisonType.Type ComType { get; set; }
        public string Constant { get; set; }

        public FilterNode(TableReferenceNode tableReference, string attributeName, ComparisonType.Type comType, string constant)
        {
            TableReference = tableReference;
            AttributeName = attributeName;
            ComType = comType;
            Constant = constant;
        }
    }
}
