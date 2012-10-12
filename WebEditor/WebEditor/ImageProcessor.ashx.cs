using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace WebEditor
{
    /// <summary>
    /// Summary description for ImageProcessor
    /// </summary>
    public class ImageProcessor : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "image/jpeg";
            // parse the filename
            int gameId = Int32.Parse(context.Request["gameId"]);
            string filename = context.Request["image"];
            int width, height = 0;
            Int32.TryParse(context.Request["w"], out width);
            Int32.TryParse(context.Request["h"], out height);
            string uploadPath = Services.FileManagerLoader.GetFileManager().UploadPath(gameId);
            Image fullsizeImg = Image.FromFile(Path.Combine(uploadPath, filename));

            context.Response.Clear();
            context.Response.BufferOutput = true;

            if (fullsizeImg != null)
            {
                if (height > 0 && width > 0)
                {
                    // resize as per the height width requested
                    if (height >= fullsizeImg.Height && width >= fullsizeImg.Width)
                        fullsizeImg.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                    else
                    {
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
            }

            fullsizeImg.Dispose();

            context.Response.Flush();

            
            context.Response.Write("Hello World");
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