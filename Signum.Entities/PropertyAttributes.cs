﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signum.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class QueryablePropertyAttribute : Attribute
    {
        public bool AvailableForQueries { get; set; }

        public QueryablePropertyAttribute(bool availableForQueries)
        {
            this.AvailableForQueries = availableForQueries;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class HiddenPropertyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UnitAttribute : Attribute
    {
        public string UnitName { get; private set; }
        public UnitAttribute(string unitName)
        {
            this.UnitName = unitName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FormatAttribute : Attribute
    {
        public string Format { get; private set; }
        public FormatAttribute(string format)
        {
            this.Format = format;
        }
    }
}
