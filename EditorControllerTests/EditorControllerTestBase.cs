using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

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
            m_controller.ClearTree += new EditorController.VoidHandler(m_controller_ClearTree);
            m_controller.BeginTreeUpdate += new EditorController.VoidHandler(m_controller_BeginTreeUpdate);
            m_controller.EndTreeUpdate += new EditorController.VoidHandler(m_controller_EndTreeUpdate);
            m_controller.AddedNode += new EditorController.AddedNodeHandler(m_controller_AddedNode);
            m_controller.UndoListUpdated += new EventHandler<EditorController.UpdateUndoListEventArgs>(m_controller_UndoListUpdated);
            m_controller.RedoListUpdated += new EventHandler<EditorController.UpdateUndoListEventArgs>(m_controller_RedoListUpdated);
            m_controller.Initialise(@"..\..\..\EditorControllerTests\test.aslx");
            DoExtraInitialisation();
        }

        public virtual void DoExtraInitialisation()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
            m_controller.Dispose();
        }

        void m_controller_ClearTree()
        {
            m_tree.Clear();
        }

        void m_controller_BeginTreeUpdate()
        {
            m_tree.BeginUpdate();
        }

        void m_controller_EndTreeUpdate()
        {
            m_tree.EndUpdate();
        }

        void m_controller_AddedNode(string key, string text, string parent, System.Drawing.Color? foreColor, System.Drawing.Color? backColor)
        {
            m_tree.Add(key, text, parent);
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
