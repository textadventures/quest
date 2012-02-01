using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebEditor.Services
{
    public class EditorService
    {
        private class TreeItem
        {
            public string Key { get; set; }
            public string Text { get; set; }
            public TreeItem Parent { get; set; }
        }

        private class ErrorData
        {
            public string Message { get; set; }
            public string Element { get; set; }
        }

        private EditorController m_controller;
        private Dictionary<string, TreeItem> m_elements = new Dictionary<string, TreeItem>();
        private int m_id;
        private Dictionary<string, ErrorData> m_scriptErrors = new Dictionary<string, ErrorData>();

        private static Dictionary<ValidationMessage, string> s_validationMessages = new Dictionary<ValidationMessage, string> {
		    {ValidationMessage.OK,"No error"},
		    {ValidationMessage.ItemAlreadyExists,"Item '{0}' already exists in the list"},
		    {ValidationMessage.ElementAlreadyExists,"An element called '{0}' already exists in this game"},
            {ValidationMessage.InvalidAttributeName, "Invalid attribute name"},
            {ValidationMessage.ExceptionOccurred, "An error occurred: {1}"},
            {ValidationMessage.InvalidElementName, "Invalid element name"},
            {ValidationMessage.CircularTypeReference, "Circular type reference"},
            {ValidationMessage.InvalidElementNameMultipleSpaces, "Invalid element name. An element name cannot start or end with a space, and cannot contain multiple consecutive spaces."},
            {ValidationMessage.InvalidElementNameInvalidWord, "Invalid element name. Elements cannot contain these words: " + string.Join(", ", EditorController.ExpressionKeywords)},
            {ValidationMessage.CannotRenamePlayerElement, "The player object cannot be renamed"},
        };

        public static string GetValidationError(ValidationResult result, string input)
        {
            return string.Format(s_validationMessages[result.Message], input, result.MessageData);
        }

        public EditorService()
        {
            m_controller = new EditorController();
        }

        public void Initialise(int id, string filename, string libFolder)
        {
            m_id = id;
            if (m_controller.Initialise(filename, libFolder))
            {
                m_controller.ClearTree += new EditorController.VoidHandler(m_controller_ClearTree);
                m_controller.BeginTreeUpdate += new EditorController.VoidHandler(m_controller_BeginTreeUpdate);
                m_controller.AddedNode += new EditorController.AddedNodeHandler(m_controller_AddedNode);
                m_controller.EndTreeUpdate += new EditorController.VoidHandler(m_controller_EndTreeUpdate);
                m_controller.UpdateTree();
            }
        }

        public EditorController Controller { get { return m_controller; } }

        void m_controller_AddedNode(string key, string text, string parent, bool isLibraryNode, int? position)
        {
            m_elements.Add(key, new TreeItem
            {
                Key = key,
                Text = text,
                Parent = (parent == null) ? null : m_elements[parent]
            });
        }

        void m_controller_ClearTree()
        {
            m_elements.Clear();
        }

        void m_controller_BeginTreeUpdate()
        {
            m_elements.Clear();
        }

        void m_controller_EndTreeUpdate()
        {
        }

        private class JsonTreeElement
        {
            public Dictionary<string, string> attr = new Dictionary<string, string>();
            public string data;
            //public string state;
            public IEnumerable<JsonTreeElement> children;
        }

        private class JsonParentElement
        {
            public IEnumerable<JsonTreeElement> data;
        }

        public object GetElementTreeForJson()
        {
            return new JsonParentElement { data = GetJsonTreeItemsForParent(null) };
        }

        private List<JsonTreeElement> GetJsonTreeItemsForParent(string parent)
        {
            List<JsonTreeElement> result = new List<JsonTreeElement>();
            TreeItem parentElement = (parent == null) ? null : m_elements[parent];
            foreach (TreeItem item in m_elements.Values.Where(e => e.Parent == parentElement))
            {
                JsonTreeElement modelTreeItem = new JsonTreeElement
                {
                    data = item.Text,
                    children = GetJsonTreeItemsForParent(item.Key)
                };
                modelTreeItem.attr.Add("data-key", item.Key);
                modelTreeItem.attr.Add("id", "tree-" + item.Key);
                result.Add(modelTreeItem);
            }
            return result;
        }

        public Models.Element GetElementModelForView(int gameId, string key, string tab, string error)
        {
            IEditorData data = m_controller.GetEditorData(key);
            IEditorDefinition def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(key));

            Dictionary<string, List<string>> otherElementErrors = new Dictionary<string, List<string>>();
            foreach (var scriptError in m_scriptErrors.Values.Where(e => e.Element != key))
            {
                if (!otherElementErrors.ContainsKey(scriptError.Element))
                {
                    otherElementErrors.Add(scriptError.Element, new List<string>());
                }
                otherElementErrors[scriptError.Element].Add(scriptError.Message);
            }

            return new Models.Element
            {
                GameId = gameId,
                Key = key,
                Name = m_controller.GetDisplayName(key),
                EditorData = data,
                EditorDefinition = def,
                Tab = tab,
                Error = error,
                OtherElementErrors = otherElementErrors,
                Controller = m_controller
            };
        }

        public string SaveElement(string key, Models.ElementSaveData saveData)
        {
            try
            {
                IEditorData data = m_controller.GetEditorData(key);
                foreach (var kvp in saveData.Values)
                {
                    object currentValue = data.GetAttribute(kvp.Key);
                    IEditableScripts script = currentValue as IEditableScripts;
                    if (script != null)
                    {
                        SaveScript(script, kvp.Value as WebEditor.Models.ElementSaveData.ScriptsSaveData, key);
                    }
                    else
                    {
                        if (DataChanged(currentValue, (kvp.Value)))
                        {
                            System.Diagnostics.Debug.WriteLine("New value for {0}: Was {1}, now {2}", kvp.Key, data.GetAttribute(kvp.Key), kvp.Value);
                            data.SetAttribute(kvp.Key, kvp.Value);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(saveData.AdditionalAction))
                {
                    ProcessAdditionalAction(key, saveData.AdditionalAction);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private bool DataChanged(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return false;
            }
            if (oldValue == null && newValue is string)
            {
                return ((string)newValue).Length > 0;
            }
            if (oldValue == null && newValue is bool)
            {
                return (bool)newValue;
            }
            if (oldValue is string && newValue is string)
            {
                return (string)oldValue != (string)newValue;
            }
            if (oldValue is bool && newValue is bool)
            {
                return (bool)oldValue != (bool)newValue;
            }
            if (oldValue is int && newValue is int)
            {
                return (int)oldValue != (int)newValue;
            }
            if (newValue is WebEditor.Models.IgnoredValue)
            {
                return false;
            }
            if (oldValue == null && newValue is WebEditor.Models.ElementSaveData.ScriptsSaveData)
            {
                if (((WebEditor.Models.ElementSaveData.ScriptsSaveData)newValue).ScriptLines.Count == 0)
                {
                    return false;
                }
            }
            throw new NotImplementedException();
        }

        private void SaveScript(IEditableScripts scripts, WebEditor.Models.ElementSaveData.ScriptsSaveData saveData, string parentElement)
        {
            int count = 0;
            foreach (IEditableScript script in scripts.Scripts)
            {
                WebEditor.Models.ElementSaveData.ScriptSaveData data = saveData.ScriptLines[count];

                if (script.Type != ScriptType.If)
                {
                    IEditorData scriptEditorData = m_controller.GetScriptEditorData(script);
                    foreach (var attribute in data.Attributes)
                    {
                        object oldValue = scriptEditorData.GetAttribute(attribute.Key);

                        if (oldValue is IEditableScripts)
                        {
                            SaveScript((IEditableScripts)oldValue, (WebEditor.Models.ElementSaveData.ScriptsSaveData)data.Attributes[attribute.Key], parentElement);
                        }
                        else if (oldValue is IEditableDictionary<IEditableScripts>)
                        {
                            IEditableDictionary<IEditableScripts> dictionary = (IEditableDictionary<IEditableScripts>)oldValue;
                            WebEditor.Models.ElementSaveData.ScriptSaveData newData = (WebEditor.Models.ElementSaveData.ScriptSaveData)attribute.Value;
                            Dictionary<string, string> keysToChange = new Dictionary<string, string>();
                            int dictionaryCount = 0;
                            foreach (var item in dictionary.Items)
                            {
                                string newKey = (string)newData.Attributes[string.Format("key{0}", dictionaryCount)];
                                if (item.Key != newKey)
                                {
                                    // Can't change dictionary keys while enumerating dictionary items - so
                                    // change them afterwards
                                    keysToChange.Add(item.Key, newKey);
                                }

                                SaveScript(item.Value.Value, (WebEditor.Models.ElementSaveData.ScriptsSaveData)newData.Attributes[string.Format("value{0}", dictionaryCount)], parentElement);

                                dictionaryCount++;
                            }

                            foreach (var item in keysToChange)
                            {
                                ValidationResult result = dictionary.CanAdd(item.Value);
                                if (result.Valid)
                                {
                                    dictionary.ChangeKey(item.Key, item.Value);
                                }
                                else
                                {
                                    AddScriptError(parentElement, script, GetValidationError(result, item.Value));
                                }
                            }
                        }
                        else
                        {
                            object newValue = attribute.Value;
                            if (DataChanged(oldValue, newValue))
                            {
                                System.Diagnostics.Debug.WriteLine("New value for script: Was {0}, now {1}", oldValue, newValue);
                                script.SetParameter(attribute.Key, newValue);
                            }
                        }
                    }
                }
                else
                {
                    EditableIfScript ifScript = (EditableIfScript)script;
                    object oldExpression = ifScript.GetAttribute("expression");
                    object newExpression = data.Attributes["expression"];
                    if (DataChanged(oldExpression, newExpression))
                    {
                        ifScript.SetAttribute("expression", newExpression);
                    }

                    SaveScript(ifScript.ThenScript, (WebEditor.Models.ElementSaveData.ScriptsSaveData)data.Attributes["then"], parentElement);

                    int elseIfCount = 0;
                    foreach (EditableIfScript.EditableElseIf elseIfScript in ifScript.ElseIfScripts)
                    {
                        object oldElseIfExpression = elseIfScript.GetAttribute("expression");
                        object newElseIfExpression = data.Attributes[string.Format("elseif{0}-expression", elseIfCount)];
                        if (DataChanged(oldElseIfExpression, newElseIfExpression))
                        {
                            elseIfScript.SetAttribute("expression", newElseIfExpression);
                        }

                        SaveScript(elseIfScript.EditableScripts, (WebEditor.Models.ElementSaveData.ScriptsSaveData)data.Attributes[string.Format("elseif{0}-then", elseIfCount)], parentElement);
                        elseIfCount++;
                    }

                    if (ifScript.ElseScript != null)
                    {
                        SaveScript(ifScript.ElseScript, (WebEditor.Models.ElementSaveData.ScriptsSaveData)data.Attributes["else"], parentElement);
                    }
                }

                count++;
            }
        }

        public Models.StringList GetStringListModel(string key, IEditorControl ctl)
        {
            IEditableList<string> value = GetStringList(key, ctl.Attribute, false);
            return GetStringListModel(value, ctl, ctl.Attribute);
        }

        public Models.StringList GetScriptStringListModel(string key, string path, IEditorControl ctl)
        {
            string parameter;
            IEditableScript scriptLine = GetScriptLine(key, path, out parameter);
            IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
            IEditableList<string> value = (IEditableList<string>)scriptEditorData.GetAttribute(parameter);
            return GetStringListModel(value, ctl, path);
        }

        private Models.StringList GetStringListModel(IEditableList<string> value, IEditorControl ctl, string attribute)
        {
            IDictionary<string, string> items = null;
            if (value != null)
            {
                items = new Dictionary<string, string>();
                foreach (var item in value.ItemsList)
                {
                    items.Add(item.Key, item.Value);
                }
            }

            return new Models.StringList
            {
                Attribute = attribute,
                EditPrompt = ctl.GetString("editprompt"),
                Items = items
            };
        }

        public Models.Script GetScript(int id, string key, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState)
        {
            IEditableScripts value = (IEditableScripts)m_controller.GetEditorData(key).GetAttribute(ctl.Attribute);

            foreach (var error in m_scriptErrors)
            {
                modelState.AddModelError(error.Key, error.Value.Message);
            }

            return new Models.Script
            {
                GameId = id,
                Key = key,
                Attribute = ctl.Attribute,
                Controller = m_controller,
                Scripts = value
            };
        }

        private void ProcessAdditionalAction(string key, string arguments)
        {
            string[] data = arguments.Split(new[] { ' ' }, 3);
            string action = data[0];
            string cmd = data[1];
            string parameter = data[2];
            switch (action)
            {
                case "stringlist":
                    ProcessStringListAction(key, cmd, parameter);
                    break;
                case "script":
                    ProcessScriptAction(key, cmd, parameter);
                    break;
                case "scriptdictionary":
                    ProcessScriptDictionaryAction(key, cmd, parameter);
                    break;
                case "error":
                    ProcessErrorAction(key, cmd, parameter);
                    break;
                case "multi":
                    ProcessMultiAction(key, cmd, parameter);
                    break;
                case "types":
                    ProcessTypesAction(key, cmd, parameter);
                    break;
            }
        }

        private void ProcessStringListAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "add":
                    data = parameter.Split(new[] { ';' }, 2);
                    StringListAdd(key, data[0], data[1]);
                    break;
                case "edit":
                    data = parameter.Split(new[] { ';' }, 3);
                    StringListEdit(key, data[0], data[1], data[2]);
                    break;
                case "delete":
                    data = parameter.Split(new[] { ';' }, 2);
                    StringListDelete(key, data[0], data[1]);
                    break;
            }
        }

        private void ProcessScriptAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "add":
                    data = parameter.Split(new[] { ';' }, 2);
                    ScriptAdd(key, data[0], data[1]);
                    break;
                case "delete":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptDelete(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "addelse":
                    ScriptAddElse(key, parameter);
                    break;
                case "addelseif":
                    ScriptAddElseIf(key, parameter);
                    break;
                case "deleteifsection":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptDeleteIfSection(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "settemplate":
                    data = parameter.Split(new[] { ';' }, 2);
                    ScriptSetTemplate(key, data[0], data[1]);
                    break;
            }
        }

        private void ProcessScriptDictionaryAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "add":
                    data = parameter.Split(new[] { ';' }, 2);
                    ScriptDictionaryAdd(key, data[0], data[1]);
                    break;
                case "delete":
                    data = parameter.Split(new[] { ';' }, 2);
                    ScriptDictionaryDelete(key, data[0], data[1]);
                    break;
            }
        }

        private void ProcessErrorAction(string key, string cmd, string parameter)
        {
            switch (cmd)
            {
                case "clear":
                    ErrorClear(parameter);
                    break;
            }
        }

        private void ProcessMultiAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "set":
                    data = parameter.Split(new[] { ';' }, 2);
                    MultiSet(key, data[0], data[1]);
                    break;
            }
        }

        private void ProcessTypesAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "set":
                    data = parameter.Split(new[] { ';' }, 2);
                    TypesSet(key, data[0], data[1]);
                    break;
            }
        }

        private void TypesSet(string element, string attribute, string value)
        {
            const string k_noType = "*";

            IEditorControl ctl = FindEditorControl(element, attribute);
            string oldType = m_controller.GetSelectedDropDownType(ctl, element);
            if (value == oldType) return;

            IDictionary<string, string> types = ctl.GetDictionary("types");

            m_controller.StartTransaction(String.Format("Change type from '{0}' to '{1}'", types[oldType], types[value]));

            if (oldType != k_noType)
            {
                m_controller.RemoveInheritedTypeFromElement(element, oldType, false);
            }

            if (value != k_noType)
            {
                m_controller.AddInheritedTypeToElement(element, value, false);
            }

            m_controller.EndTransaction();
        }

        private IEditorControl FindEditorControl(string element, string controlId)
        {
            IEditorDefinition def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(element));
            foreach (IEditorTab tab in def.Tabs.Values)
            {
                IEditorControl ctl = tab.Controls.FirstOrDefault(c => c.Id == controlId);
                if (ctl != null) return ctl;
            }
            return null;
        }

        private void MultiSet(string element, string attribute, string value)
        {
            m_controller.StartTransaction(string.Format("Change type of '{0}' {1} to '{2}'", element, attribute, value));

            object newValue;

            switch (value)
            {
                case "boolean":
                    newValue = false;
                    break;
                case "string":
                    newValue = "";
                    break;
                case "int":
                    newValue = 0;
                    break;
                case "script":
                    newValue = m_controller.CreateNewEditableScripts(element, attribute, null, false);
                    break;
                case "stringlist":
                    newValue = m_controller.CreateNewEditableList(element, attribute, null, false);
                    break;
                case "object":
                    newValue = m_controller.CreateNewEditableObjectReference(element, attribute, false);
                    break;
                case "simplepattern":
                    newValue = m_controller.CreateNewEditableCommandPattern(element, attribute, "", false);
                    break;
                case "stringdictionary":
                    newValue = m_controller.CreateNewEditableStringDictionary(element, attribute, null, null, false);
                    break;
                case "scriptdictionary":
                    newValue = m_controller.CreateNewEditableScriptDictionary(element, attribute, null, null, false);
                    break;
                case "null":
                    newValue = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            IEditorData data = m_controller.GetEditorData(element);

            var result = data.SetAttribute(attribute, newValue);

            if (!result.Valid)
            {
                // TO DO: Add error - but when can this happen?
                //PopupEditors.DisplayValidationError(result, newValue as string, "Unable to set attribute value");
                throw new NotImplementedException();
            }

            m_controller.EndTransaction();
        }

        private void ErrorClear(string id)
        {
            if (m_scriptErrors.ContainsKey(id))
            {
                m_scriptErrors.Remove(id);
            }
        }

        private void StringListAdd(string element, string attribute, string value)
        {
            // TO DO: if (m_data.ReadOnly) return;
            // TO DO: Validate input first

            IEditableList<string> list = GetStringList(element, attribute, true);
            if (list == null)
            {
                list = m_controller.CreateNewEditableList(element, attribute, value, true);
            }
            else
            {
                list.Add(value);
            }
        }

        private void StringListEdit(string element, string attribute, string key, string value)
        {
            // TO DO: if (m_data.ReadOnly) return;
            // TO DO: Validate input first

            IEditableList<string> list = GetStringList(element, attribute, true);
            list.Update(key, value);
        }

        private void StringListDelete(string element, string attribute, string key)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableList<string> list = GetStringList(element, attribute, true);
            list.Remove(key);
        }

        private void PrepareStringListForEditing(string element, string attribute, ref IEditableList<string> list)
        {
            // If we're currently displaying a list which belongs to a type we inherit from,
            // we must clone the list before we can edit it.

            if (list == null) return;
            if (list.Owner != element)
            {
                list = list.Clone(element, attribute);
            }
        }

        private void ScriptAdd(string element, string attribute, string value)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript parent;
            string parameter;
            IEditableScripts script = GetScript(element, attribute, out parent, out parameter);

            if (script == null)
            {
                if (parent == null)
                {
                    m_controller.CreateNewEditableScripts(element, attribute, value, true, true);
                }
                else
                {
                    ScriptCommandEditorData editorData = (ScriptCommandEditorData)m_controller.GetScriptEditorData(parent);
                    m_controller.CreateNewEditableScriptsChild(editorData, parameter, value, true);
                }
            }
            else
            {
                script.AddNew(value, element);
            }
        }

        private void ScriptDelete(string element, string attribute, string[] indexes)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScripts script = GetScript(element, attribute);
            script.Remove(indexes.Select(i => int.Parse(i)).ToArray());
        }

        private void ScriptAddElse(string element, string attribute)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript scriptLine = GetScriptLine(element, attribute);
            EditableIfScript ifScript = (EditableIfScript)scriptLine;
            ifScript.AddElse();
        }

        private void ScriptAddElseIf(string element, string attribute)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript scriptLine = GetScriptLine(element, attribute);
            EditableIfScript ifScript = (EditableIfScript)scriptLine;
            ifScript.AddElseIf();
        }

        private void ScriptDeleteIfSection(string element, string attribute, string[] sections)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript scriptLine = GetScriptLine(element, attribute);
            EditableIfScript ifScript = (EditableIfScript)scriptLine;

            List<EditableIfScript.EditableElseIf> elseIfsToRemove = new List<EditableIfScript.EditableElseIf>();

            foreach (string section in sections)
            {
                if (section == "else")
                {
                    ifScript.RemoveElse();
                }
                else if(section.StartsWith("elseif"))
                {
                    elseIfsToRemove.Add(ifScript.ElseIfScripts.ElementAt(int.Parse(section.Substring(6))));
                }
            }

            foreach (EditableIfScript.EditableElseIf elseIfToRemove in elseIfsToRemove)
            {
                ifScript.RemoveElseIf(elseIfToRemove);
            }
        }

        private void ScriptSetTemplate(string element, string attribute, string template)
        {
            string newExpression = m_controller.GetNewExpression(template);

            string[] path = attribute.Split('-');

            // final part of attribute path will be the name of the expression parameter. The one before
            // that will give us the script line and section (e.g. "then", "else if")

            string scriptLinePath = string.Join("-", path.Take(path.Length - 1));
            string parameter = path[path.Length - 1];

            string sectionParameter;
            IEditableScript scriptLine = GetScriptLine(element, scriptLinePath, out sectionParameter);
            if (sectionParameter == null)
            {
                EditableIfScript ifScript = scriptLine as EditableIfScript;
                if (ifScript != null)
                {
                    ifScript.SetAttribute(parameter, newExpression);
                }
                else
                {
                    scriptLine.SetParameter(parameter, newExpression);
                }
            }
            else if (sectionParameter.StartsWith("elseif"))
            {
                int elseIfIndex = int.Parse(sectionParameter.Substring(6));
                EditableIfScript ifScript = (EditableIfScript)scriptLine;
                ifScript.ElseIfScripts.ElementAt(elseIfIndex).SetAttribute(parameter, newExpression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ScriptDictionaryAdd(string element, string attribute, string value)
        {
            IEditableScript scriptLine;
            IEditableDictionary<IEditableScripts> dictionary = GetScriptDictionary(element, attribute, out scriptLine);
            ValidationResult result = dictionary.CanAdd(value);
            if (result.Valid)
            {
                dictionary.Add(value, m_controller.CreateNewEditableScripts(null, null, null, true));
            }
            else
            {
                AddScriptError(element, scriptLine, GetValidationError(result, value));
            }
        }

        private void ScriptDictionaryDelete(string element, string attribute, string value)
        {
            IEditableDictionary<IEditableScripts> dictionary = GetScriptDictionary(element, attribute);
            // value is a semicolon-separated list of indexes
            string[] keys = value.Split(';').Select(i => dictionary.Items.ElementAt(int.Parse(i)).Key).ToArray();
            dictionary.Remove(keys);
        }

        private IEditableDictionary<IEditableScripts> GetScriptDictionary(string element, string attribute)
        {
            IEditableScript scriptLine;
            return GetScriptDictionary(element, attribute, out scriptLine);
        }

        private IEditableDictionary<IEditableScripts> GetScriptDictionary(string element, string attribute, out IEditableScript scriptLine)
        {
            string parameter;
            scriptLine = GetScriptLine(element, attribute, out parameter);
            IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
            return (IEditableDictionary<IEditableScripts>)scriptEditorData.GetAttribute(parameter);
        }

        private IEditableList<string> GetStringList(string element, string attribute, bool prepareForEditing)
        {
            if (attribute.Contains('-'))
            {
                string parameter;
                IEditableScript scriptLine = GetScriptLine(element, attribute, out parameter);
                IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
                return (IEditableList<string>)scriptEditorData.GetAttribute(parameter);
            }
            else
            {
                IEditableList<string> result = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableList<string>;
                if (prepareForEditing)
                {
                    PrepareStringListForEditing(element, attribute, ref result);
                }
                return result;
            }
        }

        private void AddScriptError(string element, IEditableScript script, string error)
        {
            if (!m_scriptErrors.ContainsKey(script.Id))
            {
                m_scriptErrors.Add(script.Id, new ErrorData { Message = error, Element = element });
            }
            else
            {
                m_scriptErrors[script.Id].Message = m_scriptErrors[script.Id].Message + ". " + error;
            }
        }

        private IEditableScripts GetScript(string element, string attribute)
        {
            IEditableScript parent;
            string parameter;
            return GetScript(element, attribute, out parent, out parameter);
        }

        private IEditableScripts GetScript(string element, string attribute, out IEditableScript parent, out string parameter)
        {
            IEditableScripts script;
            parent = null;
            parameter = null;

            if (attribute.IndexOf("-") == -1)
            {
                script = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableScripts;
            }
            else
            {
                IEditableScript scriptLine = GetScriptLine(element, attribute, out parameter);
                if (parameter == "then")
                {
                    EditableIfScript ifScript = (EditableIfScript)scriptLine;
                    script = ifScript.ThenScript;
                }
                else if (parameter == "else")
                {
                    EditableIfScript ifScript = (EditableIfScript)scriptLine;
                    script = ifScript.ElseScript;
                }
                else if (parameter.StartsWith("elseif"))
                {
                    int elseIfIndex = int.Parse(parameter.Substring(6));
                    EditableIfScript ifScript = (EditableIfScript)scriptLine;
                    script = ifScript.ElseIfScripts.ElementAt(elseIfIndex).EditableScripts;
                }
                else
                {
                    parent = scriptLine;
                    IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
                    if (parameter.Contains('-'))
                    {
                        string[] path = parameter.Split('-');
                        IEditableDictionary<IEditableScripts> dictionary = (IEditableDictionary<IEditableScripts>)scriptEditorData.GetAttribute(path[0]);
                        if (path[1].StartsWith("value"))
                        {
                            return dictionary.Items.ElementAt(int.Parse(path[1].Substring(5))).Value.Value;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        return (IEditableScripts)scriptEditorData.GetAttribute(parameter);
                    }
                }
            }

            return script;
        }

        private IEditableScript GetScriptLine(string element, string attribute)
        {
            string parameter;
            return GetScriptLine(element, attribute, out parameter);
        }

        private IEditableScript GetScriptLine(string element, string attribute, out string parameter)
        {
            string[] path = attribute.Split(new[] { '-' });
            IEditableScripts parent = m_controller.GetEditorData(element).GetAttribute(path[0]) as IEditableScripts;
            return GetScriptLine(path, parent, out parameter);
        }

        private IEditableScript GetScriptLine(string[] path, IEditableScripts parent, out string parameter)
        {
            IEditableScript scriptLine = parent.Scripts.ElementAt(int.Parse(path[1]));
            int pathSectionsToRemove = 3;
            if (path.Length == 2)
            {
                parameter = null;
            }
            else if (path.Length == 3)
            {
                parameter = path[2];
            }
            else
            {
                IEditableScripts childScript;

                EditableIfScript ifScript = scriptLine as EditableIfScript;
                if (ifScript != null)
                {
                    if (path[2] == "then")
                    {
                        childScript = ifScript.ThenScript;
                    }
                    else if (path[2].StartsWith("elseif"))
                    {
                        childScript = ifScript.ElseIfScripts.ElementAt(int.Parse(path[2].Substring(6))).EditableScripts;
                    }
                    else if (path[2] == "else")
                    {
                        childScript = ifScript.ElseScript;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
                    object attribute = scriptEditorData.GetAttribute(path[2]);
                    IEditableScripts scriptAttribute = attribute as IEditableScripts;
                    IEditableDictionary<IEditableScripts> dictionaryAttribute = attribute as IEditableDictionary<IEditableScripts>;
                    if (scriptAttribute != null)
                    {
                        childScript = scriptAttribute;
                    }
                    else
                    {
                        if (path[3].StartsWith("value"))
                        {
                            if (path.Length == 4)
                            {
                                parameter = path[2] + "-" + path[3];
                                return scriptLine;
                            }
                            else
                            {
                                pathSectionsToRemove = 4;
                                childScript = dictionaryAttribute.Items.ElementAt(int.Parse(path[3].Substring(5))).Value.Value;
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }

                List<string> newPath = new List<string>(path);
                newPath.RemoveRange(0, pathSectionsToRemove - 1);
                scriptLine = GetScriptLine(newPath.ToArray(), childScript, out parameter);
            }
            return scriptLine;
        }

        private class ScriptAdderCategory
        {
            public List<ScriptAdderItem> items = new List<ScriptAdderItem>();
        }

        private class ScriptAdderItem
        {
            public string display;
            public string create;
        }

        public object GetScriptAdderJson()
        {
            Dictionary<string, ScriptAdderCategory> categories = new Dictionary<string, ScriptAdderCategory>();
            foreach (string cat in m_controller.GetAllScriptEditorCategories())
            {
                categories.Add(cat, new ScriptAdderCategory());
            }

            foreach (EditableScriptData data in m_controller.GetScriptEditorData().Values)
            {
                categories[data.Category].items.Add(new ScriptAdderItem
                {
                    display = data.AdderDisplayString,
                    create = data.CreateString
                });
            }

            return categories;
        }
    }
}