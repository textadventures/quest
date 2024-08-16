using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace EditorControllerTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestIsSimpleStringExpression()
        {
            Assert.AreEqual(true, EditorUtility.IsSimpleStringExpression("\"simple string\""));
            Assert.AreEqual(false, EditorUtility.IsSimpleStringExpression("\"not simple string\" + somevariable"));
            Assert.AreEqual(true, EditorUtility.IsSimpleStringExpression("\"simple string \\\"with nested quote\\\"\""));
            Assert.AreEqual(false, EditorUtility.IsSimpleStringExpression("\"simple string \\\"with nested quote\\\"\" + somevariable"));
        }

        [TestMethod]
        public void TestConvertToSimpleStringExpression()
        {
            Assert.AreEqual("simple string", EditorUtility.ConvertToSimpleStringExpression("\"simple string\""));
            Assert.AreEqual("simple string \"with nested quote\"", EditorUtility.ConvertToSimpleStringExpression("\"simple string \\\"with nested quote\\\"\""));
        }

        [TestMethod]
        public void TestConvertFromSimpleStringExpression()
        {
            Assert.AreEqual("\"simple string\"", EditorUtility.ConvertFromSimpleStringExpression("simple string"));
            Assert.AreEqual("\"simple string \\\"with nested quote\\\"\"", EditorUtility.ConvertFromSimpleStringExpression("simple string \"with nested quote\""));
        }
    }
}
