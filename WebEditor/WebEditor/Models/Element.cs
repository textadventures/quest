using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Web.Mvc;

namespace WebEditor.Models
{
    public class Element
    {
        public int GameId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public IEditorDefinition EditorDefinition { get; set; }
        public IEditorData EditorData { get; set; }
        public string Tab { get; set; }
    }

    [ModelBinder(typeof(ElementSaveDataModelBinder))]
    public class ElementSaveData
    {
        public Dictionary<string, object> Values { get; set; }
        public int GameId { get; set; }
        public string Key { get; set; }
        public string RedirectToElement { get; set; }
        public string AdditionalAction { get; set; }
        public string AdditionalActionTab { get; set; }
    }

    public class ElementSaveDataModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ElementSaveData result = new ElementSaveData();
            result.Values = new Dictionary<string, object>();

            int gameId = (int)bindingContext.ValueProvider.GetValue("_game_id").ConvertTo(typeof(int));
            string key = (string)bindingContext.ValueProvider.GetValue("_key").ConvertTo(typeof(string));
            string redirectToElement = (string)bindingContext.ValueProvider.GetValue("_redirectToElement").ConvertTo(typeof(string));
            string additionalAction = (string)bindingContext.ValueProvider.GetValue("_additionalAction").ConvertTo(typeof(string));
            string additionalActionTab = (string)bindingContext.ValueProvider.GetValue("_additionalActionTab").ConvertTo(typeof(string));

            result.GameId = gameId;
            result.Key = key;
            result.RedirectToElement = redirectToElement;
            result.AdditionalAction = additionalAction;
            result.AdditionalActionTab = additionalActionTab;

            var editorDictionary = controllerContext.RequestContext.HttpContext.Session["EditorDictionary"] as Dictionary<int, Services.EditorService>;

            // TO DO: This throws exception if session has expired (editorDictionary = null)
            Models.Element originalElement = editorDictionary[gameId].GetElementModelForView(gameId, key, null);

            foreach (IEditorTab tab in originalElement.EditorDefinition.Tabs.Values)
            {
                foreach (IEditorControl ctl in tab.Controls.Where(c => c.Attribute != null))
                {
                    object saveValue = null;
                    bool handled = true;    // TO DO: Temporary until all controltypes are handled

                    ValueProviderResult value = bindingContext.ValueProvider.GetValue(ctl.Attribute);

                    switch (ctl.ControlType)
                    {
                        case "textbox":
                            saveValue = value.ConvertTo(typeof(string));
                            break;
                        case "checkbox":
                            saveValue = value.ConvertTo(typeof(bool));
                            break;
                        default:
                            handled = false;    // TO DO: Temporary until all controltypes are handled
                            break;
                    }

                    if (handled)
                    {
                        result.Values.Add(ctl.Attribute, saveValue);
                    }
                }
            }

            return result;
        }
    }

}