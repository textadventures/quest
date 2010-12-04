using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class EditableScriptTests
    {
        private EditorController m_controller;
        private EditorTreeData m_tree = new EditorTreeData();

        [TestInitialize]
        public void Init()
        {
            m_controller = new EditorController();
            m_controller.ClearTree += new EditorController.VoidHandler(m_controller_ClearTree);
            m_controller.BeginTreeUpdate += new EditorController.VoidHandler(m_controller_BeginTreeUpdate);
            m_controller.EndTreeUpdate += new EditorController.VoidHandler(m_controller_EndTreeUpdate);
            m_controller.AddedNode += new EditorController.AddedNodeHandler(m_controller_AddedNode);
            m_controller.Initialise(@"..\..\..\EditorControllerTests\test.aslx");
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

        [TestMethod]
        public void CreateScript()
        {
            EditableScripts newScripts = m_controller.CreateNewEditableScripts("game", "somescript", "msg (\"hello\")");
            Assert.AreEqual(1, newScripts.Scripts.Count());
            IEditableScript script = newScripts[0];
            Assert.AreEqual("Print \"hello\"", script.DisplayString());
            m_controller.StartTransaction("Change msg script value");
            script.SetParameter(0, "\"new value\"");
            m_controller.EndTransaction();
            Assert.AreEqual("Print \"new value\"", script.DisplayString());
            m_controller.Undo();
            Assert.AreEqual("Print \"hello\"", script.DisplayString());
            m_controller.Redo();
            Assert.AreEqual("Print \"new value\"", script.DisplayString());
        }
    }
}
