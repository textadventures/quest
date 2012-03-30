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
                int newId = EditorService.CreateNewGame(createModel.SelectedType, createModel.SelectedTemplate, createModel.GameName);
                return View("CreateSuccess", new Models.CreateSuccess { Id = newId, Name = createModel.GameName });
            }
            else
            {
                return View(createModel);
            }
        }
    }
}
