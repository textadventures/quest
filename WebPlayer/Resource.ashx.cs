using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Net;

namespace WebPlayer
{
    public class Resource : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            SessionResources resources = context.Session["Resources"] as SessionResources;
            string key = context.Request["id"];
            
            if (resources == null || key == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            string filename = resources.Get(key);

            if (string.IsNullOrEmpty(filename))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            string contentType = GetContentType(filename);
            if (string.IsNullOrEmpty(contentType))
            {
                Logging.Log.InfoFormat("Unknown content type for resource {0}", filename);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            Logging.Log.DebugFormat("Sending resource '{0}', format is '{1}'", filename, contentType);
            context.Response.ContentType = contentType;
            context.Response.WriteFile(filename);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private string GetContentType(string filename)
        {
            string z = System.IO.Path.GetExtension(filename);

            switch (System.IO.Path.GetExtension(filename))
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".png":
                    return "image/png";
            }

            return "";
        }
    }
}