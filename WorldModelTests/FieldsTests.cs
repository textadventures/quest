using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class FieldsTests
    {
        const string inheritedAttributeName = "inheritedattribute";
        const string inheritedAttributeValue = "inheritedattributevalue";
        private WorldModel m_worldModel;
        private Element m_object;

        [TestInitialize]
        public void Setup()
        {
            const string inheritedTypeName = "inherited";

            m_worldModel = new WorldModel();
            Element m_objectType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(inheritedTypeName);
            m_objectType.Fields.Set(inheritedAttributeName, inheritedAttributeValue);

            m_object = m_worldModel.GetElementFactory(ElementType.Object).Create("object");
            m_object.Fields.AddType(m_objectType);
        }

        [TestMethod]
        public void TestStringFields()
        {
            const string property = "property";
            const string value1 = "first value";
            const string value2 = "second value";
            m_object.Fields.Set(property, value1);
            Assert.AreEqual(value1, m_object.Fields.Get(property));

            m_worldModel.UndoLogger.StartTransaction("Set value2");
            m_object.Fields.Set(property, value2);
            m_worldModel.UndoLogger.EndTransaction();

            Assert.AreEqual(value2, m_object.Fields.Get(property));
            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual(value1, m_object.Fields.Get(property));
        }

        [TestMethod]
        public void TestInheritedFields()
        {
            const string newValue = "newvalue";
            Assert.AreEqual(inheritedAttributeValue, m_object.Fields.Get(inheritedAttributeName));

            m_worldModel.UndoLogger.StartTransaction("Override inherited field value");
            m_object.Fields.Set(inheritedAttributeName, newValue);
            m_worldModel.UndoLogger.EndTransaction();

            Assert.AreEqual(newValue, m_object.Fields.Get(inheritedAttributeName));
            m_worldModel.UndoLogger.Undo();
            Assert.AreEqual(inheritedAttributeValue, m_object.Fields.Get(inheritedAttributeName));
        }
    }
}
