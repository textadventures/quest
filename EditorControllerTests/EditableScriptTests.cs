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
        }

        void m_controller_BeginTreeUpdate()
        {
        }

        void m_controller_EndTreeUpdate()
        {
        }

        void m_controller_AddedNode(string key, string text, string parent, System.Drawing.Color? foreColor, System.Drawing.Color? backColor)
        {
        }

        [TestMethod]
        public void CreateScript()
        {
            EditableScripts newScripts = m_controller.CreateNewEditableScripts("game", "somescript", "msg (\"\")");
            Assert.IsTrue(newScripts.Scripts.Count() == 1);
        }
    }
}
