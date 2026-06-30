using System.Text.RegularExpressions;
using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

[TestClass]
public class UtilityTests
{
    [TestMethod]
    public void TestDottedPropertiesNotEncoded()
    {
        // Dots in obj.prop expressions are now handled natively by the NCalc parser postfix rule.
        Assert.AreEqual("obj.prop", Engine.Utility.EncodeIdentifierSpaces("obj.prop"));
        Assert.AreEqual("obj1.prop, obj2.prop",
            Engine.Utility.EncodeIdentifierSpaces("obj1.prop, obj2.prop"));
        Assert.AreEqual("(\"myfile.html\")", Engine.Utility.EncodeIdentifierSpaces("(\"myfile.html\")"));
        Assert.AreEqual("\"myfile.html\"", Engine.Utility.EncodeIdentifierSpaces("\"myfile.html\""));
        Assert.AreEqual("3.141", Engine.Utility.EncodeIdentifierSpaces("3.141"));
    }

    [TestMethod]
    public void TestRemoveComments()
    {
        Assert.AreEqual("msg (\"Something\")", Engine.Utility.RemoveComments("msg (\"Something\")"));
        Assert.AreEqual("msg (\"Something\")", Engine.Utility.RemoveComments("msg (\"Something\")//comment"));
        Assert.AreEqual("", Engine.Utility.RemoveComments("//comment"));
        Assert.AreEqual("msg (\"Something with // two slashes\")",
            Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")"));
        Assert.AreEqual("msg (\"Something with // two slashes\")",
            Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")//comment"));
        Assert.AreEqual("msg (\"Something with // two slashes\")",
            Engine.Utility.RemoveComments("msg (\"Something with // two slashes\")//comment \"with a string\""));
        Assert.AreEqual("msg (\"A quote \\\"with // two slashes\\\"\")",
            Engine.Utility.RemoveComments("msg (\"A quote \\\"with // two slashes\\\"\")"));
    }

    [TestMethod]
    public void TestObscureStrings()
    {
        var input = "This is \"a test\" of obscuring strings";
        var result = Engine.Utility.ObscureStrings(input);
        Assert.AreEqual(input.Length, result.Length);
        Assert.IsTrue(result.StartsWith("This is \""));
        Assert.IsTrue(result.EndsWith("\" of obscuring strings"));
        Assert.IsFalse(result.Contains("a test"));

        //missing end quote
        try
        {
            Engine.Utility.ObscureStrings("\"missing end quote");
            Assert.Fail();
        }
        catch (MismatchingQuotesException)
        {
        }
    }

    [TestMethod]
    public void TestObscureStringsWithNestedQuotes()
    {
        var input = "This is \"a test \\\"with a nested quote\\\"\" of obscuring strings";
        var result = Engine.Utility.ObscureStrings(input);
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

        var param1 = "\"This contains two parameters, even though the first has a comma\"";
        var param2 = "\"this is the second parameter\"";
        var input = param1 + ", " + param2;
        var result = Engine.Utility.SplitParameter(input);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(param1, result[0]);
        Assert.AreEqual(param2, result[1]);
    }

    [TestMethod]
    public void TestSplitParameterWithNestedQuotes()
    {
        // ("This contains \"a nested quote, with a comma\"", "this is the second parameter")

        var param1 = "\"This contains \\\"a nested quote, with a comma\\\"\"";
        var param2 = "\"this is the second parameter\"";
        var input = param1 + ", " + param2;
        var result = Engine.Utility.SplitParameter(input);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(param1, result[0]);
        Assert.AreEqual(param2, result[1]);
    }

    [TestMethod]
    public void TestConvertVariableNamesWithSpaces()
    {
        Assert.AreEqual("my___SPACE___variable", Engine.Utility.EncodeIdentifierSpaces("my variable"));
        Assert.AreEqual("my___SPACE___variable, other___SPACE___variable",
            Engine.Utility.EncodeIdentifierSpaces("my variable, other variable"));
        Assert.AreEqual("my___SPACE___variable, \"some text\", other___SPACE___variable",
            Engine.Utility.EncodeIdentifierSpaces("my variable, \"some text\", other variable"));
        Assert.AreEqual("my___SPACE___long___SPACE___variable___SPACE___name",
            Engine.Utility.EncodeIdentifierSpaces("my long variable name"));
    }

    [TestMethod]
    public void TestNamesNearKeywordsNotConverted()
    {
        Assert.AreEqual("not my___SPACE___variable", Engine.Utility.EncodeIdentifierSpaces("not my variable"));
        Assert.AreEqual("my___SPACE___variable or other___SPACE___variable",
            Engine.Utility.EncodeIdentifierSpaces("my variable or other variable"));
        Assert.AreEqual("(not SomeFunction(\"hello there\"))",
            Engine.Utility.EncodeIdentifierSpaces("(not SomeFunction(\"hello there\"))"));
    }

    [TestMethod]
    public void TestIsValidAttributeName_ValidAttributes()
    {
        Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute"));
        Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute name"));
        Assert.IsTrue(Engine.Utility.IsValidAttributeName("attribute name2"));
        Assert.IsTrue(Engine.Utility.IsValidAttributeName("a"));
        Assert.IsTrue(Engine.Utility.IsValidFieldName("object"));
    }

    [TestMethod]
    public void TestIsValidAttributeName_InvalidAttributes()
    {
        Assert.IsFalse(Engine.Utility.IsValidAttributeName("attribute "));
        Assert.IsFalse(Engine.Utility.IsValidAttributeName("2attribute"));
        Assert.IsFalse(Engine.Utility.IsValidAttributeName("attri.bute"));
        Assert.IsFalse(Engine.Utility.IsValidAttributeName("this and that"));
        Assert.IsFalse(Engine.Utility.IsValidAttributeName("object"));
    }

    [TestMethod]
    public void TestGetParameter()
    {
        Assert.AreEqual("\"parameter\"", Engine.Utility.GetParameter("msg (\"parameter\")"));
        Assert.AreEqual("\"parameter 1\", \"parameter 2\"",
            Engine.Utility.GetParameter("msg (\"parameter 1\", \"parameter 2\")"));
        Assert.AreEqual("\"parameter with a bracket ) in a string\"",
            Engine.Utility.GetParameter("msg (\"parameter with a bracket ) in a string\")"));
    }

    [TestMethod]
    public void TestSplitParameter_UnmatchedClosingBracket()
    {
        var result = Engine.Utility.SplitParameter("a), b");
        result.Count.ShouldBe(2);
        result[0].ShouldBe("a)");
        result[1].ShouldBe("b");
    }

    [TestMethod]
    public void TestConvertVerbSimplePattern_EscapesVerbMetacharacters()
    {
        var pattern = Engine.Utility.ConvertVerbSimplePattern("pick.up", null);
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        regex.IsMatch("pick.up book").ShouldBeTrue();
        regex.IsMatch("pickXup book").ShouldBeFalse();
    }

    [TestMethod]
    public void TestConvertVerbSimplePattern_EscapesSeparatorMetacharacters()
    {
        var pattern = Engine.Utility.ConvertVerbSimplePattern("put", "with;a.k.a.");
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        // Literal separator "a.k.a." should populate object2
        var literalSep = regex.Match("put book a.k.a. box");
        literalSep.Success.ShouldBeTrue();
        literalSep.Groups["object2"].Value.ShouldBe("box");

        // "aXkXaX" should NOT be treated as the separator (dot is not a wildcard)
        var wildcardSep = regex.Match("put book aXkXaX box");
        wildcardSep.Success.ShouldBeTrue();
        wildcardSep.Groups["object2"].Value.ShouldBe(""); // absorbed into object, not a separator match
    }

    [TestMethod]
    public void TestIndentScript()
    {
        var script = "if (true) {\nmsg(\"hello\")\n}";
        var result = Engine.Utility.IndentScript(script, 0, "  ");
        result.ShouldBe(
            Environment.NewLine +
            "if (true) {" + Environment.NewLine +
            "  msg(\"hello\")" + Environment.NewLine +
            "}" + Environment.NewLine);
    }

    [TestMethod]
    public void TestIndentScript_IgnoresBracesInStrings()
    {
        // A '{' inside a string literal must not increase the indent level for subsequent lines
        var script = "msg(\"before { after\")\nmsg(\"end\")";
        var result = Engine.Utility.IndentScript(script, 0, "  ");
        result.ShouldBe(
            Environment.NewLine +
            "msg(\"before { after\")" + Environment.NewLine +
            "msg(\"end\")" + Environment.NewLine);
    }

}