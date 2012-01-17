using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using AxeSoftware.Quest;

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

        public JsonResult Load(int id)
        {
            Services.EditorService editor = new Services.EditorService();
            EditorDictionary[id] = editor;
            string debugFile = ConfigurationManager.AppSettings["DebugFile"];
            string libFolder = ConfigurationManager.AppSettings["LibraryFolder"];
            editor.Initialise(id, debugFile, libFolder);
            return Json(editor.GetElementTreeForJson(), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditElement(int id, string key, string tab)
        {
            Models.Element model = EditorDictionary[id].GetElementModelForView(id, key, tab);
            return PartialView("ElementEditor", model);
        }

        [HttpPost]
        public PartialViewResult SaveElement(Models.ElementSaveData element)
        {
            EditorDictionary[element.GameId].SaveElement(element.Key, element);
            string tab = !string.IsNullOrEmpty(element.AdditionalAction) ? element.AdditionalActionTab : null;
            return EditElement(element.GameId, element.RedirectToElement, tab);
        }

        public PartialViewResult EditStringList(int id, string key, IEditorControl control)
        {
            return PartialView("StringListEditor", EditorDictionary[id].GetStringList(key, control));
        }

        public PartialViewResult EditScript(int id, string key, IEditorControl control)
        {
            return PartialView("ScriptEditor", EditorDictionary[id].GetScript(key, control));
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
