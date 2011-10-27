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
    }

    [ModelBinder(typeof(ElementSaveDataModelBinder))]
    public class ElementSaveData
    {
        public Dictionary<string, object> Values { get; set; }
        public int GameId { get; set; }
        public string Key { get; set; }
    }

    public class ElementSaveDataModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ElementSaveData result = new ElementSaveData();
            result.Values = new Dictionary<string, object>();

            int gameId = (int)bindingContext.ValueProvider.GetValue("_game_id").ConvertTo(typeof(int));
            string key = (string)bindingContext.ValueProvider.GetValue("_key").ConvertTo(typeof(string));

            result.GameId = gameId;
            result.Key = key;

            var editorDictionary = controllerContext.RequestContext.HttpContext.Session["EditorDictionary"] as Dictionary<int, Services.EditorService>;
            Models.Element originalElement = editorDictionary[gameId].GetElementModelForView(gameId, key);

            foreach (IEditorTab tab in originalElement.EditorDefinition.Tabs.Values)
            {
                foreach (IEditorControl ctl in tab.Controls)
                {
                    // TO DO: Some form fields are duplicated (remove ContainsKey condition -> throws exception)
                    if (ctl.Attribute != null && !result.Values.ContainsKey(ctl.Attribute))
                    {
                        // TO DO: This will need to have some logic to convert the value to the correct type
                        result.Values.Add(ctl.Attribute, bindingContext.ValueProvider.GetValue(ctl.Attribute).ConvertTo(typeof(string)));
                    }
                }
            }

            return result;
        }
    }

}