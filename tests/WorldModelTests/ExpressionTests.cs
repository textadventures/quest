using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace WorldModelTests
{
    [TestClass]
    public class ExpressionTests
    {
        const string attributeName = "attribute";
        const string attributeValue = "attributevalue";
        const string childAttributeValue = "childattributevalue";

        const string intAttributeName = "intattribute";
        const int intAttributeValue = 42;

        private WorldModel m_worldModel;
        private Element m_object;
        private Element m_child;

        [TestInitialize]
        public void Setup()
        {
            m_worldModel = new WorldModel();
            m_object = m_worldModel.GetElementFactory(ElementType.Object).Create("object");
            m_object.Fields.Set(attributeName, attributeValue);
            m_object.Fields.Set(intAttributeName, intAttributeValue);

            m_child = m_worldModel.GetElementFactory(ElementType.Object).Create("child");
            m_child.Parent = m_object;
            m_child.Fields.Set(attributeName, childAttributeValue);
        }

        private T RunExpression<T>(string expression)
        {
            Expression<T> expr = new Expression<T>(expression, new ScriptContext(m_worldModel));
            Context c = new Context();
            return expr.Execute(c);
        }

        [TestMethod]
        public void TestReadStringFields()
        {
            string result;
            
            result = RunExpression<string>("object.attribute");
            Assert.AreEqual(attributeValue, result);

            result = RunExpression<string>("child.attribute");
            Assert.AreEqual(childAttributeValue, result);
        }

        [TestMethod]
        public void TestReadChildParentField()
        {
            string result = RunExpression<string>("child.parent.attribute");
            Assert.AreEqual(attributeValue, result);
        }

        [TestMethod]
        public void TestStringConcatenate()
        {
            const string extraString = "testconcat";
            string result = RunExpression<string>("object.attribute + \"" + extraString + "\"");
            Assert.AreEqual(attributeValue + extraString, result);
        }

        [TestMethod]
        public void TestReadIntField()
        {
            int result = RunExpression<int>("object.intattribute");
            Assert.AreEqual(intAttributeValue, result);
        }

        [TestMethod]
        public void TestAddition()
        {
            int result = RunExpression<int>("object.intattribute + 3");
            Assert.AreEqual(intAttributeValue + 3, result);
        }

        [TestMethod]
        public void TestChangingTypes()
        {
            ScriptContext scriptContext = new ScriptContext(m_worldModel);
            string expression = "a + b";
            ExpressionGeneric expr = new ExpressionGeneric(expression, scriptContext);
            Context c = new Context();
            c.Parameters = new Parameters();
            c.Parameters.Add("a", 1);
            c.Parameters.Add("b", 2);

            Assert.AreEqual(3, (int)expr.Execute(c));

            c.Parameters["a"] = "A";
            c.Parameters["b"] = "B";

            Assert.AreEqual("AB", (string)expr.Execute(c));
        }
    }
}
