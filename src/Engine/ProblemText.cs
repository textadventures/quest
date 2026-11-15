using System.Globalization;
using System.Text.RegularExpressions;

namespace QuestViva.Engine;

// A problem's message in a form the Editor can translate. Code names a template, and Args fill in
// its {placeholders}. The Editor looks the template up as "checkProblem.<Code>" in its locale files.
// The English templates are below, and ToEnglish() builds the message `qv check` and the engine's
// own errors show from them, so both say the same thing.
//
// Two placeholders are special. {reason} is filled by Reason, another message, when there is one
// (an error inside a script that couldn't be added), and otherwise by Args["reason"], text that
// can't be translated, such as NCalc's parse errors. A template with ".one" and ".other" forms is
// chosen by Args["count"], as the Editor's tPlural does.
public sealed partial record ProblemText(
    string Code,
    IReadOnlyDictionary<string, string> Args,
    ProblemText? Reason = null)
{
    public static readonly IReadOnlyDictionary<string, string> EnglishTemplates = new Dictionary<string, string>
    {
        // GameChecker
        ["duplicateElement"] =
            "There's more than one element called '{name}'. Only the last one is used: this {kind} is replaced by " +
            "the {replacementKind} defined after it.",
        ["hiddenByEngineFunction"] =
            "'{name}' is also the name of a function built into the engine. An expression such as x = {name}(...) " +
            "calls the built-in one, so this function's return value is never used. Give it a different name.",
        ["verbSharesBuiltInAttribute"] =
            "This verb{pattern} runs the attribute '{attribute}', and so does the built-in verb '{other}' in " +
            "{filename}. Two verbs can't share an attribute: the game shows an error as it starts, and leaves some " +
            "verbs out of objects' verb lists. Remove this verb, and give objects a '{attribute}' script or text for " +
            "the built-in one to use.",
        ["verbSharesAttribute"] =
            "This verb{pattern} runs the attribute '{attribute}', and so does the verb '{other}'. Two verbs can't " +
            "share an attribute: the game shows an error as it starts, and leaves some verbs out of objects' verb " +
            "lists. Give one of them a different attribute.",
        ["verbSharesAttributeInLibrary"] =
            "This verb{pattern} runs the attribute '{attribute}', and so does the verb '{other}' in {filename}. Two " +
            "verbs can't share an attribute: the game shows an error as it starts, and leaves some verbs out of " +
            "objects' verb lists. Give one of them a different attribute.",
        ["emptyExpression"] =
            "An expression is empty: a condition or argument with nothing in it, such as 'if () { }' or " +
            "'MoveObject (coin, )'",
        ["unparseableExpression"] = "Can't parse the expression '{expression}': {reason}",
        ["unknownFunction"] = "Unknown function '{name}' in '{expression}'",
        ["expressionArgumentCount.one"] = "'{name}' takes {count} parameter, but is given {arguments} in '{expression}'",
        ["expressionArgumentCount.other"] = "'{name}' takes {count} parameters, but is given {arguments} in '{expression}'",
        ["unknownCommandOrFunction"] = "Unknown script command or function '{name}'",
        ["callArgumentCount.one"] = "'{name}' takes {count} parameter, but is given {arguments}",
        ["callArgumentCount.other"] = "'{name}' takes {count} parameters, but is given {arguments}",

        // ScriptFactory
        ["scriptError"] = "Error adding script '{script}': {reason}",
        ["unexpectedElse"] = "Unexpected 'else' (error with parent 'if'?):{script}",
        ["unexpectedOtherwise"] = "Unexpected 'otherwise' (error with parent 'firsttime'?):{script}",
        ["unrecognisedCommand"] = "Unrecognised script command '{script}'",

        // ScriptParseException
        ["parameterCount"] = "Expected {expected} parameter(s) in script '{script}'",
        ["parameterListExpected"] = "Expected a parameter list in '{script}'",
        ["missingBracket"] = "Missing '{bracket}'",
        ["missingQuote"] = "Missing quote character in {text}",
        ["tooManyCloseBrackets"] = "Too many ')'",
        ["askParameterCount"] = "'ask' script should have 1 parameter: 'ask ({parameters})'",
        ["foreachParameterCount"] = "'foreach' script should have 2 parameters: 'foreach ({parameters})'",
        ["forParameterCount"] = "'for' script should have 3 or 4 parameters: 'for ({parameters})'",
        ["showMenuParameterCount"] = "'show menu' script should have 3 parameters: 'show menu ({parameters})'",
        ["rundelegateParameterCount"] = "Expected at least 2 parameters in rundelegate call",
        ["invalidInSwitch"] = "Invalid inside switch block: '{text}'",
        ["duplicateSwitchCase"] = "'switch' block contains duplicate case '{case}'",
        ["invalidJsFunctionName"] = "Invalid JS function name in '{script}'",
        ["invalidVariableName"] = "Invalid variable name '{name}' in '{script}'",
        ["invalidAttributeName"] = "Invalid attribute name '{name}' in '{script}'",
    };

    public static ProblemText Of(string code, params (string Name, object Value)[] args) =>
        new(code, args.ToDictionary(a => a.Name, a => Convert.ToString(a.Value, CultureInfo.InvariantCulture) ?? ""));

    public string ToEnglish()
    {
        var key = Args.TryGetValue("count", out var count) ? $"{Code}.{(count == "1" ? "one" : "other")}" : Code;
        var template = EnglishTemplates[key];
        return Placeholder().Replace(template, m =>
        {
            var name = m.Groups[1].Value;
            if (name == "reason" && Reason != null)
            {
                return Reason.ToEnglish();
            }

            return Args.TryGetValue(name, out var value) ? value : m.Value;
        });
    }

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex Placeholder();
}

// An error parsing a script, whose message the Editor's Check game can translate. Its Message is
// the English one, as before.
public class ScriptParseException(ProblemText text) : Exception(text.ToEnglish())
{
    public ProblemText Text { get; } = text;

    public ScriptParseException(string code, params (string Name, object Value)[] args)
        : this(ProblemText.Of(code, args))
    {
    }
}
