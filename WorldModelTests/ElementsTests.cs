using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class ElementsTests
    {
        private WorldModel m_worldModel;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();

            // a
            // - b
            //   - c
            //   - d
            // - e
            // f

            Element a = m_worldModel.GetElementFactory(ElementType.Object).Create("a");
            Element b = m_worldModel.GetElementFactory(ElementType.Object).Create("b");
            b.Parent = a;
            Element c = m_worldModel.GetElementFactory(ElementType.Object).Create("c");
            c.Parent = b;
            Element d = m_worldModel.GetElementFactory(ElementType.Object).Create("d");
            d.Parent = b;
            Element e = m_worldModel.GetElementFactory(ElementType.Object).Create("e");
            e.Parent = a;
            Element f = m_worldModel.GetElementFactory(ElementType.Object).Create("f");
        }

        [TestMethod]
        public void TestGetChildrenOfA()
        {
            List<string> childList = new List<string>(
                m_worldModel.Elements.GetChildElements(m_worldModel.Elements.Get("a")).Select(e => e.Name));
            
            // all children of a should be b,c,d,e

            Assert.AreEqual(4, childList.Count);
            Assert.AreEqual("b", childList[0]);
            Assert.AreEqual("c", childList[1]);
            Assert.AreEqual("d", childList[2]);
            Assert.AreEqual("e", childList[3]);
        }

        [TestMethod]
        public void TestGetChildrenOfF()
        {
            List<string> childList = new List<string>(
                m_worldModel.Elements.GetChildElements(m_worldModel.Elements.Get("f")).Select(e => e.Name));

            // no children of f

            Assert.AreEqual(0, childList.Count);
        }
    }
}
