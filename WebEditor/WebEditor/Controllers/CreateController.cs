using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebEditor.Services;

namespace WebEditor.Controllers
{
    public class CreateController : Controller
    {
        //
        // GET: /Create/

        public ActionResult Index()
        {
            return RedirectToAction("New");
        }

        public ActionResult New()
        {
            return View(EditorService.GetCreateModel());
        }

        [HttpPost]
        public ActionResult New(Models.Create createModel)
        {
            EditorService.PopulateCreateModelLists(createModel);
            if (ModelState.IsValid)
            {
                // TO DO: Create game
                return View(createModel);
            }
            else
            {
                return View(createModel);
            }
        }
    }
}
