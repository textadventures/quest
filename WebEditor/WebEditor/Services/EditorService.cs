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

        public Models.Element GetElementModelForView(int gameId, string key)
        {
            IEditorData data = m_controller.GetEditorData(key);
            IEditorDefinition def = m_controller.GetEditorDefinition(m_controller.GetElementEditorName(key));
            return new Models.Element
            {
                GameId = gameId,
                Key = key,
                Name = m_controller.GetDisplayName(key),
                EditorData = data,
                EditorDefinition = def
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

        public Models.StringList GetStringList(string key, string attribute)
        {
            IEditableList<string> value = (IEditableList<string>)m_controller.GetEditorData(key).GetAttribute(attribute);
            return new Models.StringList { Items = (value == null) ? null : value.Items.Values.Select(v => v.Value) };
        }
    }
}