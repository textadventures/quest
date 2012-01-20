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

        public class ScriptsSaveData
        {
            public List<ScriptSaveData> ScriptLines { get; private set; }

            public ScriptsSaveData()
            {
                ScriptLines = new List<ScriptSaveData>();
            }
        }

        public class ScriptSaveData
        {
            public Dictionary<string, object> Attributes { get; set; }

            public ScriptSaveData()
            {
                Attributes = new Dictionary<string, object>();
            }
        }
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
                if (!tab.IsTabVisible(originalElement.EditorData)) continue;
                foreach (IEditorControl ctl in tab.Controls.Where(c => c.Attribute != null))
                {
                    if (!ctl.IsControlVisible(originalElement.EditorData)) continue;
                    object saveValue = null;
                    bool handled = true;    // TO DO: Temporary until all controltypes are handled

                    ValueProviderResult value = bindingContext.ValueProvider.GetValue(ctl.Attribute);

                    switch (ctl.ControlType)
                    {
                        case "textbox":
                        case "dropdown":
                            saveValue = value.ConvertTo(typeof(string));
                            break;
                        case "checkbox":
                            saveValue = value.ConvertTo(typeof(bool));
                            break;
                        case "script":
                            saveValue = BindScript(bindingContext.ValueProvider, ctl.Attribute, originalElement.EditorData, editorDictionary[gameId].Controller);
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

        private object BindScript(IValueProvider provider, string attribute, IEditorData data, EditorController controller)
        {
            IEditableScripts originalScript = (IEditableScripts)data.GetAttribute(attribute);

            if (originalScript == null) return null;

            ElementSaveData.ScriptsSaveData result = new ElementSaveData.ScriptsSaveData();
            
            BindScriptLines(provider, attribute, controller, originalScript, result);
            
            return result;
        }

        private void BindScriptLines(IValueProvider provider, string attribute, EditorController controller, IEditableScripts originalScript, ElementSaveData.ScriptsSaveData result)
        {
            int count = 0;
            foreach (IEditableScript script in originalScript.Scripts)
            {
                ElementSaveData.ScriptSaveData scriptLine = new ElementSaveData.ScriptSaveData();

                if (script.Type != ScriptType.If)
                {
                    IEditorDefinition definition = controller.GetEditorDefinition(script);
                    foreach (IEditorControl ctl in definition.Controls.Where(c => c.Attribute != null))
                    {
                        object value = GetScriptParameterValue(provider, attribute, count, ctl);
                        scriptLine.Attributes.Add(ctl.Attribute, value);
                    }
                }
                else
                {
                    EditableIfScript ifScript = (EditableIfScript)script;

                    string expressionKey = string.Format("{0}-{1}-expression", attribute, count);
                    ValueProviderResult expressionValue = provider.GetValue(expressionKey);
                    scriptLine.Attributes.Add("expression", expressionValue.ConvertTo(typeof(string)));

                    ElementSaveData.ScriptsSaveData thenScriptResult = new ElementSaveData.ScriptsSaveData();
                    BindScriptLines(provider, string.Format("{0}-{1}-then", attribute, count), controller, ifScript.ThenScript, thenScriptResult);
                    scriptLine.Attributes.Add("then", thenScriptResult);

                    int elseIfCount = 0;
                    foreach (EditableIfScript.EditableElseIf elseIf in ifScript.ElseIfScripts)
                    {
                        string elseIfExpressionKey = string.Format("{0}-{1}-elseif{2}-expression", attribute, count, elseIfCount);
                        ValueProviderResult elseIfExpressionValue = provider.GetValue(elseIfExpressionKey);
                        scriptLine.Attributes.Add(string.Format("elseif{0}-expression", elseIfCount), elseIfExpressionValue.ConvertTo(typeof(string)));

                        ElementSaveData.ScriptsSaveData elseIfScriptResult = new ElementSaveData.ScriptsSaveData();
                        BindScriptLines(provider, string.Format("{0}-{1}-elseif{2}", attribute, count, elseIfCount), controller, elseIf.EditableScripts, elseIfScriptResult);
                        scriptLine.Attributes.Add(string.Format("elseif{0}-then", elseIfCount), elseIfScriptResult);
                        elseIfCount++;
                    }

                    if (ifScript.ElseScript != null)
                    {
                        ElementSaveData.ScriptsSaveData elseScriptResult = new ElementSaveData.ScriptsSaveData();
                        BindScriptLines(provider, string.Format("{0}-{1}-else", attribute, count), controller, ifScript.ElseScript, elseScriptResult);
                        scriptLine.Attributes.Add("else", elseScriptResult);
                    }
                }

                result.ScriptLines.Add(scriptLine);
                count++;
            }
        }

        private object GetScriptParameterValue(IValueProvider provider, string attribute, int count, IEditorControl ctl)
        {
            if (ctl.ControlType == "expression")
            {
                string simpleEditor = ctl.GetString("simpleeditor") ?? "textbox";
                string dropdownKey = string.Format("{0}-{1}-{2}-expressioneditordropdown", attribute, count, ctl.Attribute);
                string dropdownKeyValue = provider.GetValue(dropdownKey).ConvertTo(typeof(string)) as string;
                if (dropdownKeyValue == "expression")
                {
                    string key = string.Format("{0}-{1}-{2}-expressioneditor", attribute, count, ctl.Attribute);
                    ValueProviderResult value = provider.GetValue(key);
                    return value == null ? null : value.ConvertTo(typeof(string));
                }
                else
                {
                    if (simpleEditor == "boolean")
                    {
                        return dropdownKeyValue == "yes" ? "true" : "false";
                    }
                    else
                    {
                        string key = string.Format("{0}-{1}-{2}-simpleeditor", attribute, count, ctl.Attribute);
                        ValueProviderResult value = provider.GetValue(key);
                        string simpleValue = value.ConvertTo(typeof(string)) as string;

                        switch (simpleEditor)
                        {
                            case "objects":
                            case "number":
                                return simpleValue;
                            default:
                                return EditorUtility.ConvertFromSimpleStringExpression(simpleValue);
                        }
                    }
                }
            }
            else
            {
                string key = string.Format("{0}-{1}-{2}", attribute, count, ctl.Attribute);
                ValueProviderResult value = provider.GetValue(key);
                return value == null ? null : value.ConvertTo(typeof(string));
            }
        }
    }
}