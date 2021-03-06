﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Views
{
    using System;
    using System.Collections.Generic;
    
    #line 1 "..\..\Signum\Views\NormalControl.cshtml"
    using System.Configuration;
    
    #line default
    #line hidden
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    
    #line 2 "..\..\Signum\Views\NormalControl.cshtml"
    using Signum.Engine.Operations;
    
    #line default
    #line hidden
    using Signum.Entities;
    using Signum.Utilities;
    using Signum.Web;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.5.4.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Signum/Views/NormalControl.cshtml")]
    public partial class NormalControl : System.Web.Mvc.WebViewPage<TypeContext>
    {
        public NormalControl()
        {
        }
        public override void Execute()
        {




            
            #line 4 "..\..\Signum\Views\NormalControl.cshtml"
  
    ModifiableEntity modifiable = Model.UntypedValue as ModifiableEntity;
    string partialViewName = ViewData[ViewDataKeys.PartialViewName].ToString();
               

            
            #line default
            #line hidden
WriteLiteral("<div>\r\n    ");


            
            #line 9 "..\..\Signum\Views\NormalControl.cshtml"
Write(Html.AntiForgeryToken());

            
            #line default
            #line hidden
WriteLiteral("\r\n    ");


            
            #line 10 "..\..\Signum\Views\NormalControl.cshtml"
Write(Html.Hidden(ViewDataKeys.TabId, ViewData[ViewDataKeys.TabId]));

            
            #line default
            #line hidden
WriteLiteral("\r\n    ");


            
            #line 11 "..\..\Signum\Views\NormalControl.cshtml"
Write(Html.Hidden(ViewDataKeys.PartialViewName, ViewData[ViewDataKeys.PartialViewName]));

            
            #line default
            #line hidden
WriteLiteral("\r\n    <div class=\"ui-widget-header ui-corner-all sf-normal-control-header\">\r\n    " +
"    <span class=\"sf-type-nice-name\">");


            
            #line 13 "..\..\Signum\Views\NormalControl.cshtml"
                                   Write(Navigator.Manager.GetTypeTitle(modifiable));

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n        ");


            
            #line 14 "..\..\Signum\Views\NormalControl.cshtml"
   Write(Html.RenderWidgetsForEntity(modifiable, partialViewName, Model.ControlID));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n    <div class=\"clearall\"></div>\r\n");


            
            #line 17 "..\..\Signum\Views\NormalControl.cshtml"
     if(string.IsNullOrEmpty(ViewBag.Title))
    {
        ViewBag.Title = modifiable.TryToString();
    }

            
            #line default
            #line hidden
WriteLiteral("    <span class=\"sf-entity-title\">");


            
            #line 21 "..\..\Signum\Views\NormalControl.cshtml"
                              Write(ViewBag.Title);

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n</div>\r\n<div class=\"sf-button-bar\">\r\n    ");


            
            #line 24 "..\..\Signum\Views\NormalControl.cshtml"
Write(ButtonBarEntityHelper.GetForEntity(new EntityButtonContext
    { 
        ViewButtons = ViewButtons.Save,
        ShowOperations = (bool)ViewData[ViewDataKeys.ShowOperations],
        ControllerContext = this.ViewContext,
        PartialViewName = ViewData[ViewDataKeys.PartialViewName].ToString(),
        Prefix =  Model.ControlID
    },  (ModifiableEntity)Model.UntypedValue).ToString(Html));

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n\r\n<div class=\"clearall\"></div>\r\n<div class=\"validationSummaryAjax\">\r\n  " +
"  ");


            
            #line 36 "..\..\Signum\Views\NormalControl.cshtml"
Write(Html.ValidationSummaryAjax());

            
            #line default
            #line hidden
WriteLiteral("\r\n    ");


            
            #line 37 "..\..\Signum\Views\NormalControl.cshtml"
Write(Html.NormalPageHeader());

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n<div id=\"divMainControl\" class=\"sf-main-control\" data-prefix=\"");


            
            #line 39 "..\..\Signum\Views\NormalControl.cshtml"
                                                         Write(Model.ControlID);

            
            #line default
            #line hidden
WriteLiteral("\">\r\n");


            
            #line 40 "..\..\Signum\Views\NormalControl.cshtml"
       Html.RenderPartial(partialViewName, Model);

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");


        }
    }
}
#pragma warning restore 1591
