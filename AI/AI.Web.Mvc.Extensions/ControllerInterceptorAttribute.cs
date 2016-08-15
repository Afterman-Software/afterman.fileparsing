using System;
using System.Configuration;
using System.Reflection;
using System.Web.Mvc;
using AI.Common.Tracking;

namespace AI.Web.Mvc.Attributes
{
    using Extensions;

    public class ControllerInterceptorAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ConfigurationManager.AppSettings["CaptureDigitalExhaust"] == null || ConfigurationManager.AppSettings["CaptureDigitalExhaust"].Trim().ToLower() == "true")
            {
                DigitalExhaust exhaust = new DigitalExhaust();
                exhaust.UserName = filterContext.HttpContext.User.Identity.Name;
                exhaust.EventDateTime = DateTime.Now;
                exhaust.Controller = AiStringExtensions.ToFitLength(filterContext.Controller.GetType().ToString(), 128);
                exhaust.ActionOrUrl = AiStringExtensions.ToFitLength(filterContext.ActionDescriptor.ActionName, 1024);
                exhaust.ControllerActionDetails = AiStringExtensions.ToFitLength(filterContext.ActionParameters.ToQString(), 4096);
                exhaust.RouteData = AiStringExtensions.ToFitLength(filterContext.RouteData.Values.ToQString(), 4096);
                exhaust.IsAuthenticated = filterContext.HttpContext.Request.IsAuthenticated;
                exhaust.IsSecure = filterContext.HttpContext.Request.IsSecureConnection;
                exhaust.IsAjax = filterContext.HttpContext.Request.IsAjaxRequest();
                exhaust.QueryString = AiStringExtensions.ToFitLength(filterContext.HttpContext.Request.QueryString.ToQString(), 2048);
                exhaust.RefererUrl = filterContext.HttpContext.Request.UrlReferrer == null ? null : AiStringExtensions.ToFitLength(filterContext.HttpContext.Request.UrlReferrer.ToString(), 512);
                exhaust.UserAgent = AiStringExtensions.ToFitLength(filterContext.HttpContext.Request.UserAgent, 1024);
                exhaust.RemoteAddress = AiStringExtensions.ToFitLength(filterContext.HttpContext.Request.UserHostAddress, 512);
                exhaust.ExtraData = filterContext.HttpContext.Request.Headers.ToQString();

                Hack.DigitalExhaustHandler.Send(exhaust);
            }

            Type controllerType = filterContext.Controller.GetType();
            MethodInfo ctmi = controllerType.GetMethod("DoubleCheckSecurity");
            if (ctmi != null)
            {
                ctmi.Invoke(filterContext.Controller, null);
            }
        }
    }
}