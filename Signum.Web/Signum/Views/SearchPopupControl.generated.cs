﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18010
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Views
{
    using System;
    using System.Collections.Generic;
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
    using Signum.Entities;
    using Signum.Utilities;
    using Signum.Web;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.5.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Signum/Views/SearchPopupControl.cshtml")]
    public class SearchPopupControl : System.Web.Mvc.WebViewPage<Context>
    {
        public SearchPopupControl()
        {
        }
        public override void Execute()
        {


            
            #line 2 "..\..\Signum\Views\SearchPopupControl.cshtml"
    FindOptions findOptions = (FindOptions)ViewData[ViewDataKeys.FindOptions];

            
            #line default
            #line hidden
WriteLiteral("<div id=\"");


            
            #line 3 "..\..\Signum\Views\SearchPopupControl.cshtml"
    Write(Model.Compose("panelPopup"));

            
            #line default
            #line hidden
WriteLiteral("\">\r\n    <span class=\"sf-popup-title\">\r\n        <span style=\"float:left; display:b" +
"lock\">");


            
            #line 5 "..\..\Signum\Views\SearchPopupControl.cshtml"
                                           Write(ViewBag.Title);

            
            #line default
            #line hidden
WriteLiteral("</span>\r\n");


            
            #line 6 "..\..\Signum\Views\SearchPopupControl.cshtml"
          

            
            #line default
            #line hidden
WriteLiteral("            <a id=\"");


            
            #line 7 "..\..\Signum\Views\SearchPopupControl.cshtml"
              Write(Model.Compose("sfFullScreen"));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"sf-popup-fullscreen\">\r\n                <span class=\"ui-icon ui-icon-extl" +
"ink\">fullscreen</span>\r\n            </a>\r\n");


            
            #line 10 "..\..\Signum\Views\SearchPopupControl.cshtml"
        

            
            #line default
            #line hidden
WriteLiteral("    </span>\r\n    <div class=\"sf-query-button-bar\">\r\n        <input type=\"button\" " +
"id=\"");


            
            #line 13 "..\..\Signum\Views\SearchPopupControl.cshtml"
                            Write(Model.Compose("btnOk"));

            
            #line default
            #line hidden
WriteLiteral("\" class=\"sf-query-button sf-ok-button\" value=\"OK\" ");


            
            #line 13 "..\..\Signum\Views\SearchPopupControl.cshtml"
                                                                                                      Write(ViewData[ViewDataKeys.OnOk] != null ? Html.Raw("onclick=\"" + ViewData[ViewDataKeys.OnOk] + "\"") : null);

            
            #line default
            #line hidden
WriteLiteral(" />\r\n    </div>\r\n    <div class=\"sf-popup-body\">\r\n");


            
            #line 16 "..\..\Signum\Views\SearchPopupControl.cshtml"
           
            ViewData[ViewDataKeys.InPopup] = true;
            Html.RenderPartial(ViewData[ViewDataKeys.PartialViewName].ToString(), Model);
        

            
            #line default
            #line hidden
WriteLiteral("        ");


            
            #line 20 "..\..\Signum\Views\SearchPopupControl.cshtml"
   Write(Html.ValidationSummaryAjax(Model));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </div>\r\n</div>\r\n");


        }
    }
}
#pragma warning restore 1591
