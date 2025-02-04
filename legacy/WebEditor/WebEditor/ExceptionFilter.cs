using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebEditor
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            Exception error = filterContext.Exception;
            Logging.Log.ErrorFormat("Error in {0}: {1}\n{2}\n\nIP: {3}\nReferrer: {4}\nUserAgent: {5}",
                filterContext.RequestContext.HttpContext.Request.Url,
                error.Message,
                error.StackTrace,
                filterContext.RequestContext.HttpContext.Request.UserHostAddress,
                filterContext.RequestContext.HttpContext.Request.UrlReferrer,
                filterContext.RequestContext.HttpContext.Request.UserAgent);
        }
    }
}