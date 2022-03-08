﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryEditorTool.Models
{
    public class SELECTStmt : IStmt, IValue
    {
        public INode? Parent { get; set; }
        public INode? Child { get; set; }
        public INode? Value { get; set; }

        public SELECTStmt(INode? parent, INode? child, INode? value)
        {
            Parent = parent;
            Child = child;
            Value = value;
        }

        public override string? ToString()
        {
            return $"SELECT {Value} FROM {Child}";
        }
    }
}
