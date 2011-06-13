using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AxeSoftware.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestConvertDottedProperties()
        {
            Assert.AreEqual("obj___DOT___prop", Utility.ConvertDottedPropertiesToVariable("obj.prop"));
            Assert.AreEqual("obj1___DOT___prop obj2___DOT___prop", Utility.ConvertDottedPropertiesToVariable("obj1.prop obj2.prop"));
            Assert.AreEqual("(\"myfile.html\")", Utility.ConvertDottedPropertiesToVariable("(\"myfile.html\")"));
            Assert.AreEqual("\"myfile.html\"", Utility.ConvertDottedPropertiesToVariable("\"myfile.html\""));
            Assert.AreEqual("obj1___DOT___prop \"test.html\" obj2___DOT___prop", Utility.ConvertDottedPropertiesToVariable("obj1.prop \"test.html\" obj2.prop"));
        }

        [TestMethod]
        public void TestRemoveComments()
        {
            Assert.AreEqual("msg (\"Something\")", Utility.RemoveComments("msg (\"Something\")"));
            Assert.AreEqual("msg (\"Something\")", Utility.RemoveComments("msg (\"Something\")//comment"));
            Assert.AreEqual("", Utility.RemoveComments("//comment"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Utility.RemoveComments("msg (\"Something with // two slashes\")"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Utility.RemoveComments("msg (\"Something with // two slashes\")//comment"));
            Assert.AreEqual("msg (\"Something with // two slashes\")", Utility.RemoveComments("msg (\"Something with // two slashes\")//comment \"with a string\""));
        }

        [TestMethod]
        public void TestObscureStrings()
        {
            string input = "This is \"a test\" of obscuring strings";
            string result = Utility.ObscureStrings(input);
            Assert.AreEqual(input.Length, result.Length);
            Assert.IsTrue(result.StartsWith("This is \""));
            Assert.IsTrue(result.EndsWith("\" of obscuring strings"));
            Assert.IsFalse(result.Contains("a test"));
        }

        [TestMethod]
        public void TestSplitParameter()
        {
            // ("This contains two parameters, even though the first has a comma", "this is the second parameter")

            string param1 = "\"This contains two parameters, even though the first has a comma\"";
            string param2 = "\"this is the second parameter\"";
            string input = param1 + ", " + param2;
            var result = Utility.SplitParameter(input);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(param1, result[0]);
            Assert.AreEqual(param2, result[1]);
        }

        [TestMethod]
        public void TestSplitParameterWithNestedQuotes()
        {
            // ("This contains \"a nested quote, with a comma\"", "this is the second parameter")

            string param1 = "\"This contains \\\"a nested quote, with a comma\\\"\"";
            string param2 = "\"this is the second parameter\"";
            string input = param1 + ", " + param2;
            var result = Utility.SplitParameter(input);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(param1, result[0]);
            Assert.AreEqual(param2, result[1]);
        }
    }
}
