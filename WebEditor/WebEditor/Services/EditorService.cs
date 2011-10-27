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
            m_elements.Add(key, new TreeItem { 
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

        public Models.Game GetModelForView()
        {
            return new Models.Game
            {
                Elements = GetTreeItemsForParent(null),
                Name = m_controller.GameName,
                GameId = m_id
            };
        }

        private List<Models.Game.TreeItem> GetTreeItemsForParent(string parent)
        {
            List<Models.Game.TreeItem> result = new List<Models.Game.TreeItem>();
            TreeItem parentElement = (parent == null) ? null : m_elements[parent];
            foreach (TreeItem item in m_elements.Values.Where(e => e.Parent == parentElement))
            {
                Models.Game.TreeItem modelTreeItem = new Models.Game.TreeItem
                {
                    Key = item.Key,
                    Text = item.Text,
                    Children = GetTreeItemsForParent(item.Key)
                };
                result.Add(modelTreeItem);
            }
            return result;
        }
    }
}