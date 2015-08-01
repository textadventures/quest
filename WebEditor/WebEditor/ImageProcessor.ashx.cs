using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Web.SessionState;
using System.Configuration;

namespace WebEditor
{
    /// <summary>
    /// Summary description for ImageProcessor
    /// </summary>
    public class ImageProcessor : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {            
            // parse the filename
            int gameId = Int32.Parse(context.Request["gameId"]);
            string filename = context.Request["image"];
            
            string uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(gameId);
            
            if (Config.AzureFiles)
            {
                context.Response.Redirect(ConfigurationManager.AppSettings["AzureFilesBase"] + uploadPath + "/" + filename, false);
                context.ApplicationInstance.CompleteRequest();
                return;
            }

            string path = Path.Combine(uploadPath, filename);
            if (!File.Exists(path)) return;

            Image fullsizeImg = Image.FromFile(path);

            int width, height = 0;
            Int32.TryParse(context.Request["w"], out width);
            Int32.TryParse(context.Request["h"], out height);

            context.Response.Clear();
            context.Response.BufferOutput = true;
            string format = fullsizeImg.RawFormat.ToString();
            // Set correct content type header
            if (ImageFormat.Jpeg.Equals(fullsizeImg.RawFormat))
                context.Response.ContentType = "image/jpeg";
            else if (ImageFormat.Png.Equals(fullsizeImg.RawFormat))
                context.Response.ContentType = "image/png";
            else if (ImageFormat.Gif.Equals(fullsizeImg.RawFormat))
                context.Response.ContentType = "image/gif";
            else if (ImageFormat.Tiff.Equals(fullsizeImg.RawFormat))
                context.Response.ContentType = "image/tiff";
            else if (ImageFormat.Bmp.Equals(fullsizeImg.RawFormat))
                context.Response.ContentType = "image/bmp";
            else
                context.Response.ContentType = "image";  //default -- when MIME Type not known

            if (height > 0 && width > 0)
            {
                // resize as per the height width requested
                if (height >= fullsizeImg.Height && width >= fullsizeImg.Width)
                    fullsizeImg.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                else
                {
                    // Maintain aspect ratio.
                    float widthPerc = (float)width / (float)fullsizeImg.Width;
                    float heightPerc = (float)height / (float)fullsizeImg.Height;
                    float resizePerc = heightPerc < widthPerc ? heightPerc : widthPerc;
                    height = (int)(fullsizeImg.Height * resizePerc);
                    width = (int)(fullsizeImg.Width * resizePerc);

                    Bitmap bmp = new Bitmap(width, height);
                    Graphics objGraphics = Graphics.FromImage(bmp);
                    objGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;
                    objGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    objGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    objGraphics.DrawImage(fullsizeImg, 0, 0, bmp.Width, bmp.Height);
                    bmp.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                    objGraphics.Dispose();
                    bmp.Dispose();
                }
            }
            else
            {
                fullsizeImg.Save(context.Response.OutputStream, ImageFormat.Jpeg);
            }

            fullsizeImg.Dispose();

            context.Response.Flush();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}