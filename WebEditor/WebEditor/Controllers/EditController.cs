using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AxeSoftware.Quest;
using System.Configuration;

namespace WebEditor.Controllers
{
    public class EditController : Controller
    {
        //
        // GET: /Edit/Game/1

        public ActionResult Game(int id)
        {
            return View();
        }

        public ActionResult Load()
        {
            EditorController editor = new EditorController();
            string debugFile = ConfigurationManager.AppSettings["DebugFile"];
            string libFolder = ConfigurationManager.AppSettings["LibraryFolder"];
            editor.Initialise(debugFile, libFolder);
            WebEditor.Models.Game model = new Models.Game();
            model.Name = editor.GameName;
            return PartialView("GameInfo", model);
        }
    }
}
