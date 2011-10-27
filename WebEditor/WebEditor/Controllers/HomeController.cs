using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebEditor.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            // TO DO: This should be set in config
            return new RedirectResult("http://www.textadventures.co.uk/");
        }
    }
}
