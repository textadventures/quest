using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;
using System.Configuration;
using WebInterfaces;

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

        public struct InitialiseResult
        {
            public bool Success;
            public string Error;
        }

        private EditorController m_controller;
        private Dictionary<string, TreeItem> m_elements = new Dictionary<string, TreeItem>();
        private int m_id;
        private Dictionary<string, ErrorData> m_scriptErrors = new Dictionary<string, ErrorData>();
        private Dictionary<string, Dictionary<string, string>> m_elementErrors = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, ErrorData> m_dictionaryErrors = new Dictionary<string, ErrorData>();
        private bool m_mustRefreshTree = false;
        private string m_popupError = null;
        private bool m_canUndo = false;
        private bool m_canRedo = false;
        private bool m_createInverse = true;
        private bool m_needsSaving = false;
        private string m_uiAction = null;
        private string m_initMessage = null;
        private List<IEditableScript> m_selectedScripts = new List<IEditableScript>();

        public static string GetValidationError(ValidationResult result, object input)
        {
            return EditorController.GetValidationError(result, input);
        }

        public EditorService()
        {
            m_controller = new EditorController();
            m_controller.EditorMode = EditorMode.Web;
        }

        public InitialiseResult Initialise(int id, string filename, string libFolder, bool simpleMode)
        {
            InitialiseResult result = new InitialiseResult();
            m_id = id;
            m_controller.SimpleMode = simpleMode;
            m_controller.ShowMessage += m_controller_ShowMessage;

            if (m_controller.Initialise(filename, libFolder))
            {
                m_controller.ClearTree += m_controller_ClearTree;
                m_controller.BeginTreeUpdate += m_controller_BeginTreeUpdate;
                m_controller.AddedNode += m_controller_AddedNode;
                m_controller.RemovedNode += m_controller_RemovedNode;
                m_controller.RenamedNode += m_controller_RenamedNode;
                m_controller.RetitledNode += m_controller_RetitledNode;
                m_controller.EndTreeUpdate += m_controller_EndTreeUpdate;
                m_controller.UndoListUpdated += m_controller_UndoListUpdated;
                m_controller.RedoListUpdated += m_controller_RedoListUpdated;
                m_controller.UpdateTree();
                result.Success = true;
            }
            else
            {
                result.Success = false;
                result.Error = m_initMessage;
            }

            return result;
        }

        void m_controller_ShowMessage(object sender, TextAdventures.Quest.EditorController.ShowMessageEventArgs e)
        {
            m_initMessage = e.Message;
        }

        public EditorController Controller { get { return m_controller; } }

        void m_controller_AddedNode(object sender, TextAdventures.Quest.EditorController.AddedNodeEventArgs e)
        {
            if (m_elements.ContainsKey(e.Key)) return;
            if (e.Parent != null && !m_elements.ContainsKey(e.Parent)) return;
            m_elements.Add(e.Key, new TreeItem
            {
                Key = e.Key,
                Text = e.Text,
                Parent = (e.Parent == null) ? null : m_elements[e.Parent]
            });
            m_mustRefreshTree = true;
        }

        void m_controller_RemovedNode(object sender, TextAdventures.Quest.EditorController.RemovedNodeEventArgs e)
        {
            List<string> keysToRemove = new List<string>();
            PopulateTreeChildrenList(e.Key, keysToRemove);
            foreach (string removeKey in keysToRemove)
            {
                m_elements.Remove(removeKey);
            }
            m_mustRefreshTree = true;
        }

        private void PopulateTreeChildrenList(string key, List<string> keys)
        {
            keys.Add(key);
            foreach (string childKey in from treeItem in m_elements
                                        where treeItem.Value.Parent != null && treeItem.Value.Parent.Key == key
                                        select treeItem.Key)
            {
                PopulateTreeChildrenList(childKey, keys);
            }
        }

        void m_controller_RenamedNode(object sender, TextAdventures.Quest.EditorController.RenamedNodeEventArgs e)
        {
            TreeItem item = m_elements[e.OldName];
            m_elements.Remove(e.OldName);
            item.Key = e.NewName;
            item.Text = e.NewName;
            m_elements.Add(e.NewName, item);

            m_mustRefreshTree = true;
        }

        void m_controller_RetitledNode(object sender, TextAdventures.Quest.EditorController.RetitledNodeEventArgs e)
        {
            if (!m_elements.ContainsKey(e.Key)) return;
            m_elements[e.Key].Text = e.NewTitle;
            m_mustRefreshTree = true;
        }

        void m_controller_ClearTree(object sender, EventArgs e)
        {
            m_elements.Clear();
        }

        void m_controller_BeginTreeUpdate(object sender, EventArgs e)
        {
        }

        void m_controller_EndTreeUpdate(object sender, EventArgs e)
        {
        }

        void m_controller_UndoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
        {
            m_canUndo = e.UndoList.Any();
        }

        void m_controller_RedoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
        {
            m_canRedo = e.UndoList.Any();
        }

        private class JsonTreeElement
        {
            public Dictionary<string, string> attr = new Dictionary<string, string>();
            public string data;
            public IEnumerable<JsonTreeElement> children;
        }

        private class JsonParentElement
        {
            public IEnumerable<JsonTreeElement> data;
        }

        public object GetElementTreeForJson()
        {
            m_mustRefreshTree = false;
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
                modelTreeItem.attr.Add("id", "tree-" + item.Key.Replace(" ", "-"));
                result.Add(modelTreeItem);
            }
            return result;
        }

        public Models.Element GetElementModelForView(int gameId, string key)
        {
            return GetElementModelForView(gameId, key, null, null, null, null);
        }

        public Models.Element GetElementModelForView(int gameId, string key, string tab, string error, string refreshTreeSelectElement, System.Web.Mvc.ModelStateDictionary modelState)
        {
            IEditorData data = null;
            IEditorDefinition def = null;
            Dictionary<string, List<string>> otherElementErrors = new Dictionary<string, List<string>>();

            if (refreshTreeSelectElement == null && !m_mustRefreshTree)
            {
                data = m_controller.GetEditorData(key);
                def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(key));

                foreach (var scriptError in m_scriptErrors.Values.Where(e => e.Element != key))
                {
                    if (!otherElementErrors.ContainsKey(scriptError.Element))
                    {
                        otherElementErrors.Add(scriptError.Element, new List<string>());
                    }
                    otherElementErrors[scriptError.Element].Add(scriptError.Message);
                }

                foreach (var elementError in m_elementErrors.Where(e => e.Key != key && e.Value.Count > 0))
                {
                    if (!otherElementErrors.ContainsKey(elementError.Key))
                    {
                        otherElementErrors.Add(elementError.Key, new List<string>());
                    }
                    otherElementErrors[elementError.Key].AddRange(elementError.Value.Values);
                }

                foreach (var dictionaryError in m_dictionaryErrors.Values.Where(e => e.Element != key))
                {
                    if (!otherElementErrors.ContainsKey(dictionaryError.Element))
                    {
                        otherElementErrors.Add(dictionaryError.Element, new List<string>());
                    }
                    otherElementErrors[dictionaryError.Element].Add(dictionaryError.Message);
                }
            }

            if (m_mustRefreshTree && refreshTreeSelectElement == null)
            {
                refreshTreeSelectElement = key;
            }

            if (modelState != null && m_elementErrors.ContainsKey(key))
            {
                foreach (var errorData in m_elementErrors[key])
                {
                    modelState.AddModelError(errorData.Key, errorData.Value);
                }
            }

            string popupError = null;
            string uiAction = null;
            if (refreshTreeSelectElement == null)
            {
                popupError = m_popupError;
                m_popupError = null;

                uiAction = m_uiAction;
                m_uiAction = null;
            }

            IEnumerable<string> newObjectPossibleParents = m_controller.GetPossibleNewObjectParentsForCurrentSelection(key);
            IEnumerable<string> movePossibleParents = m_controller.GetMovePossibleParents(key);
            IEnumerable<string> allObjects = m_controller.GetObjectNames("object").OrderBy(n => n);

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
                Controller = m_controller,
                RefreshTreeSelectElement = refreshTreeSelectElement,
                PopupError = popupError,
                NewObjectPossibleParents = (newObjectPossibleParents == null) ? null : string.Join(";", newObjectPossibleParents),
                EnabledButtons = GetEnabledButtons(key),
                PageTitle = "Quest - " + m_controller.GameName,
                AvailableVerbs = string.Join("~", m_controller.GetVerbProperties().Values),
                UIAction = uiAction,
                CanMove = m_controller.CanMoveElement(key),
                MovePossibleParents = (movePossibleParents == null) ? null : string.Join(";", movePossibleParents),
                IsElement = m_controller.ElementExists(key),
                AllObjects = string.Join(";", allObjects),
                NextPage = (m_controller.EditorStyle == EditorStyle.GameBook) ? m_controller.GetUniqueElementName("Page1") : null,
                HiddenScripts = GetHiddenScripts(),
                ScriptCategories = GetScriptCategories(),
            };
        }

        public struct SaveElementResult
        {
            public string Error;
            public string RefreshTreeSelectElement;
        }

        public SaveElementResult SaveElement(string key, Models.ElementSaveData saveData)
        {
            string elementName = key;
            SaveElementResult result = new SaveElementResult();
            try
            {
                m_controller.StartTransaction("Edit");
                IEditorData data = m_controller.GetEditorData(key);
                foreach (var kvp in saveData.Values)
                {
                    object currentValue = data.GetAttribute(kvp.Key);
                    IEditableScripts script = currentValue as IEditableScripts;
                    var scriptDictionary = currentValue as IEditableDictionary<IEditableScripts>;
                    var stringDictionary = currentValue as IEditableDictionary<string>;
                    var newObjectRef = kvp.Value as WebEditor.Models.ElementSaveData.ObjectReferenceSaveData;
                    var newCommandPattern = kvp.Value as WebEditor.Models.ElementSaveData.PatternSaveData;
                    if (script != null)
                    {
                        SaveScript(script, kvp.Value as WebEditor.Models.ElementSaveData.ScriptsSaveData, key);
                    }
                    else if (scriptDictionary != null)
                    {
                        SaveScriptDictionary(key, null, scriptDictionary, kvp.Value as WebEditor.Models.ElementSaveData.ScriptSaveData);
                    }
                    else if (stringDictionary != null)
                    {
                        SaveStringDictionary(key, stringDictionary, kvp.Value as WebEditor.Models.ElementSaveData.ScriptSaveData);
                    }
                    else if (newObjectRef != null)
                    {
                        SaveObjectReference(data, kvp.Key, currentValue as IEditableObjectReference, newObjectRef);
                    }
                    else if (newCommandPattern != null)
                    {
                        SaveCommandPattern(data, kvp.Key, currentValue as IEditableCommandPattern, newCommandPattern);
                    }
                    else
                    {
                        if (DataChanged(currentValue, (kvp.Value)))
                        {
                            System.Diagnostics.Debug.WriteLine("New value for {0}: Was {1}, now {2}", kvp.Key, data.GetAttribute(kvp.Key), kvp.Value);
                            m_needsSaving = true;
                            ValidationResult validationResult = data.SetAttribute(kvp.Key, kvp.Value);
                            if (validationResult.Valid)
                            {
                                ErrorClear(key, kvp.Key);
                                // Element has been renamed
                                if (kvp.Key == "name") elementName = (string)kvp.Value;
                                if (saveData.RedirectToElement == key) saveData.RedirectToElement = elementName;
                            }
                            else
                            {
                                AddElementError(key, kvp.Key, GetValidationError(validationResult, kvp.Value));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log.ErrorFormat("Error in SaveElement: {0}", ex);
                result.Error = ex.Message;
                return result;
            }
            finally
            {
                m_controller.EndTransaction();
            }

            // Any additional actions (creating new objects etc.) occur under a separate transaction
            // to the main element saving transaction

            try
            {

                if (!string.IsNullOrEmpty(saveData.AdditionalAction))
                {
                    var actionResult = ProcessAdditionalAction(elementName, saveData.AdditionalAction);
                    result.RefreshTreeSelectElement = actionResult.RefreshTreeSelectElement;
                }
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                return result;
            }
            finally
            {
                // TO DO: Review this. May not be efficient to save the entire file each time - may want
                // to save every minute etc. But if so, will also need to consider saving on session timeout,
                // before loading a game (i.e. if user presses browser Refresh button, don't want to lose
                // changes), and before playing.

                if (m_needsSaving)
                {
                    FileManagerLoader.GetFileManager().SaveFile(m_id, m_controller.Save());
                    m_needsSaving = false;
                }
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
            if (oldValue == null && newValue is int)
            {
                return (int)newValue != 0;
            }
            if (oldValue == null && newValue is double)
            {
                return (double)newValue != 0;
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
            if (oldValue is double && newValue is double)
            {
                return (double)oldValue != (double)newValue;
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
            if (oldValue == null && newValue is WebEditor.Models.ElementSaveData.ScriptSaveData)
            {
                return false;
            }
            throw new NotImplementedException(string.Format("Unhandled data types in call to DataChanged. 1: {0} ({1}), 2: {2} ({3})",
                oldValue ?? "null",
                oldValue == null ? "null" : oldValue.GetType().ToString(),
                newValue ?? "null",
                newValue == null ? "null" : newValue.GetType().ToString()));
        }

        private void SaveScript(IEditableScripts scripts, WebEditor.Models.ElementSaveData.ScriptsSaveData saveData, string parentElement)
        {
            int count = 0;
            foreach (IEditableScript script in scripts.Scripts)
            {
                WebEditor.Models.ElementSaveData.ScriptSaveData data = saveData.ScriptLines[count];

                if (data.Error != null)
                {
                    AddScriptError(parentElement, script, data.Error);
                }

                if (data.IsSelected)
                {
                    if (!m_selectedScripts.Contains(script))
                    {
                        m_selectedScripts.Add(script);
                    }
                }
                else
                {
                    if (m_selectedScripts.Contains(script))
                    {
                        m_selectedScripts.Remove(script);
                    }
                }

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
                            SaveScriptDictionary(parentElement, script, dictionary, newData);
                        }
                        else
                        {
                            object newValue = attribute.Value;
                            if (DataChanged(oldValue, newValue))
                            {
                                System.Diagnostics.Debug.WriteLine("New value for script: Was {0}, now {1}", oldValue, newValue);
                                script.SetParameter(attribute.Key, newValue);
                                m_needsSaving = true;
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
                        m_needsSaving = true;
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
                            m_needsSaving = true;
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

        private void SaveScriptDictionary(string parentElement, IEditableScript script, IEditableDictionary<IEditableScripts> dictionary, WebEditor.Models.ElementSaveData.ScriptSaveData newData)
        {
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
                    m_needsSaving = true;
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
                    if (script != null)
                    {
                        AddScriptError(parentElement, script, GetValidationError(result, item.Value));
                    }
                    else
                    {
                        // TO DO: Add ScriptDictionary error
                    }
                }
            }
        }

        private void SaveStringDictionary(string parentElement, IEditableDictionary<string> dictionary, WebEditor.Models.ElementSaveData.ScriptSaveData newData)
        {
            Dictionary<string, string> keysToChange = new Dictionary<string, string>();
            Dictionary<string, string> valuesToChange = new Dictionary<string, string>();
            int dictionaryCount = 0;
            foreach (var item in dictionary.Items)
            {
                string newKey = (string)newData.Attributes[string.Format("key{0}", dictionaryCount)];
                if (item.Key != newKey)
                {
                    // Can't change dictionary keys while enumerating dictionary items - so
                    // change them afterwards
                    keysToChange.Add(item.Key, newKey);
                    m_needsSaving = true;
                }

                string newValue = (string)newData.Attributes[string.Format("value{0}", dictionaryCount)];
                if (item.Value.Value != newValue)
                {
                    valuesToChange.Add(item.Key, newValue);
                }

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
                    AddDictionaryError(parentElement, dictionary, GetValidationError(result, item.Value));
                }
            }

            foreach (var item in valuesToChange)
            {
                dictionary.Update(item.Key, item.Value);
            }
        }

        private void SaveObjectReference(IEditorData data, string attribute, IEditableObjectReference currentValue, WebEditor.Models.ElementSaveData.ObjectReferenceSaveData newValue)
        {
            if (currentValue == null && string.IsNullOrEmpty(newValue.Reference)) return;

            if (currentValue == null || currentValue.Reference != newValue.Reference)
            {
                IEditableObjectReference newReference = m_controller.CreateNewEditableObjectReference(data.Name, attribute, false);
                newReference.Reference = newValue.Reference;
                data.SetAttribute(attribute, newReference);
                m_needsSaving = true;
            }
        }

        private void SaveCommandPattern(IEditorData data, string attribute, IEditableCommandPattern currentValue, WebEditor.Models.ElementSaveData.PatternSaveData newValue)
        {
            if (currentValue == null || currentValue.Pattern != newValue.Pattern)
            {
                IEditableCommandPattern newPattern = m_controller.CreateNewEditableCommandPattern(data.Name, attribute, newValue.Pattern, false);
                m_needsSaving = true;
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

        public Models.Script GetScriptModel(int id, string key, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState)
        {
            IEditableScripts value = (IEditableScripts)m_controller.GetEditorData(key).GetAttribute(ctl.Attribute);
            return GetScriptModel(id, key, value, ctl.Attribute, modelState);
        }

        public Models.Script GetScriptModel(int id, string key, IEditableScripts value, string attribute, System.Web.Mvc.ModelStateDictionary modelState)
        {
            foreach (var error in m_scriptErrors)
            {
                modelState.AddModelError(error.Key, error.Value.Message);
            }

            return new Models.Script
            {
                GameId = id,
                Key = key,
                Attribute = attribute,
                Controller = m_controller,
                Scripts = value,
                SelectedScripts = m_selectedScripts
            };
        }

        public Models.ScriptDictionary GetScriptDictionaryModel(int id, string key, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState)
        {
            IEditableDictionary<IEditableScripts> value = GetScriptDictionary(key, ctl.Attribute);
            return GetScriptDictionaryModel(id, key, value, ctl, ctl.Attribute, modelState);
        }

        public Models.ScriptDictionary GetScriptScriptDictionaryModel(int id, string key, string path, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState)
        {
            string parameter;
            IEditableScript scriptLine = GetScriptLine(key, path, out parameter);
            IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
            IEditableDictionary<IEditableScripts> value = (IEditableDictionary<IEditableScripts>)scriptEditorData.GetAttribute(parameter);
            return GetScriptDictionaryModel(id, key, value, ctl, path, modelState);
        }

        private Models.ScriptDictionary GetScriptDictionaryModel(int id, string key, IEditableDictionary<IEditableScripts> value, IEditorControl ctl, string attribute, System.Web.Mvc.ModelStateDictionary modelState)
        {
            return GetScriptDictionaryModel(id, key, value, ctl.GetString("keyprompt"), ctl.GetString("source"), attribute, modelState);
        }

        public Models.ScriptDictionary GetScriptDictionaryModel(int id, string key, IEditableDictionary<IEditableScripts> value, string keyPrompt, string source, string attribute, System.Web.Mvc.ModelStateDictionary modelState)
        {
            foreach (var error in m_dictionaryErrors)
            {
                modelState.AddModelError(error.Key, error.Value.Message);
            }

            return new Models.ScriptDictionary
            {
                GameId = id,
                Key = key,
                Attribute = attribute,
                KeyPrompt = keyPrompt,
                Source = source,
                Value = value
            };
        }

        public Models.StringDictionary GetStringDictionaryModel(int id, string key, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState)
        {
            IEditableDictionary<string> value = m_controller.GetEditorData(key).GetAttribute(ctl.Attribute) as IEditableDictionary<string>;

            foreach (var error in m_dictionaryErrors)
            {
                modelState.AddModelError(error.Key, error.Value.Message);
            }

            return new Models.StringDictionary
            {
                GameId = id,
                Key = key,
                Attribute = ctl.Attribute,
                KeyPrompt = ctl.GetString("keyprompt"),
                Source = ctl.GetString("source"),
                SourceExclude = ctl.GetString("sourceexclude"),
                Value = value
            };
        }

        public Models.StringDictionary GetStringDictionaryModel(int id, string key, IEditorControl ctl, System.Web.Mvc.ModelStateDictionary modelState, bool gameBook)
        {
            Models.StringDictionary result = GetStringDictionaryModel(id, key, ctl, modelState);
            result.GameBook = gameBook;
            return result;
        }

        public Models.ElementsList GetElementsListModel(int id, string key, IEditorControl ctl)
        {
            IEditorData data = m_controller.GetEditorData(key);
            string elementType = ctl.GetString("elementtype");
            string objectType = ctl.GetString("objecttype");
            string filter = ctl.GetString("listfilter");
            IEnumerable<string> elements;

            if (elementType == "object")
            {
                string parent = data == null ? null : data.Name;
                if (parent == "game") parent = null;
                elements = Controller.GetObjectNames(objectType, false, parent, true);
            }
            else
            {
                elements = Controller.GetElementNames(elementType, false);
            }

            Dictionary<string, Models.ElementsListItem> listItems = new Dictionary<string, Models.ElementsListItem>();

            string previousElement = null;

            foreach (string element in elements.Where(e =>
            {
                if (filter == null) return true;
                return Controller.ElementIsVerb(e) == (filter == "verb");
            }))
            {
                listItems.Add(element, new Models.ElementsListItem
                {
                    Text = Controller.GetDisplayName(element),
                    CanDelete = Controller.CanDelete(element) ? "1" : "0",
                    Previous = previousElement
                });

                if (previousElement != null)
                {
                    listItems[previousElement].Next = element;
                }

                previousElement = element;
            }

            return new Models.ElementsList
            {
                Key = ctl.Id,
                Items = listItems,
                ElementType = elementType,
                ObjectType = objectType,
                Filter = filter,
                FillScreen = ctl.GetBool("expand")
            };
        }

        public Models.Exits GetExitsModel(int id, string key, IEditorControl ctl)
        {
            Models.Exits result = new Models.Exits
            {
                Id = ctl.Id,
                Objects = new List<string>(m_controller.GetObjectNames("object")
                    .Where(n => n != key)
                    .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)),
                CreateInverse = m_createInverse,
                SimpleMode = m_controller.SimpleMode
            };

            IEnumerable<string> exits = m_controller.GetObjectNames("exit", key, true);
            List<string> compassDirections = new List<string>(ctl.GetListString("compass"));
            IDictionary<string, string> compassTypes = ctl.GetDictionary("compasstypes");

            result.Directions = new List<Models.Exits.CompassDirection>(
                compassDirections.Select(d => new Models.Exits.CompassDirection
                {
                    Name = d,
                    InverseName = GetInverseDirection(d, compassDirections),
                    DirectionType = compassTypes[d],
                    InverseDirectionType = compassTypes[GetInverseDirection(d, compassDirections)]
                })
            );

            Models.Exits.Exit previous = null;
            result.AllExits = new Dictionary<string, Models.Exits.Exit>();

            foreach (string exit in exits)
            {
                IEditorData data = m_controller.GetEditorData(exit);
                IEditableObjectReference to = data.GetAttribute("to") as IEditableObjectReference;
                string alias = data.GetAttribute("alias") as string;
                bool lookOnly = m_controller.GetEditorData(exit).GetAttribute("lookonly") as bool? == true;
                if (compassDirections.Contains(alias))
                {
                    int index = compassDirections.IndexOf(alias);
                    Models.Exits.CompassDirection exitModel = result.Directions[index];
                    exitModel.ElementId = exit;
                    exitModel.To = (to == null) ? null : to.Reference;
                    exitModel.LookOnly = lookOnly;
                }
                var thisExit = new Models.Exits.Exit
                {
                    Name = exit,
                    Alias = alias,
                    To = (to == null) ? null : to.Reference,
                    LookOnly = lookOnly,
                    Previous = (previous == null) ? null : previous.Name
                };
                result.AllExits.Add(exit, thisExit);
                if (previous != null)
                {
                    result.AllExits[previous.Name].Next = exit;
                }
                previous = thisExit;
            }

            return result;
        }

        private static List<int> s_oppositeDirs = new List<int> { 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 11, 10 };

        private string GetInverseDirection(string direction, List<string> directionNames)
        {
            // 0 = NW, 1 = N, 2 = NE
            // 3 = W ,        4 = E     8 = U, 10 = In
            // 5 = SW, 6 = S, 7 = SE    9 = D, 11 = Out

            // So opposites are:

            //   0 <--> 7
            //   1 <--> 6
            //   2 <--> 5
            //   3 <--> 4
            //   4 <--> 3
            //   5 <--> 2
            //   6 <--> 1
            //   7 <--> 0
            //   8 <--> 9
            //   9 <--> 8
            //  10 <--> 11
            //  11 <--> 10

            int dirIndex = directionNames.IndexOf(direction);
            int opposite = s_oppositeDirs[dirIndex];
            if (opposite == -1) return null;
            return directionNames[opposite];
        }

        public Models.Verbs GetVerbsModel(int id, string key, IEditorControl ctl)
        {
            return new Models.Verbs
            {
                GameId = id,
                Key = key,
                Controller = m_controller,
                EditorControl = ctl,
                EditorData = (IEditorDataExtendedAttributeInfo)m_controller.GetEditorData(key)
            };
        }

        public struct AdditionalActionResult
        {
            public string RefreshTreeSelectElement;
        }

        public AdditionalActionResult ProcessAdditionalAction(string key, string arguments)
        {
            Logging.Log.DebugFormat("{0}: ProcessAdditionalAction {1}, {2}", m_id, key, arguments);
            AdditionalActionResult result = new AdditionalActionResult();
            string[] data = arguments.Split(new[] { ' ' }, 3);
            string action = data[0];
            if (action == "none") return result;
            m_needsSaving = true;
            string cmd = data[1];
            string parameter = (data.Length == 3) ? data[2] : null;
            switch (action)
            {
                case "main":
                    result.RefreshTreeSelectElement = ProcessMainAction(key, cmd, parameter);
                    break;
                case "stringlist":
                    ProcessStringListAction(key, cmd, parameter);
                    break;
                case "script":
                    ProcessScriptAction(key, cmd, parameter);
                    break;
                case "scriptdictionary":
                    ProcessScriptDictionaryAction(key, cmd, parameter);
                    break;
                case "stringdictionary":
                    ProcessStringDictionaryAction(key, cmd, parameter);
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
                case "elementslist":
                    ProcessElementsListAction(key, cmd, parameter);
                    break;
                case "exits":
                    result.RefreshTreeSelectElement = ProcessExitsAction(key, cmd, parameter);
                    break;
                case "verbs":
                    ProcessVerbsAction(key, cmd, parameter);
                    break;
            }
            return result;
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
                case "cut":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptCut(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "copy":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptCopy(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "paste":
                    ScriptPaste(key, parameter);
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
                case "moveup":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptMoveUp(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "movedown":
                    data = parameter.Split(new[] { ';' }, 2);
                    if (data[1].Length > 0)
                    {
                        ScriptMoveDown(key, data[0], data[1].Split(';'));
                    }
                    break;
                case "codeview":
                    m_uiAction = "codeview " + parameter;
                    break;
                case "codeviewset":
                    data = parameter.Split(new[] { ';' }, 2);
                    ScriptSetCode(key, data[0], data[1]);
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

        private void ProcessStringDictionaryAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "add":
                    data = parameter.Split(new[] { ';' }, 2);
                    StringDictionaryAdd(key, data[0], data[1]);
                    break;
                case "delete":
                    data = parameter.Split(new[] { ';' }, 2);
                    StringDictionaryDelete(key, data[0], data[1]);
                    break;
                case "gamebookaddpage":
                    data = parameter.Split(new[] { ';' }, 2);
                    string result = AddNewObject(null, data[1]);
                    if (result != null)
                    {
                        StringDictionaryAdd(key, data[0], data[1]);
                    }
                    break;
            }
        }

        private void ProcessErrorAction(string key, string cmd, string parameter)
        {
            switch (cmd)
            {
                case "clear":
                    ErrorClear(key, parameter);
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

        private string ProcessMainAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "addroom":
                    return AddNewRoom(parameter);
                case "addobject":
                    data = parameter.Split(new[] { ';' }, 2);
                    return AddNewObject(data[1], data[0]);
                case "addpage":
                    return AddNewObject(null, parameter);
                case "addexit":
                    return AddNewExit(key);
                case "addfunction":
                    return AddNewFunction(parameter);
                case "addtimer":
                    return AddNewTimer(parameter);
                case "addturnscript":
                    if (key == "game") key = null;
                    return AddNewTurnScript(key);
                case "addcommand":
                    if (key == "game" || !m_controller.ElementExists(key)) key = null;
                    return AddNewCommand(key);
                case "delete":
                    if (DeleteElement(key))
                    {
                        return "game";
                    }
                    break;
                case "move":
                    MoveElement(key, parameter);
                    break;
                case "undo":
                    if (m_canUndo)
                    {
                        m_controller.Undo();
                    }
                    break;
                case "redo":
                    if (m_canRedo)
                    {
                        m_controller.Redo();
                    }
                    break;
                //case "cut":
                //    if (m_controller.CanCopy(key))
                //    {
                //        m_controller.CutElements(new string[] { key });
                //    }
                //    break;
                case "copy":
                    if (m_controller.CanCopy(key))
                    {
                        m_controller.CopyElements(new string[] { key });
                    }
                    break;
                case "paste":
                    if (m_controller.CanPaste(key))
                    {
                        m_controller.PasteElements(key);
                    }
                    break;
                case "settings":
                    m_uiAction = "settings";
                    break;
            }
            return null;
        }

        private void ProcessElementsListAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "delete":
                    DeleteElement(parameter);
                    break;
                case "swap":
                    data = parameter.Split(new[] { ';' }, 2);
                    SwapElements(data[0], data[1]);
                    break;
            }
        }

        private string ProcessExitsAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "create1":
                    data = parameter.Split(new[] { ';' }, 3);
                    CreateExit(key, data[0], data[1], data[2]);
                    break;
                case "create2":
                    data = parameter.Split(new[] { ';' }, 5);
                    CreateExitWithInverse(key, data[0], data[1], data[2], data[3], data[4]);
                    break;
                case "createlook":
                    data = parameter.Split(new[] { ';' }, 2);
                    return CreateLookExit(key, data[0], data[1]);
                case "delete":
                    DeleteElement(parameter);
                    break;
                case "swap":
                    data = parameter.Split(new[] { ';' }, 2);
                    SwapElements(data[0], data[1]);
                    break;
            }

            return null;
        }

        private void ProcessVerbsAction(string key, string cmd, string parameter)
        {
            string[] data;
            switch (cmd)
            {
                case "add":
                    data = parameter.Split(new[] { ';' }, 2);
                    VerbsAdd(key, data[0], data[1]);
                    break;
                case "delete":
                    VerbsDelete(key, parameter.Split(';'));
                    break;
            }
        }

        private void VerbsAdd(string element, string editorId, string verbPattern)
        {
            verbPattern = verbPattern.ToLower();
            IEditorControl ctl = FindEditorControl(element, editorId);
            IEditorDataExtendedAttributeInfo data = (IEditorDataExtendedAttributeInfo)m_controller.GetEditorData(element);
            string verbAttribute = m_controller.GetVerbAttributeForPattern(verbPattern);

            if (!Controller.IsVerbAttribute(verbAttribute))
            {
                TextAdventures.Quest.EditorController.CanAddVerbResult canAddResult = Controller.CanAddVerb(verbPattern);
                if (!canAddResult.CanAdd)
                {
                    IDictionary<string, string> clashMessages = ctl.GetDictionary("clashmessages");
                    string clashMessage = "Verb would clash with command: " + canAddResult.ClashingCommandDisplay;
                    if (clashMessages.ContainsKey(canAddResult.ClashingCommand))
                    {
                        clashMessage += "<br /><br />" + clashMessages[canAddResult.ClashingCommand];
                    }
                    m_popupError = clashMessage;
                    return;
                }
            }

            if (data.GetAttribute(verbAttribute) != null)
            {
                m_popupError = "Verb already exists";
                return;
            }

            Controller.StartTransaction(string.Format("Add '{0}' verb", verbPattern));

            if (!Controller.IsVerbAttribute(verbAttribute))
            {
                string newVerbId = Controller.CreateNewVerb(null, false);
                IEditorData verbData = Controller.GetEditorData(newVerbId);
                verbData.SetAttribute("property", verbAttribute);
                EditableCommandPattern pattern = (EditableCommandPattern)verbData.GetAttribute("pattern");
                pattern.Pattern = verbPattern;
                string defaultExpression = ctl.GetString("defaultexpression");
                verbData.SetAttribute("defaultexpression", defaultExpression.Replace("#verb#", verbPattern));
            }

            ValidationResult setAttrResult = data.SetAttribute(verbAttribute, String.Empty);
            if (!setAttrResult.Valid)
            {
                m_popupError = GetValidationError(setAttrResult, verbAttribute);
            }
            Controller.EndTransaction();
        }

        private void VerbsDelete(string element, string[] verbs)
        {
            m_controller.StartTransaction("Delete verbs");
            IEditorDataExtendedAttributeInfo data = (IEditorDataExtendedAttributeInfo)m_controller.GetEditorData(element);
            foreach (string verb in verbs)
            {
                data.RemoveAttribute(verb);
            }
            m_controller.EndTransaction();
        }

        private void CreateExit(string from, string to, string direction, string type)
        {
            m_controller.CreateNewExit(from, to, direction, type, false);
            m_createInverse = false;
        }

        private void CreateExitWithInverse(string from, string to, string direction, string type, string inverse, string inverseType)
        {
            // does an inverse exit already exist in the "to" room?
            bool inverseAlreadyExists = false;
            foreach (string exitName in m_controller.GetObjectNames("exit", to, true))
            {
                IEditorData exitData = m_controller.GetEditorData(exitName);
                bool lookOnly = exitData.GetAttribute("lookonly") as bool? == true;
                if (!lookOnly)
                {
                    string alias = exitData.GetAttribute("alias") as string;
                    if (alias == inverse)
                    {
                        inverseAlreadyExists = true;
                        break;
                    }
                }
            }

            if (!inverseAlreadyExists)
            {
                m_controller.CreateNewExit(from, to, direction, inverse, type, inverseType);
            }
            else
            {
                m_popupError = string.Format("Unable to create the inverse exit from '{0}' {1} to {2} - an exit in this direction already exists",
                    to, inverse, from);
                m_controller.CreateNewExit(from, to, direction, type, false);
            }

            m_createInverse = true;
        }

        private string CreateLookExit(string from, string direction, string type)
        {
            return m_controller.CreateNewExit(from, null, direction, type, true);
        }

        private string AddNewRoom(string value)
        {
            ValidationResult result = m_controller.CanAdd(value);
            if (!result.Valid)
            {
                m_popupError = GetValidationError(result, value);
                return null;
            }

            m_controller.CreateNewRoom(value, null, null);
            return value;
        }

        private string AddNewObject(string parent, string value)
        {
            ValidationResult result = m_controller.CanAdd(value);
            string alias = null;
            if (!result.Valid)
            {
                if (string.IsNullOrEmpty(result.SuggestedName))
                {
                    m_popupError = GetValidationError(result, value);
                    return null;
                }
                alias = value;
                value = result.SuggestedName;
            }

            if (parent != null && !m_controller.ElementExists(parent)) parent = null;

            m_controller.CreateNewObject(value, parent, alias);
            return value;
        }

        private string AddNewExit(string parent)
        {
            return m_controller.CreateNewExit(parent);
        }

        private string AddNewFunction(string value)
        {
            ValidationResult result = m_controller.CanAdd(value);
            if (!result.Valid)
            {
                m_popupError = GetValidationError(result, value);
                return null;
            }

            m_controller.CreateNewFunction(value);
            return value;
        }

        private string AddNewTimer(string value)
        {
            ValidationResult result = m_controller.CanAdd(value);
            if (!result.Valid)
            {
                m_popupError = GetValidationError(result, value);
                return null;
            }

            m_controller.CreateNewTimer(value);
            return value;
        }

        private string AddNewTurnScript(string parent)
        {
            return m_controller.CreateNewTurnScript(parent);
        }

        private string AddNewCommand(string parent)
        {
            return m_controller.CreateNewCommand(parent);
        }

        private bool DeleteElement(string element)
        {
            if (m_controller.CanDelete(element))
            {
                m_controller.DeleteElement(element, true);
                return true;
            }
            return false;
        }

        private void MoveElement(string element, string newParent)
        {
            if (m_controller.CanMoveElement(element, newParent))
            {
                m_controller.MoveElement(element, newParent);
            }
        }

        private void SwapElements(string element1, string element2)
        {
            m_controller.StartTransaction("Reorder elements");
            m_controller.SwapElements(element1, element2);
            m_controller.EndTransaction();
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

        private IEditorControl FindEditorControlByAttribute(string element, string attribute)
        {
            IEditorDefinition def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(element));
            foreach (IEditorTab tab in def.Tabs.Values)
            {
                IEditorControl ctl = tab.Controls.FirstOrDefault(c => c.Attribute == attribute);
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

        private void ErrorClear(string element, string id)
        {
            if (m_scriptErrors.ContainsKey(id))
            {
                m_scriptErrors.Remove(id);
            }
            if (m_elementErrors.ContainsKey(element))
            {
                if (m_elementErrors[element].ContainsKey(id))
                {
                    m_elementErrors[element].Remove(id);
                }
            }
            if (m_dictionaryErrors.ContainsKey(id))
            {
                m_dictionaryErrors.Remove(id);
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

        private void ScriptCut(string element, string attribute, string[] indexes)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScripts script = GetScript(element, attribute);
            script.Cut(indexes.Select(i => int.Parse(i)).ToArray());
        }

        private void ScriptCopy(string element, string attribute, string[] indexes)
        {
            IEditableScripts script = GetScript(element, attribute);
            script.Copy(indexes.Select(i => int.Parse(i)).ToArray());
        }

        private void ScriptPaste(string element, string attribute)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript parent;
            string parameter;
            IEditableScripts script = GetScript(element, attribute, out parent, out parameter);

            if (script == null)
            {
                m_controller.StartTransaction("Paste script");
                if (parent == null)
                {
                    script = m_controller.CreateNewEditableScripts(element, attribute, null, false);
                }
                else
                {
                    ScriptCommandEditorData editorData = (ScriptCommandEditorData)m_controller.GetScriptEditorData(parent);
                    script = m_controller.CreateNewEditableScriptsChild(editorData, parameter, null, false);
                }
                script.Paste(0, false);
                m_controller.EndTransaction();
            }
            else
            {
                script.Paste(script.Count, true);
            }
        }

        private void ScriptMoveUp(string element, string attribute, string[] indexes)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScripts script = GetScript(element, attribute);
            m_controller.StartTransaction("Script move up");

            var indexNumbers = from i in indexes
                               let number = int.Parse(i)
                               orderby number
                               select number;

            foreach (int index in indexNumbers)
            {
                if (index == 0) continue;
                script.Swap(index - 1, index);
            }

            m_controller.EndTransaction();
        }

        private void ScriptMoveDown(string element, string attribute, string[] indexes)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScripts script = GetScript(element, attribute);
            m_controller.StartTransaction("Script move down");

            var indexNumbers = from i in indexes
                               let number = int.Parse(i)
                               orderby number descending
                               select number;

            foreach (int index in indexNumbers)
            {
                if (index == script.Count - 1) continue;
                script.Swap(index, index + 1);
            }

            m_controller.EndTransaction();
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
                else if (section.StartsWith("elseif"))
                {
                    elseIfsToRemove.Add(ifScript.ElseIfScripts.ElementAt(int.Parse(section.Substring(6))));
                }
            }

            foreach (EditableIfScript.EditableElseIf elseIfToRemove in elseIfsToRemove)
            {
                ifScript.RemoveElseIf(elseIfToRemove);
            }
        }

        private void ScriptSetCode(string element, string attribute, string code)
        {
            // TO DO: if (m_data.ReadOnly) return;

            IEditableScript parent;
            string parameter;
            IEditableScripts script = GetScript(element, attribute, out parent, out parameter);

            if (script == null)
            {
                if (parent == null)
                {
                    script = m_controller.CreateNewEditableScripts(element, attribute, null, true, true);
                }
                else
                {
                    var editorData = (ScriptCommandEditorData)m_controller.GetScriptEditorData(parent);
                    script = m_controller.CreateNewEditableScriptsChild(editorData, parameter, null, true);                   
                }
            }

            script.Code = code;
        }

        private void ScriptSetTemplate(string element, string attribute, string template)
        {
            m_controller.StartTransaction("Set script template");
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
            m_controller.EndTransaction();
        }

        private void ScriptDictionaryAdd(string element, string attribute, string value)
        {
            IEditableScript scriptLine;
            IEditableDictionary<IEditableScripts> dictionary = GetScriptDictionary(element, attribute, out scriptLine);
            if (dictionary == null)
            {
                m_controller.CreateNewEditableScriptDictionary(
                    element,
                    attribute,
                    value,
                    m_controller.CreateNewEditableScripts(null, null, null, true),
                    true
                );
            }
            else
            {
                ValidationResult result = dictionary.CanAdd(value);
                if (result.Valid)
                {
                    dictionary.Add(value, m_controller.CreateNewEditableScripts(null, null, null, true));
                }
                else
                {
                    if (scriptLine != null)
                    {
                        AddScriptError(element, scriptLine, GetValidationError(result, value));
                    }
                    else
                    {
                        AddDictionaryError(element, dictionary, GetValidationError(result, value));
                    }
                }
            }
        }

        private void ScriptDictionaryDelete(string element, string attribute, string value)
        {
            IEditableDictionary<IEditableScripts> dictionary = GetScriptDictionary(element, attribute);
            // value is a semicolon-separated list of indexes
            string[] keys = value.Split(';').Select(i => dictionary.Items.ElementAt(int.Parse(i)).Key).ToArray();
            dictionary.Remove(keys);
        }

        private void StringDictionaryAdd(string element, string attribute, string value)
        {
            var dictionary = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableDictionary<string>;
            if (dictionary == null)
            {
                m_controller.CreateNewEditableStringDictionary(
                    element,
                    attribute,
                    value,
                    "",
                    true
                );
            }
            else
            {
                ValidationResult result = dictionary.CanAdd(value);
                if (result.Valid)
                {
                    dictionary.Add(value, "");
                }
                else
                {
                    AddDictionaryError(element, dictionary, GetValidationError(result, value));
                }
            }
        }

        private void StringDictionaryDelete(string element, string attribute, string value)
        {
            var dictionary = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableDictionary<string>;
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
            if (attribute.Contains('-'))
            {
                string parameter;
                scriptLine = GetScriptLine(element, attribute, out parameter);
                IEditorData scriptEditorData = m_controller.GetScriptEditorData(scriptLine);
                return (IEditableDictionary<IEditableScripts>)scriptEditorData.GetAttribute(parameter);
            }
            else
            {
                scriptLine = null;
                return m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableDictionary<IEditableScripts>;
            }
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

        private void AddElementError(string element, string attribute, string error)
        {
            if (!m_elementErrors.ContainsKey(element))
            {
                m_elementErrors.Add(element, new Dictionary<string, string>());
            }
            m_elementErrors[element][attribute] = error;
        }

        private void AddDictionaryError<T>(string element, IEditableDictionary<T> dictionary, string error)
        {
            if (!m_dictionaryErrors.ContainsKey(dictionary.Id))
            {
                m_dictionaryErrors.Add(dictionary.Id, new ErrorData { Message = error, Element = element });
            }
            else
            {
                m_dictionaryErrors[dictionary.Id].Message = m_dictionaryErrors[dictionary.Id].Message + ". " + error;
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

            // Is it simply an attribute?

            if (attribute.IndexOf("-") == -1)
            {
                script = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableScripts;
            }
            else
            {
                string[] path;

                // Is it a script dictionary?

                path = attribute.Split(new[] { '-' });
                object value = m_controller.GetEditorData(element).GetAttribute(path[0]);
                IEditableDictionary<IEditableScripts> dict = value as IEditableDictionary<IEditableScripts>;
                if (dict != null)
                {
                    if (!path[1].StartsWith("value"))
                    {
                        throw new ArgumentException("Invalid path format");
                    }
                    int index = int.Parse(path[1].Substring(5));

                    if (path.Length == 2)
                    {
                        return dict.Items.ElementAt(index).Value.Value;
                    }
                }

                // Is it a nested script?

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
                        path = parameter.Split('-');
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
            object value = m_controller.GetEditorData(element).GetAttribute(path[0]);

            IEditableScripts parent = value as IEditableScripts;
            if (parent != null)
            {
                return GetScriptLine(path, parent, out parameter);
            }

            IEditableDictionary<IEditableScripts> dict = value as IEditableDictionary<IEditableScripts>;
            if (dict != null)
            {
                if (!path[1].StartsWith("value"))
                {
                    throw new ArgumentException("Invalid path format");
                }
                int index = int.Parse(path[1].Substring(5));
                IEditableScripts script = dict.Items.ElementAt(index).Value.Value;

                List<string> newPath = new List<string>(path);
                newPath.RemoveRange(0, 1);
                return GetScriptLine(newPath.ToArray(), script, out parameter);
            }

            throw new ArgumentException("Unknown script type");
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
            public string button;
            public string key;
        }

        public object GetScriptAdderJson()
        {
            Dictionary<string, ScriptAdderCategory> categories = new Dictionary<string, ScriptAdderCategory>();
            foreach (string cat in m_controller.GetAllScriptEditorCategories(true))
            {
                categories.Add(cat, new ScriptAdderCategory());
            }

            foreach (var data in m_controller.GetScriptEditorData())
            {
                if (m_controller.SimpleMode && !data.Value.IsVisibleInSimpleMode) continue;
                if (data.Value.IsDesktopOnly) continue;
                categories[data.Value.Category].items.Add(new ScriptAdderItem
                {
                    display = data.Value.AdderDisplayString,
                    create = data.Value.CreateString,
                    button = data.Value.CommonButton,
                    key = data.Key
                });
            }

            return categories;
        }

        private string GetEnabledButtons(string element)
        {
            List<string> result = new List<string>();
            if (m_canUndo) result.Add("undo");
            if (m_canRedo) result.Add("redo");
            if (m_controller.CanDelete(element)) result.Add("delete");
            if (m_controller.CanDelete(element) && m_controller.CanCopy(element)) result.Add("cut");
            if (m_controller.CanCopy(element)) result.Add("copy");
            if (m_controller.CanPaste(element)) result.Add("paste");
            return string.Join(";", result);
        }

        public static Models.Create GetCreateModel(string templateFolder)
        {
            Models.Create model = new Models.Create
            {
                SelectedTemplate = "English",
                SelectedType = "Text adventure"
            };
            PopulateCreateModelLists(model, templateFolder);
            return model;
        }

        public static void PopulateCreateModelLists(Models.Create model, string templateFolder)
        {
            Dictionary<string, TemplateData> templates = GetAvailableTemplates(templateFolder);
            model.AllTemplates = templates.Values.Where(t => t.Type == EditorStyle.TextAdventure).Select(t => t.TemplateName);
            model.AllTypes = new List<string> { "Text adventure", "Gamebook" };
        }

        private static Dictionary<string, TemplateData> GetAvailableTemplates(string folder)
        {
            return EditorController.GetAvailableTemplates(folder);
        }

        private static string GetTemplateFile(string gameType, string templateName, string folder)
        {
            if (gameType == "Gamebook")
            {
                return GetAvailableTemplates(folder).Values.First(t => t.Type == EditorStyle.GameBook).Filename;
            }
            return GetAvailableTemplates(folder).Values.First(t => t.TemplateName == templateName).Filename;
        }

        public static int CreateNewGame(string gameType, string templateName, string gameName, string templateFolder)
        {
            string filename = EditorController.GenerateSafeFilename(gameName) + ".aslx";
            CreateNewFileData fileData = FileManagerLoader.GetFileManager().CreateNewFile(filename, gameName);
            var data = EditorController.CreateNewGameFile(fileData.FullPath, GetTemplateFile(gameType, templateName, templateFolder), gameName);

            if (!Config.AzureFiles)
            {
                System.IO.File.WriteAllText(fileData.FullPath, data);
            }
            
            FileManagerLoader.GetFileManager().FinishCreatingNewFile(fileData.FullPath, data);
            return fileData.Id;
        }

        public List<string> GetPermittedExtensions(string element, string attribute)
        {
            string source;

            if (attribute != null)
            {
                IEditorControl ctl;

                if (attribute.Contains('-'))
                {
                    // attribute will end with "-simpleeditor"
                    attribute = attribute.Substring(0, attribute.Length - 13);
                    string parameter;
                    IEditableScript script = GetScriptLine(element, attribute, out parameter);
                    IEditorDefinition def = m_controller.GetEditorDefinition(script);
                    ctl = def.Controls.First(c => c.Attribute == parameter);
                }
                else
                {
                    ctl = FindEditorControlByAttribute(element, attribute);
                }

                source = ctl.GetString("source");
            }
            else
            {
                // if no attribute specified, e.g. this is the text processor helper form, use a default
                // image extensions list.

                source = "*.jpg;*.jpeg;*.png;*.gif";
            }

            return new List<string>(source.Split(';').Select(s => s.Substring(1)));
        }

        public string Style
        {
            get
            {
                return m_controller.EditorStyle == TextAdventures.Quest.EditorStyle.GameBook ? "gamebook" : "textadventure";
            }
        }

        public void Publish(string filename, IEnumerable<EditorController.PackageIncludeFile> includeFiles, System.IO.Stream outputStream)
        {
            m_controller.Publish(filename, false, includeFiles, outputStream);
        }

        private string GetHiddenScripts()
        {
            var hiddenScripts = m_controller.GetScriptEditorData()
                                            .Where(d => !d.Value.IsVisible())
                                            .Select(d => d.Key);
            return string.Join(";", hiddenScripts);
        }

        private string GetScriptCategories()
        {
            var scriptCategories = m_controller.GetAllScriptEditorCategories(false);
            return string.Join(";", scriptCategories);
        }
    }
}