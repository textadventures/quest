using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPlayer.Controllers
{
    public class GameSessionController : Controller
    {
        [HttpPost]
        public ActionResult Init(string userId, string sessionId, string bufferId)
        {
            OutputBuffer buffer;
            if (OutputBuffers.TryGetValue(bufferId, out buffer))
            {
                buffer.InitOutputLogger(userId, sessionId);
            }

            return Json(new object());
        }

        private Dictionary<string, OutputBuffer> OutputBuffers
        {
            get { return Session["OutputBuffers"] as Dictionary<string, OutputBuffer>; }
        }
    }
}