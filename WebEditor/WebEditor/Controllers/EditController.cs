using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace WebEditor.Controllers
{
    public class EditController : Controller
    {
        //
        // GET: /Edit/Game/1

        public ActionResult Game(int id)
        {
            Models.Editor model = new Models.Editor();
            model.GameId = id;
            return View(model);
        }

        public PartialViewResult Load(int id)
        {
            Services.EditorService editor = new Services.EditorService();
            string debugFile = ConfigurationManager.AppSettings["DebugFile"];
            string libFolder = ConfigurationManager.AppSettings["LibraryFolder"];
            editor.Initialise(debugFile, libFolder);
            Models.Game model = editor.GetModelForView();
            return PartialView("GameInfo", model);
        }
    }
}
