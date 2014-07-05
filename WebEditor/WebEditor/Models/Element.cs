using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;
using System.Web.Mvc;
using System.Text.RegularExpressions;

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
        public string Error { get; set; }
        public Dictionary<string, List<string>> OtherElementErrors { get; set; }
        public EditorController Controller { get; set; }
        public string RefreshTreeSelectElement { get; set; }
        public string PopupError { get; set; }
        public string NewObjectPossibleParents { get; set; }
        public string EnabledButtons { get; set; }
        public string PageTitle { get; set; }
        public string Reload { get; set; }
        public string AvailableVerbs { get; set; }
        public string UIAction { get; set; }
        public bool CanMove { get; set; }
        public string MovePossibleParents { get; set; }
        public bool IsElement { get; set; }
        public string AllObjects { get; set; }
        public string NextPage { get; set; }
        public string HiddenScripts { get; set; }
        public string ScriptCategories { get; set; }
    }

    public class EditAttributeModel
    {
        public Element Element { get; set; }
        public IEditorControl Control { get; set; }
        public object Value { get; set; }
    }

    public class IgnoredValue
    {
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
        public bool Success { get; set; }

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
            public bool IsSelected { get; set; }
            public Dictionary<string, object> Attributes { get; set; }
            public string Error { get; set; }

            public ScriptSaveData()
            {
                Attributes = new Dictionary<string, object>();
            }
        }

        public class ObjectReferenceSaveData
        {
            public string Reference { get; set; }
        }

        public class PatternSaveData
        {
            public string Pattern { get; set; }
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
            string ignoreExpression = (string)bindingContext.ValueProvider.GetValue("_ignoreExpression").ConvertTo(typeof(string));

            result.GameId = gameId;
            result.Key = key;
            result.RedirectToElement = redirectToElement;
            result.AdditionalAction = additionalAction;
            result.AdditionalActionTab = additionalActionTab;

            var editorDictionary = controllerContext.RequestContext.HttpContext.Session["EditorDictionary"] as Dictionary<int, Services.EditorService>;

            if (editorDictionary == null)
            {
                result.Success = false;
                return result;
            }

            if (!editorDictionary.ContainsKey(gameId))
            {
                Logging.Log.ErrorFormat("Current Session does not contain a game id of {0}", gameId);
                result.Success = false;
                return result;
            }

            if (editorDictionary[gameId] == null)
            {
                Logging.Log.ErrorFormat("Current Session has game id {0} = null", gameId);
                result.Success = false;
                return false;
            }

            Models.Element originalElement = editorDictionary[gameId].GetElementModelForView(gameId, key);

            if (originalElement == null)
            {
                Logging.Log.ErrorFormat("BindModel failed for game {0} element '{1}' - originalElement is null", gameId, key);
                result.Success = false;
                return false;
            }
            if (originalElement.EditorDefinition == null)
            {
                Logging.Log.ErrorFormat("BindModel failed for game {0} element '{1}' - originalElement.EditorDefinition is null", gameId, key);
                result.Success = false;
                return false;
            }
            if (originalElement.EditorDefinition.Tabs == null)
            {
                Logging.Log.ErrorFormat("BindModel failed for game {0} element '{1}' - originalElement.EditorDefinition.Tabs is null", gameId, key);
                result.Success = false;
                return false;
            }

            foreach (IEditorTab tab in originalElement.EditorDefinition.Tabs.Values)
            {
                if (!tab.IsTabVisible(originalElement.EditorData)) continue;
                if (tab.GetBool("desktop")) continue;
                if (editorDictionary[gameId].Controller.SimpleMode && !tab.IsTabVisibleInSimpleMode) continue;
                foreach (IEditorControl ctl in tab.Controls)
                {
                    if (!ctl.IsControlVisible(originalElement.EditorData)) continue;
                    if (ctl.GetBool("desktop")) continue;
                    if (editorDictionary[gameId].Controller.SimpleMode && !ctl.IsControlVisibleInSimpleMode) continue;
                    BindControl(bindingContext, result, gameId, ignoreExpression, editorDictionary, originalElement, ctl, ctl.ControlType);
                }
            }

            result.Success = true;

            return result;
        }

        private void BindControl(ModelBindingContext bindingContext, ElementSaveData result, int gameId, string ignoreExpression, Dictionary<int, Services.EditorService> editorDictionary, Models.Element originalElement, IEditorControl ctl, string controlType, string attribute = null)
        {
            object saveValue = null;
            bool addSaveValueToResult = true;
            if (attribute == null) attribute = ctl.Attribute;

            // check to see if attribute changes from attribute editor is being passed along
            // if so, use the attribute editor value

            if (bindingContext.ValueProvider.ContainsPrefix("attr_" + attribute))
            {
                attribute = "attr_" + attribute;
            }

            switch (controlType)
            {
                case "textbox":
                case "dropdown":
                case "file":
                    saveValue = GetValueProviderString(bindingContext.ValueProvider, attribute);
                    break;
                case "number":
                    string stringValue = GetValueProviderString(bindingContext.ValueProvider, attribute);
                    int intValue;
                    int.TryParse(stringValue, out intValue);
                    int? min = ctl.GetInt("minimum");
                    int? max = ctl.GetInt("maximum");
                    if (min.HasValue && intValue < min) intValue = min.Value;
                    if (max.HasValue && intValue > max) intValue = max.Value;
                    saveValue = intValue;
                    break;
                case "numberdouble":
                    string stringDoubleValue = GetValueProviderString(bindingContext.ValueProvider, attribute);
                    double doubleValue;
                    double.TryParse(stringDoubleValue, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out doubleValue);
                    double? doubleMin = ctl.GetDouble("minimum");
                    double? doubleMax = ctl.GetDouble("maximum");
                    if (doubleMin.HasValue && doubleValue < doubleMin) doubleValue = doubleMin.Value;
                    if (doubleMax.HasValue && doubleValue > doubleMax) doubleValue = doubleMax.Value;
                    saveValue = doubleValue;
                    break;
                case "richtext":
                    // Replace all new line characters with a <Br/> tag here
                    string richtextValue = GetValueProviderString(bindingContext.ValueProvider, attribute);
                    saveValue = StripHTMLComments(HttpUtility.HtmlDecode(richtextValue.Replace(Environment.NewLine, "<br/>")));
                    break;
                case "checkbox":
                    ValueProviderResult value = bindingContext.ValueProvider.GetValue(attribute);
                    if (value == null)
                    {
                        Logging.Log.ErrorFormat("Expected true/false value for '{0}', but got null", attribute);
                        saveValue = false;
                    }
                    else
                    {
                        saveValue = value.ConvertTo(typeof(bool));
                    }
                    break;
                case "script":
                    saveValue = BindScript(bindingContext.ValueProvider, attribute, originalElement.EditorData, editorDictionary[gameId].Controller, ignoreExpression);
                    break;
                case "multi":
                    string type = WebEditor.Views.Edit.ControlHelpers.GetTypeName(originalElement.EditorData.GetAttribute(attribute));
                    string subControlType = WebEditor.Views.Edit.ControlHelpers.GetEditorNameForType(type, ctl.GetDictionary("editors"));
                    BindControl(bindingContext, result, gameId, ignoreExpression, editorDictionary, originalElement, ctl, subControlType);
                    addSaveValueToResult = false;
                    break;
                case "objects":
                    saveValue = new ElementSaveData.ObjectReferenceSaveData
                    {
                        Reference = GetValueProviderString(bindingContext.ValueProvider, "dropdown-" + attribute)
                    };
                    break;
                case "verbs":
                    IEditorDataExtendedAttributeInfo extendedData = (IEditorDataExtendedAttributeInfo)originalElement.EditorData;
                    foreach (IEditorAttributeData attr in extendedData.GetAttributeData().Where(a => !a.IsInherited))
                    {
                        if (editorDictionary[gameId].Controller.IsVerbAttribute(attr.AttributeName))
                        {
                            object attrValue = extendedData.GetAttribute(attr.AttributeName);
                            string attrStringValue = attrValue as string;
                            IEditableScripts attrScriptValue = attrValue as IEditableScripts;
                            IEditableDictionary<IEditableScripts> attrDictionaryValue = attrValue as IEditableDictionary<IEditableScripts>;
                            if (attrStringValue != null)
                            {
                                BindControl(bindingContext, result, gameId, ignoreExpression, editorDictionary, originalElement, ctl, "textbox", attr.AttributeName);
                            }
                            else if (attrScriptValue != null)
                            {
                                BindControl(bindingContext, result, gameId, ignoreExpression, editorDictionary, originalElement, ctl, "script", attr.AttributeName);
                            }
                            else if (attrDictionaryValue != null)
                            {
                                BindControl(bindingContext, result, gameId, ignoreExpression, editorDictionary, originalElement, ctl, "scriptdictionary", attr.AttributeName);
                            }
                        }
                    }
                    addSaveValueToResult = false;
                    break;
                case "list":
                    addSaveValueToResult = false;
                    break;
                case "pattern":
                    saveValue = new ElementSaveData.PatternSaveData
                    {
                        Pattern = GetValueProviderString(bindingContext.ValueProvider, attribute)
                    };
                    break;
                case "scriptdictionary":
                    var originalDictionary = originalElement.EditorData.GetAttribute(attribute) as IEditableDictionary<IEditableScripts>;
                    saveValue = BindScriptDictionary(bindingContext.ValueProvider, editorDictionary[gameId].Controller, ignoreExpression, originalDictionary, attribute);
                    break;
                case "stringdictionary":
                case "gamebookoptions":
                    var originalStringDictionary = originalElement.EditorData.GetAttribute(attribute) as IEditableDictionary<string>;
                    saveValue = BindStringDictionary(bindingContext.ValueProvider, editorDictionary[gameId].Controller, ignoreExpression, originalStringDictionary, attribute, ctl);
                    break;
                default:
                    if (attribute == null || controlType == null)
                    {
                        addSaveValueToResult = false;
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Save data model binder not implemented for control type {0}", controlType));
                    }
                    break;
            }

            if (addSaveValueToResult)
            {
                if (result.Values.ContainsKey(attribute))
                {
                    Logging.Log.ErrorFormat("SaveData already contains attribute \"{0}\" - saveValue (\"{1}\") discarded", attribute, saveValue);
                }
                else
                {
                    // remove attr prefix if it is in the attribute name:
                    if (attribute.StartsWith("attr_"))
                    {
                        attribute = attribute.Substring(5);
                    }

                    result.Values.Add(attribute, saveValue);
                }
            }
        }

        private object BindScript(IValueProvider provider, string attribute, IEditorData data, EditorController controller, string ignoreExpression)
        {
            IEditableScripts originalScript = (IEditableScripts)data.GetAttribute(attribute);

            if (originalScript == null) return null;

            ElementSaveData.ScriptsSaveData result = new ElementSaveData.ScriptsSaveData();

            BindScriptLines(provider, attribute, controller, originalScript, result, ignoreExpression);

            return result;
        }

        private void BindScriptLines(IValueProvider provider, string attribute, EditorController controller, IEditableScripts originalScript, ElementSaveData.ScriptsSaveData result, string ignoreExpression)
        {
            if (originalScript == null) return;
            int count = 0;
            foreach (IEditableScript script in originalScript.Scripts)
            {
                ElementSaveData.ScriptSaveData scriptLine = new ElementSaveData.ScriptSaveData();
                scriptLine.IsSelected = (bool)provider.GetValue(string.Format("selected-{0}-{1}", attribute, count)).ConvertTo(typeof(bool));

                if (script.Type != ScriptType.If)
                {
                    IEditorDefinition definition = controller.GetEditorDefinition(script);
                    foreach (IEditorControl ctl in definition.Controls.Where(c => c.Attribute != null))
                    {
                        string key = string.Format("{0}-{1}-{2}", attribute, count, ctl.Attribute);

                        if (ctl.ControlType == "script")
                        {
                            IEditorData scriptEditorData = controller.GetScriptEditorData(script);
                            IEditableScripts originalSubScript = (IEditableScripts)scriptEditorData.GetAttribute(ctl.Attribute);
                            ElementSaveData.ScriptsSaveData scriptResult = new ElementSaveData.ScriptsSaveData();
                            BindScriptLines(provider, key, controller, originalSubScript, scriptResult, ignoreExpression);
                            scriptLine.Attributes.Add(ctl.Attribute, scriptResult);
                        }
                        else if (ctl.ControlType == "scriptdictionary")
                        {
                            IEditorData dictionaryData = controller.GetScriptEditorData(script);
                            IEditableDictionary<IEditableScripts> dictionary = (IEditableDictionary<IEditableScripts>)dictionaryData.GetAttribute(ctl.Attribute);
                            ElementSaveData.ScriptSaveData switchResult = BindScriptDictionary(provider, controller, ignoreExpression, dictionary, key);
                            scriptLine.Attributes.Add(ctl.Attribute, switchResult);
                        }
                        else if (ctl.ControlType == "list")
                        {
                            // do nothing
                        }
                        else
                        {
                            object value = GetScriptParameterValue(
                                scriptLine,
                                controller,
                                provider,
                                key,
                                ctl.ControlType,
                                ctl.GetString("simpleeditor") ?? "textbox",
                                ctl.GetString("usetemplates"),
                                (string)script.GetParameter(ctl.Attribute),
                                ignoreExpression
                            );
                            scriptLine.Attributes.Add(ctl.Attribute, value);
                        }
                    }
                }
                else
                {
                    EditableIfScript ifScript = (EditableIfScript)script;

                    object expressionValue = GetScriptParameterValue(
                        scriptLine,
                        controller,
                        provider,
                        string.Format("{0}-{1}-expression", attribute, count),
                        "expression",
                        null,
                        "if",
                        (string)ifScript.GetAttribute("expression"),
                        ignoreExpression
                    );

                    scriptLine.Attributes.Add("expression", expressionValue);

                    ElementSaveData.ScriptsSaveData thenScriptResult = new ElementSaveData.ScriptsSaveData();
                    BindScriptLines(provider, string.Format("{0}-{1}-then", attribute, count), controller, ifScript.ThenScript, thenScriptResult, ignoreExpression);
                    scriptLine.Attributes.Add("then", thenScriptResult);

                    int elseIfCount = 0;
                    foreach (EditableIfScript.EditableElseIf elseIf in ifScript.ElseIfScripts)
                    {
                        object elseIfExpressionValue = GetScriptParameterValue(
                            scriptLine,
                            controller,
                            provider,
                            string.Format("{0}-{1}-elseif{2}-expression", attribute, count, elseIfCount),
                            "expression",
                            null,
                            "if",
                            elseIf.Expression,
                            ignoreExpression
                        );

                        scriptLine.Attributes.Add(string.Format("elseif{0}-expression", elseIfCount), elseIfExpressionValue);

                        ElementSaveData.ScriptsSaveData elseIfScriptResult = new ElementSaveData.ScriptsSaveData();
                        BindScriptLines(provider, string.Format("{0}-{1}-elseif{2}", attribute, count, elseIfCount), controller, elseIf.EditableScripts, elseIfScriptResult, ignoreExpression);
                        scriptLine.Attributes.Add(string.Format("elseif{0}-then", elseIfCount), elseIfScriptResult);
                        elseIfCount++;
                    }

                    if (ifScript.ElseScript != null)
                    {
                        ElementSaveData.ScriptsSaveData elseScriptResult = new ElementSaveData.ScriptsSaveData();
                        BindScriptLines(provider, string.Format("{0}-{1}-else", attribute, count), controller, ifScript.ElseScript, elseScriptResult, ignoreExpression);
                        scriptLine.Attributes.Add("else", elseScriptResult);
                    }
                }

                result.ScriptLines.Add(scriptLine);
                count++;
            }
        }

        private ElementSaveData.ScriptSaveData BindScriptDictionary(IValueProvider provider, EditorController controller, string ignoreExpression, IEditableDictionary<IEditableScripts> dictionary, string key)
        {
            ElementSaveData.ScriptSaveData result = new ElementSaveData.ScriptSaveData();
            if (dictionary != null)
            {
                int dictionaryCount = 0;
                foreach (var item in dictionary.Items.Values)
                {
                    string expressionValue = GetValueProviderString(provider, string.Format("{0}-key{1}", key, dictionaryCount));
                    result.Attributes.Add(string.Format("key{0}", dictionaryCount), expressionValue);

                    ElementSaveData.ScriptsSaveData scriptResult = new ElementSaveData.ScriptsSaveData();
                    BindScriptLines(provider, string.Format("{0}-value{1}", key, dictionaryCount), controller, item.Value, scriptResult, ignoreExpression);
                    result.Attributes.Add(string.Format("value{0}", dictionaryCount), scriptResult);

                    dictionaryCount++;
                }
            }
            return result;
        }

        private ElementSaveData.ScriptSaveData BindStringDictionary(IValueProvider provider, EditorController controller, string ignoreExpression, IEditableDictionary<string> dictionary, string key, IEditorControl ctl)
        {
            ElementSaveData.ScriptSaveData result = new ElementSaveData.ScriptSaveData();
            if (dictionary != null)
            {
                int dictionaryCount = 0;
                foreach (var item in dictionary.Items)
                {
                    string keyValue;
                    if (string.IsNullOrEmpty(ctl.GetString("source")))
                    {
                        keyValue = GetValueProviderString(provider, string.Format("{0}-key{1}", key, dictionaryCount));
                    }
                    else
                    {
                        // key is not editable when a source is specified
                        keyValue = item.Key;
                    }
                    result.Attributes.Add(string.Format("key{0}", dictionaryCount), keyValue);

                    string valueValue = GetValueProviderString(provider, string.Format("{0}-value{1}", key, dictionaryCount));
                    result.Attributes.Add(string.Format("value{0}", dictionaryCount), valueValue);

                    dictionaryCount++;
                }
            }
            return result;
        }

        private object GetScriptParameterValue(ElementSaveData.ScriptSaveData scriptLine, EditorController controller, IValueProvider provider, string attributePrefix, string controlType, string simpleEditor, string templatesFilter, string oldValue, string ignoreExpression)
        {
            if (attributePrefix == ignoreExpression) return new IgnoredValue();

            if (controlType == "expression")
            {
                if (templatesFilter == null)
                {
                    string dropdownKey = string.Format("{0}-expressioneditordropdown", attributePrefix);
                    ValueProviderResult dropdownKeyValueResult = provider.GetValue(dropdownKey);
                    string dropdownKeyValue = (dropdownKeyValueResult != null) ? dropdownKeyValueResult.ConvertTo(typeof(string)) as string : null;
                    if (dropdownKeyValue == "expression" || dropdownKeyValue == null)
                    {
                        string key = string.Format("{0}-expressioneditor", attributePrefix);
                        return GetAndValidateValueProviderString(provider, key, oldValue, scriptLine, controller);
                    }
                    else
                    {
                        if (simpleEditor == "boolean")
                        {
                            return dropdownKeyValue == "yes" ? "true" : "false";
                        }
                        else
                        {
                            string key = string.Format("{0}-simpleeditor", attributePrefix);
                            string simpleValue = GetValueProviderString(provider, key);
                            if (simpleValue == null) return string.Empty;

                            switch (simpleEditor)
                            {
                                case "objects":
                                case "number":
                                case "numberdouble":
                                    return simpleValue;
                                default:
                                    return EditorUtility.ConvertFromSimpleStringExpression(simpleValue);
                            }
                        }
                    }
                }
                else
                {
                    string dropdownKey = string.Format("{0}-templatedropdown", attributePrefix);
                    string dropdownKeyValue = provider.GetValue(dropdownKey).ConvertTo(typeof(string)) as string;
                    if (dropdownKeyValue == "expression")
                    {
                        string key = attributePrefix;
                        return GetAndValidateValueProviderString(provider, key, oldValue, scriptLine, controller);
                    }
                    else
                    {
                        IEditorDefinition editorDefinition = controller.GetExpressionEditorDefinition(oldValue, templatesFilter);
                        IEditorData data = controller.GetExpressionEditorData(oldValue, templatesFilter, null);

                        foreach (IEditorControl ctl in editorDefinition.Controls.Where(c => c.Attribute != null))
                        {
                            string key = attributePrefix + "-" + ctl.Attribute;
                            object value = GetScriptParameterValue(scriptLine, controller, provider, key, ctl.ControlType, ctl.GetString("simpleeditor") ?? "textbox", null, null, ignoreExpression);
                            data.SetAttribute(ctl.Attribute, value);
                        }

                        return controller.GetExpression(data, null, null);
                    }
                }
            }
            else
            {
                string key = attributePrefix;
                return GetValueProviderString(provider, key);
            }
        }

        private string GetAndValidateValueProviderString(IValueProvider provider, string key, string oldValue, ElementSaveData.ScriptSaveData scriptLine, EditorController controller)
        {
            string result = GetValueProviderString(provider, key);
            ValidationResult validationResult = controller.ValidateExpression(result);
            if (!validationResult.Valid)
            {
                scriptLine.Error = string.Format("Could not set value '{0}' - {1}",
                    result,
                    Services.EditorService.GetValidationError(validationResult, result));
                result = oldValue;
            }
            return result;
        }

        private string GetValueProviderString(IValueProvider provider, string key)
        {
            try
            {
                ValueProviderResult value = provider.GetValue(key);
                return value == null ? null : (string)value.ConvertTo(typeof(string));
            }
            catch (HttpRequestValidationException)
            {
                // Workaround for "potentially dangerous" form values which may contain HTML
                Func<System.Collections.Specialized.NameValueCollection> formGetter;
                Func<System.Collections.Specialized.NameValueCollection> queryStringGetter;
                Microsoft.Web.Infrastructure.DynamicValidationHelper.ValidationUtility.GetUnvalidatedCollections(HttpContext.Current, out formGetter, out queryStringGetter);
                return formGetter().Get(key);
            }
        }

        private string StripHTMLComments(string input)
        {
            if (input == null) return null;
            return Regex.Replace(input, "<!--.*?-->", string.Empty, RegexOptions.Singleline);
        }
    }
}