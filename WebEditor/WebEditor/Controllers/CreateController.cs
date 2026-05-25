using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TextAdventures.Quest;
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
            return View(EditorService.GetCreateModel(TemplateFolder));
        }

        [HttpPost]
        public ActionResult New(Models.Create createModel)
        {
            EditorService.PopulateCreateModelLists(createModel, TemplateFolder);
            if (ModelState.IsValid)
            {
                if (EditorController.IsReservedFilename(createModel.GameName, TemplateFolder))
                {
                    ModelState.AddModelError("GameName", string.Format("\"{0}\" cannot be used as a game name as it conflicts with a language template file.", createModel.GameName));
                    return View(createModel);
                }
                int newId = EditorService.CreateNewGame(createModel.SelectedType,
                    createModel.SelectedTemplate,
                    createModel.GameName,
                    TemplateFolder);
                return View("CreateSuccess", new Models.CreateSuccess { Id = newId, Name = createModel.GameName });
            }
            else
            {
                return View(createModel);
            }
        }

        private string TemplateFolder
        {
            get { return Server.MapPath("~/bin/Core/Templates/"); }
        }
    }
}
