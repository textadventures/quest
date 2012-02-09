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
            ViewBag.Title = "Editor";
            return View(model);
        }

        public JsonResult Load(int id)
        {
            Services.EditorService editor = new Services.EditorService();
            EditorDictionary[id] = editor;
            string libFolder = ConfigurationManager.AppSettings["LibraryFolder"];
            editor.Initialise(id, libFolder);
            return Json(editor.GetElementTreeForJson(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Scripts(int id)
        {
            return Json(EditorDictionary[id].GetScriptAdderJson(), JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult EditElement(int id, string key, string tab, string error, string refreshTreeSelectElement)
        {
            if (Session["EditorDictionary"] == null)
            {
                return Timeout();
            }
            Models.Element model = EditorDictionary[id].GetElementModelForView(id, key, tab, error, refreshTreeSelectElement, ModelState);
            return PartialView("ElementEditor", model);
        }

        [HttpPost]
        public PartialViewResult SaveElement(Models.ElementSaveData element)
        {
            if (!element.Success)
            {
                return Timeout();
            }
            var result = EditorDictionary[element.GameId].SaveElement(element.Key, element);
            string tab = !string.IsNullOrEmpty(element.AdditionalAction) ? element.AdditionalActionTab : null;
            return EditElement(element.GameId, element.RedirectToElement, tab, result.Error, result.RefreshTreeSelectElement);
        }

        private PartialViewResult Timeout()
        {
            return PartialView("ElementEditor", new Models.Element
            {
                PopupError = "Sorry, your session has timed out.",
                Reload = "1"
            });
        }

        public PartialViewResult EditStringList(int id, string key, IEditorControl control)
        {
            return PartialView("StringListEditor", EditorDictionary[id].GetStringListModel(key, control));
        }

        public PartialViewResult EditScriptStringList(int id, string key, string path, IEditorControl control)
        {
            return PartialView("StringListEditor", EditorDictionary[id].GetScriptStringListModel(key, path, control));
        }

        public PartialViewResult EditScript(int id, string key, IEditorControl control)
        {
            return PartialView("ScriptEditor", EditorDictionary[id].GetScriptModel(id, key, control, ModelState));
        }

        public PartialViewResult ElementsList(int id, string key, IEditorControl control)
        {
            return PartialView("ElementsList", EditorDictionary[id].GetElementsListModel(id, key, control));
        }

        public PartialViewResult EditExits(int id, string key, IEditorControl control)
        {
            return PartialView("ExitsEditor", EditorDictionary[id].GetExitsModel(id, key, control));
        }

        public PartialViewResult EditVerbs(int id, string key, IEditorControl control)
        {
            return PartialView("VerbsEditor", EditorDictionary[id].GetVerbsModel(id, key, control));
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

        public JsonResult RefreshTree(int id)
        {
            return Json(EditorDictionary[id].GetElementTreeForJson(), JsonRequestBehavior.AllowGet);
        }
    }
}
