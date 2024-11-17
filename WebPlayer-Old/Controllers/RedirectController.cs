using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPlayer.Controllers
{
    public class RedirectController : Controller
    {
        public ActionResult Games(string id, string path)
        {
            if (path.EndsWith(".html") || path.EndsWith(".htm"))
            {
                return Redirect("https://textadventures.co.uk/games/play/" + id);
            }
            return HttpNotFound();
        }
    }
}