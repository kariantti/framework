﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Signum.Entities;

namespace Signum.Web.Lines
{
    public class HourMinuteLine : BaseLine
    {
        public readonly RouteValueDictionary ValueHtmlProps = new RouteValueDictionary();

        public bool WriteHiddenOnReadonly { get; set; }

        public HourMinuteLine(Type type, object untypedValue, Context parent, string controlID, PropertyRoute propertyRoute)
            : base(type, untypedValue, parent, controlID, propertyRoute)
        {
        }

        protected override void SetReadOnly()
        {
        }
    }
}