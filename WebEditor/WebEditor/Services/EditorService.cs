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

        private EditorController m_controller;
        private Dictionary<string, TreeItem> m_elements = new Dictionary<string, TreeItem>();
        private int m_id;

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
            public string state;
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
                result.Add(modelTreeItem);
            }
            return result;
        }

        public Models.Element GetElementModelForView(int gameId, string key, string tab)
        {
            IEditorData data = m_controller.GetEditorData(key);
            IEditorDefinition def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(key));
            return new Models.Element
            {
                GameId = gameId,
                Key = key,
                Name = m_controller.GetDisplayName(key),
                EditorData = data,
                EditorDefinition = def,
                Tab = tab
            };
        }

        public void SaveElement(string key, Models.ElementSaveData saveData)
        {
            IEditorData data = m_controller.GetEditorData(key);
            foreach (var kvp in saveData.Values)
            {
                if (DataChanged(data.GetAttribute(kvp.Key), (kvp.Value)))
                {
                    System.Diagnostics.Debug.WriteLine("New value for {0}: Was {1}, now {2}", kvp.Key, data.GetAttribute(kvp.Key), kvp.Value);
                    data.SetAttribute(kvp.Key, kvp.Value);
                }
            }
            if (!string.IsNullOrEmpty(saveData.AdditionalAction))
            {
                ProcessAdditionalAction(key, saveData.AdditionalAction);
            }
        }

        private bool DataChanged(object oldValue, object newValue)
        {
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
            throw new NotImplementedException();
        }

        public Models.StringList GetStringList(string key, IEditorControl ctl)
        {
            IEditableList<string> value = (IEditableList<string>)m_controller.GetEditorData(key).GetAttribute(ctl.Attribute);
            return new Models.StringList
            {
                Attribute = ctl.Attribute,
                EditPrompt = ctl.GetString("editprompt"),
                Items = (value == null) ? null : value.Items.Values.Select(v => v.Value)
            };
        }

        private void ProcessAdditionalAction(string key, string action)
        {
            string[] data = action.Split(new[] { ' ' }, 2);
            string cmd = data[0];
            string parameter = data[1];
            switch (cmd)
            {
                case "stringlist":
                    data = parameter.Split(new[] { ' ' }, 2);
                    string stringListCmd = data[0];
                    string stringListParameter = data[1];
                    switch (stringListCmd)
                    {
                        case "add":
                            data = stringListParameter.Split(new[] { ';' }, 2);
                            StringListAdd(key, data[0], data[1]);
                            break;
                    }
                    break;
            }
        }

        private void StringListAdd(string element, string attribute, string value)
        {
            // TO DO: if (m_data.ReadOnly) return;
            // TO DO: Validate input first

            IEditableList<string> list = m_controller.GetEditorData(element).GetAttribute(attribute) as IEditableList<string>;
            if (list == null)
            {
                list = m_controller.CreateNewEditableList(element, attribute, value, true);
            }
            else
            {
                PrepareStringListForEditing(element, attribute, ref list);
                list.Add(value);
            }
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

    }
}