using System.Text.Json;
using System.Text.RegularExpressions;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public partial class ProblemTextTests
{
    [TestMethod]
    public void EnglishFillsInArgs()
    {
        var text = ProblemText.Of("unknownFunction", ("name", "Foo"), ("expression", "Foo(1)"));
        Assert.AreEqual("Unknown function 'Foo' in 'Foo(1)'", text.ToEnglish());
    }

    [TestMethod]
    public void CountChoosesThePluralForm()
    {
        Assert.AreEqual("'f' takes 1 parameter, but is given 2",
            ProblemText.Of("callArgumentCount", ("name", "f"), ("count", 1), ("arguments", 2)).ToEnglish());
        Assert.AreEqual("'f' takes 0 parameters, but is given 2",
            ProblemText.Of("callArgumentCount", ("name", "f"), ("count", 0), ("arguments", 2)).ToEnglish());
    }

    [TestMethod]
    public void ReasonFillsInTheReason()
    {
        var text = ProblemText.Of("scriptError", ("script", "if (x)) {}"), ("reason", "ignored")) with
        {
            Reason = ProblemText.Of("tooManyCloseBrackets")
        };
        Assert.AreEqual("Error adding script 'if (x)) {}': Too many ')'", text.ToEnglish());
    }

    [TestMethod]
    public void AScriptParseExceptionsMessageIsItsEnglish()
    {
        var ex = new ScriptParseException("missingBracket", ("bracket", ')'));
        Assert.AreEqual("Missing ')'", ex.Message);
        Assert.AreEqual("missingBracket", ex.Text.Code);
    }

    [TestMethod]
    public void MismatchingQuotesCanBeTranslated()
    {
        var ex = Assert.ThrowsException<MismatchingQuotesException>(() => Utility.ObscureStrings("\"abc"));
        Assert.AreEqual("missingQuote", ex.Text.Code);
        StringAssert.StartsWith(ex.Message, "Missing quote character in ");
    }

    // The Editor's locale files translate each template as "checkProblem.<code>". English must say
    // the same as the engine, and every other locale must have every template, with the same
    // placeholders.
    [TestMethod]
    public void EveryLocaleHasEveryTemplate()
    {
        var locales = LocalesFolder();
        var en = Templates(Path.Combine(locales, "en.json"));
        CollectionAssert.AreEquivalent(ProblemText.EnglishTemplates.Keys.ToList(), en.Keys.ToList());
        foreach (var (code, template) in ProblemText.EnglishTemplates)
        {
            Assert.AreEqual(template, en[code], code);
        }

        foreach (var file in Directory.GetFiles(locales, "*.json").Where(f => Path.GetFileName(f) != "en.json"))
        {
            var translated = Templates(file);
            CollectionAssert.AreEquivalent(en.Keys.ToList(), translated.Keys.ToList(), Path.GetFileName(file));
            foreach (var (code, template) in en)
            {
                CollectionAssert.AreEquivalent(Placeholders(template), Placeholders(translated[code]),
                    $"{Path.GetFileName(file)}: {code}");
            }
        }
    }

    private static Dictionary<string, string> Templates(string file) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file))!
            .Where(e => e.Key.StartsWith("checkProblem.") && !e.Key.StartsWith("checkProblem.kind."))
            .ToDictionary(e => e.Key["checkProblem.".Length..], e => e.Value);

    private static List<string> Placeholders(string template) =>
        Placeholder().Matches(template).Select(m => m.Value).Distinct().ToList();

    private static string LocalesFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "QuestViva.sln")))
        {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir, "Couldn't find the repository root");
        return Path.Combine(dir.FullName, "src", "AppShell", "static", "locales");
    }

    [GeneratedRegex(@"\{\w+\}")]
    private static partial Regex Placeholder();
}
