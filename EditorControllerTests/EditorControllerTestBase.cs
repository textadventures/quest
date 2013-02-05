using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public abstract class EditorControllerTestBase
    {
        private EditorController m_controller;
        private EditorTreeData m_tree = new EditorTreeData();
        private List<string> m_undoList = new List<string>();
        private List<string> m_redoList = new List<string>();

        protected EditorController Controller
        {
            get { return m_controller; }
        }

        protected List<string> UndoList
        {
            get { return m_undoList; }
        }

        protected List<string> RedoList
        {
            get { return m_redoList; }
        }

        [TestInitialize]
        public void Init()
        {
            m_controller = new EditorController();
            m_controller.ClearTree += m_controller_ClearTree;
            m_controller.BeginTreeUpdate += m_controller_BeginTreeUpdate;
            m_controller.EndTreeUpdate += m_controller_EndTreeUpdate;
            m_controller.AddedNode += m_controller_AddedNode;
            m_controller.UndoListUpdated += m_controller_UndoListUpdated;
            m_controller.RedoListUpdated += m_controller_RedoListUpdated;
            string tempFile = System.IO.Path.GetTempFileName();
            ExtractResource("EditorControllerTests.test.aslx", tempFile);
            m_controller.Initialise(tempFile);
            DoExtraInitialisation();
            try
            {
                System.IO.File.Delete(tempFile);
            }
            catch (System.IO.IOException)
            {
                // ignore
            }
        }

        private void ExtractResource(string resource, string location)
        {
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            using (var streamReader = new System.IO.StreamReader(stream))
            {
                System.IO.File.WriteAllText(location, streamReader.ReadToEnd());
            }
        }

        public virtual void DoExtraInitialisation()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
            m_controller.Dispose();
        }

        void m_controller_ClearTree(object sender, EventArgs e)
        {
            m_tree.Clear();
        }

        void m_controller_BeginTreeUpdate(object sender, EventArgs e)
        {
            m_tree.BeginUpdate();
        }

        void m_controller_EndTreeUpdate(object sender, EventArgs e)
        {
            m_tree.EndUpdate();
        }

        void m_controller_AddedNode(object sender, EditorController.AddedNodeEventArgs e)
        {
            m_tree.Add(e.Key, e.Text, e.Parent);
        }

        void m_controller_UndoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
        {
            m_undoList = new List<string>(e.UndoList);
        }

        void m_controller_RedoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
        {
            m_redoList = new List<string>(e.UndoList);
        }
    }
}
