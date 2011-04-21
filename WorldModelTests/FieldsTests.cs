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

        const string inheritedAttribute2Name = "otherattribute";
        const string inheritedAttribute2Value = "othervalue";

        const string attributeDefinedByDefaultName = "somedefaultattribute";
        const string attributeDefinedByDefaultValue = "somedefaultvalue";

        const string attributeDefinedByDefault2Name = "otherdefaultattribute";
        const string attributeDefinedByDefault2Value = "otherdefaultvalue";
        const string attributeDefinedByDefault2OverriddenValue = "overriddendefaultvalue";

        private WorldModel m_worldModel;
        private Element m_object;
        private Element m_subType;
        private Element m_objectType;
        private Element m_defaultType;

        [TestInitialize]
        public void Setup()
        {
            const string inheritedTypeName = "inherited";
            const string subInheritedTypeName = "subtype";
            const string defaultObject = "defaultobject";

            m_worldModel = new WorldModel();

            m_defaultType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(defaultObject);
            m_defaultType.Fields.Set(attributeDefinedByDefaultName, attributeDefinedByDefaultValue);
            m_defaultType.Fields.Set(attributeDefinedByDefault2Name, attributeDefinedByDefault2Value);

            m_subType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(subInheritedTypeName);
            m_subType.Fields.Set(inheritedAttribute2Name, inheritedAttribute2Value);
            m_defaultType.Fields.Set(attributeDefinedByDefault2Name, attributeDefinedByDefault2OverriddenValue);

            m_objectType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(inheritedTypeName);
            m_objectType.Fields.Set(inheritedAttributeName, inheritedAttributeValue);
            m_objectType.Fields.AddType(m_subType);

            m_object = m_worldModel.GetElementFactory(ElementType.Object).Create("object");
            m_object.Fields.AddType(m_objectType);
            m_object.Fields.Resolve(null);
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

        [TestMethod]
        public void TestSubinheritedFields()
        {
            // Test fields from:
            //    m_subType -> m_objectType -> m_object
            Assert.AreEqual(inheritedAttribute2Value, m_object.Fields.Get(inheritedAttribute2Name));

            // Test that defaultobject values are picked up
            Assert.AreEqual(attributeDefinedByDefaultValue, m_object.Fields.Get(attributeDefinedByDefaultName));

            // Test a defaultobject value overridden by a subtype
            Assert.AreEqual(attributeDefinedByDefault2OverriddenValue, m_object.Fields.Get(attributeDefinedByDefault2Name));
        }
    }
}
