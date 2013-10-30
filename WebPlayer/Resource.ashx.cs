using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Net;

namespace WebPlayer
{
    public class Resource : IHttpHandler, IReadOnlySessionState
    {
        private Dictionary<string, string> m_mimeTypes = new Dictionary<string, string>();

        public Resource()
        {
            m_mimeTypes.Add(".jpg", "image/jpeg");
            m_mimeTypes.Add(".jpeg", "image/jpeg");
            m_mimeTypes.Add(".gif", "image/gif");
            m_mimeTypes.Add(".bmp", "image/bmp");
            m_mimeTypes.Add(".png", "image/png");
            m_mimeTypes.Add(".wav", "audio/wav");
            m_mimeTypes.Add(".mp3", "audio/mpeg3");
            m_mimeTypes.Add(".ogg", "audio/ogg");
            m_mimeTypes.Add(".js", "application/javascript");
            m_mimeTypes.Add(".ttf", "application/font-woff");
        }

        public void ProcessRequest(HttpContext context)
        {
            SessionResources resources = context.Session["Resources"] as SessionResources;
            string id = context.Request["id"];
            string filename = context.Request["filename"];
            
            if (resources == null || id == null || filename == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            Stream stream = resources.Get(id, filename);

            if (stream == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                Logging.Log.InfoFormat("File not found: {0}", filename);
                return;
            }

            string contentType = GetContentType(filename);
            if (string.IsNullOrEmpty(contentType))
            {
                Logging.Log.InfoFormat("Unknown content type for resource {0}", filename);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            Logging.Log.DebugFormat("Sending resource {0}:'{1}', format is '{2}'", id, filename, contentType);
            context.Response.ContentType = contentType;

            int fileSize = (int)stream.Length;
            byte[] buffer = new byte[fileSize];
            stream.Read(buffer, 0, fileSize);
            stream.Close();
            context.Response.BinaryWrite(buffer);
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
            string result;
            if (m_mimeTypes.TryGetValue(System.IO.Path.GetExtension(filename).ToLower(), out result))
            {
                return result;
            }

            return "";
        }
    }
}