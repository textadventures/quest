using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAdventures.Quest;

namespace WorldModelTests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void TestConvertDottedProperties()
        {
            Assert.AreEqual("obj___DOT___prop", Utility.ConvertVariablesToFleeFormat("obj.prop"));
            Assert.AreEqual("obj1___DOT___prop, obj2___DOT___prop", Utility.ConvertVariablesToFleeFormat("obj1.prop, obj2.prop"));
            Assert.AreEqual("(\"myfile.html\")", Utility.ConvertVariablesToFleeFormat("(\"myfile.html\")"));
            Assert.AreEqual("\"myfile.html\"", Utility.ConvertVariablesToFleeFormat("\"myfile.html\""));
            Assert.AreEqual("obj1___DOT___prop \"test.html\" obj2___DOT___prop", Utility.ConvertVariablesToFleeFormat("obj1.prop \"test.html\" obj2.prop"));
        }

        [TestMethod]
        public void TestDecimalPointsNotConvertedToDottedProperties()
        {
            Assert.AreEqual("3.141", Utility.ConvertVariablesToFleeFormat("3.141"));
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
            Assert.AreEqual("msg (\"A quote \\\"with // two slashes\\\"\")", Utility.RemoveComments("msg (\"A quote \\\"with // two slashes\\\"\")"));
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

            //missing end quote
            try
            {
                Utility.ObscureStrings("\"missing end quote");
                Assert.Fail();
            }
            catch (MismatchingQuotesException)
            {
            }
        }

        [TestMethod]
        public void TestObscureStringsWithNestedQuotes()
        {
            string input = "This is \"a test \\\"with a nested quote\\\"\" of obscuring strings";
            string result = Utility.ObscureStrings(input);
            Assert.AreEqual(input.Length, result.Length);
            Assert.IsTrue(result.StartsWith("This is \""));
            Assert.IsTrue(result.EndsWith("\" of obscuring strings"));
            Assert.IsFalse(result.Contains("a test"));
            Assert.IsFalse(result.Contains("a nested quote"));
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

        [TestMethod]
        public void TestConvertVariableNamesWithSpaces()
        {
            Assert.AreEqual("my___SPACE___variable", Utility.ConvertVariablesToFleeFormat("my variable"));
            Assert.AreEqual("my___SPACE___variable, other___SPACE___variable", Utility.ConvertVariablesToFleeFormat("my variable, other variable"));
            Assert.AreEqual("my___SPACE___variable, \"some text\", other___SPACE___variable", Utility.ConvertVariablesToFleeFormat("my variable, \"some text\", other variable"));
            Assert.AreEqual("my___SPACE___long___SPACE___variable___SPACE___name", Utility.ConvertVariablesToFleeFormat("my long variable name"));
        }

        [TestMethod]
        public void TestNamesNearKeywordsNotConverted()
        {
            Assert.AreEqual("not my___SPACE___variable", Utility.ConvertVariablesToFleeFormat("not my variable"));
            Assert.AreEqual("my___SPACE___variable or other___SPACE___variable", Utility.ConvertVariablesToFleeFormat("my variable or other variable"));
            Assert.AreEqual("(not SomeFunction(\"hello there\"))", Utility.ConvertVariablesToFleeFormat("(not SomeFunction(\"hello there\"))"));
        }

        [TestMethod]
        public void TestIsValidAttributeName_ValidAttributes()
        {
            Assert.IsTrue(Utility.IsValidAttributeName("attribute"));
            Assert.IsTrue(Utility.IsValidAttributeName("attribute name"));
            Assert.IsTrue(Utility.IsValidAttributeName("attribute name2"));
            Assert.IsTrue(Utility.IsValidAttributeName("a"));
        }

        [TestMethod]
        public void TestIsValidAttributeName_InvalidAttributes()
        {
            Assert.IsFalse(Utility.IsValidAttributeName("attribute "));
            Assert.IsFalse(Utility.IsValidAttributeName("1attribute"));
            Assert.IsFalse(Utility.IsValidAttributeName("attri.bute"));
            Assert.IsFalse(Utility.IsValidAttributeName("this and that"));
        }

        [TestMethod]
        public void TestGetParameter()
        {
            Assert.AreEqual("\"parameter\"", Utility.GetParameter("msg (\"parameter\")"));
            Assert.AreEqual("\"parameter 1\", \"parameter 2\"", Utility.GetParameter("msg (\"parameter 1\", \"parameter 2\")"));
            Assert.AreEqual("\"parameter with a bracket ) in a string\"", Utility.GetParameter("msg (\"parameter with a bracket ) in a string\")"));
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_Variable()
        {
            string name = "somevar";
            string obj;
            string variable;
            Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual(null, obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_OneObject()
        {
            string name = "someobject.somevar";
            string obj;
            string variable;
            Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_TwoObjects()
        {
            string name = "someobject.anotherobject.somevar";
            string obj;
            string variable;
            Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject.anotherobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_ThreeObjects()
        {
            string name = "someobject.anotherobject.thirdobject.somevar";
            string obj;
            string variable;
            Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("someobject.anotherobject.thirdobject", obj);
            Assert.AreEqual("somevar", variable);
        }

        [TestMethod]
        public void TestResolveObjectDotAttribute_ThreeObjectsAndSpaces()
        {
            string name = "some object.another object.thirdobject.some var";
            string obj;
            string variable;
            Utility.ResolveObjectDotAttribute(name, out obj, out variable);
            Assert.AreEqual("some object.another object.thirdobject", obj);
            Assert.AreEqual("some var", variable);
        }
    }
}
