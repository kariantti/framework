﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Net;
using System.Web;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Security;
using Signum.Utilities;
using System.Web.Routing;
using Signum.Engine.Exceptions;

namespace Signum.Web
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class SignumExceptionHandlerAttribute : HandleErrorAttribute
    {
        public static Action<HandleErrorInfo> LogException;
        public static Func<Exception, Exception> CleanException = e => e;

        public static Func<HttpContextBase, HandleErrorInfo, ActionResult> GetResult = (HttpContextBase context, HandleErrorInfo model) =>
        {
            if (context.Request.IsAjaxRequest())
            {
                return new ContentResult
                {
                    Content = model.Exception.Message
                };
            }
            else
            {
                return new ViewResult
                {
                    ViewName = NavigationManager.ViewPrefix.Formato("Error"),
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model)
                    {
                        {ViewDataKeys.Title, model.Exception.Message}
                    },
                };
            }
        };

        public static Func<Exception, int> GetHttpError = (Exception ex) =>
        {
            int error = new HttpException(null, ex).GetHttpCode();

            if (error == 401) //not authorized shows a log-in
                return 500;

            if (ex.GetType() == typeof(EntityNotFoundException))
                return 404;

            return error;
        };

        public override void OnException(ExceptionContext filterContext)
        {
            OnControllerException(filterContext);
        }

        public static Action<ExceptionContext> OnControllerException = (ExceptionContext filterContext) =>
        {
            Exception exception = CleanException(filterContext.Exception);
            HandleErrorInfo model = new HandleErrorInfo(exception, 
                (string)filterContext.RouteData.Values["controller"], 
                (string)filterContext.RouteData.Values["action"]);

            if (LogException != null)
                LogException(model);

            filterContext.Result = GetResult(filterContext.HttpContext, model);
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = GetHttpError(exception);
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        };


        /// <param name="request">The request from the GlobalAsax is behaves differently!</param>
        public static void HandlerApplication_Error(HttpRequest request, HttpContext context, bool isWebRequest)
        {
            if (Navigator.Manager == null || !Navigator.Manager.Initialized)
                return;

            Exception ex = CleanException(context.Server.GetLastError());
            context.Server.ClearError();

            context.Response.StatusCode = GetHttpError(ex);
            context.Response.TrySkipIisCustomErrors = true;

            HandleErrorInfo hei = new HandleErrorInfo(ex, "Global.asax", "Application_Error");

            if (LogException != null)
                LogException(hei);

            if (isWebRequest)
            {
                HttpContextBase contextBase = context.Request.RequestContext.HttpContext;

                IController controller = new ErrorController { Result = GetResult(contextBase, hei) }; 

                var rd = new RouteData
                { 
                    Values= 
                    {
                        { "Controller", "Error"},
                        { "Action", "Error"}
                    }
                };

                controller.Execute(new RequestContext(contextBase, rd));  
            }
        }

        class ErrorController: Controller
        {
            public ActionResult Result;

            public ActionResult Error()
            {
                return Result;
            }
        }
    }
}