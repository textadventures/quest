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
            EditorDictionary[id] = editor;
            string debugFile = ConfigurationManager.AppSettings["DebugFile"];
            string libFolder = ConfigurationManager.AppSettings["LibraryFolder"];
            editor.Initialise(id, debugFile, libFolder);
            Models.Game model = editor.GetModelForView();
            return PartialView("GameInfo", model);
        }

        public PartialViewResult EditElement(int id, string key)
        {
            Models.Element model = EditorDictionary[id].GetElementModelForView(key);
            return PartialView("ElementEditor", model);
        }

        private Dictionary<int, Services.EditorService> EditorDictionary
        {
            get
            {
                Dictionary<int, Services.EditorService> result = Session["EditorDictionary"] as Dictionary<int, Services.EditorService>;
                if (result == null)
                {
                    result = new Dictionary<int, Services.EditorService>();
                    Session["EditorDictionary"] = result;
                }
                return result;
            }
        }
    }
}
