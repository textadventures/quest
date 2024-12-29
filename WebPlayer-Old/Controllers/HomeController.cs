using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace WebPlayer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect(ConfigurationManager.AppSettings["WebsiteHome"] ?? "https://textadventures.co.uk/");
        }
    }
}