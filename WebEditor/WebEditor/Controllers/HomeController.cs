using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace WebEditor.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return new RedirectResult(ConfigurationManager.AppSettings["WebsiteHome"] ?? "http://textadventures.co.uk/");
        }
    }
}
