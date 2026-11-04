using NCalc;
using QuestViva.Engine.Expressions;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine;

// One problem GameChecker found, each one something that fails when the game runs it. Element and
// Attribute say where, when it's known: Kind is the element's kind ("object", "exit", "command",
// "function", "type", ...), and Filename the library it comes from, or null for the game file itself.
public sealed record CheckProblem(
    string Message,
    string? Kind = null,
    string? Element = null,
    string? Attribute = null,
    string? Filename = null);

// Finds problems in a loaded game without running it, for `qv check`.
//
// The engine parses a script only when it first runs (LazyLoadScript), and an expression only when
// it's first evaluated, so a game with a misspelt command or function loads cleanly and fails in
// play. GameChecker parses every script in the game's own elements up front, with a ScriptFactory
// that doesn't defer anything, and checks each expression those scripts contain: that it parses,
// and that every function it calls exists, with no more arguments than a game function takes.
// Fewer is fine in an expression: the rest are null, and Core relies on it (QuickParams).
//
// "The game's own elements" means everything except the built-in libraries (Core.aslx, the
// language files, ...), which ship with the engine: the game file's elements, and those of any
// library the author included alongside it. A published .quest file has its libraries inlined,
// so its Core functions are checked too.
//
// Object and variable names in expressions aren't checked: whether "lamp" is an object, a local
// variable or a function parameter depends on what has run before, which a static check can't
// always tell.
public static class GameChecker
{
    // Functions the expression evaluator handles itself, before looking for a game function.
    private static readonly string[] EvaluatorFunctions =
        ["__Quest_MethodCall__", "__Quest_PropertyAccess__", "__Quest_Index__", "cast", "if", "IsDefined"];

    // The .NET methods expressions can call by name, which the evaluator tries before game functions.
    private static readonly HashSet<string> BuiltInFunctions = new(
        typeof(ExpressionOwner).GetMethods()
            .Concat(typeof(Math).GetMethods())
            .Concat(typeof(StringFunctions).GetMethods())
            .Concat(typeof(DateTimeFunctions).GetMethods())
            .Select(m => m.Name),
        StringComparer.InvariantCultureIgnoreCase);

    // The world is best loaded in edit mode (InitialiseEdit), which doesn't stop at the first
    // script it can't create. Its scripts are still parsed as playing the game would parse them:
    // edit mode keeps some things as the Editor shows them, such as a switch case's comma-separated
    // values as one expression.
    public static IReadOnlyList<CheckProblem> Check(WorldModel world)
    {
        var problems = new List<CheckProblem>();

        foreach (var displaced in world.Elements.DisplacedLocalElements)
        {
            var replacement = world.Elements.Get(displaced.Name);
            problems.Add(new CheckProblem(
                $"There's more than one element called '{displaced.Name}'. Only the last one is used: " +
                $"this {Kind(displaced)} is replaced by {Article(Kind(replacement))}.",
                Kind(displaced), displaced.Name, null, LibraryFilename(displaced)));
        }

        // Every script's text first, in the mode the game was loaded in: getting the text of a
        // script that's partly parsed parses the rest, which must not happen in play mode, where
        // a script that can't be created throws.
        var scripts = new List<(CheckProblem Where, string Text)>();
        foreach (var element in world.Elements.GetElements().ToList())
        {
            if (IsBuiltIn(element))
            {
                continue;
            }

            var filename = LibraryFilename(element);
            foreach (var attribute in element.Fields.FieldNames.ToList())
            {
                switch (element.Fields.Get(attribute))
                {
                    case IScript script:
                        AddScript(Where(element, attribute, filename), script);
                        break;
                    case QuestDictionary<IScript> dictionary:
                        foreach (var (key, value) in dictionary)
                        {
                            AddScript(Where(element, $"{attribute}[\"{key}\"]", filename), value);
                        }

                        break;
                }
            }
        }

        var editMode = world.EditMode;
        world.EditMode = false;
        try
        {
            var factory = new ScriptFactory(world, lazy: false);
            foreach (var (where, text) in scripts)
            {
                CheckScript(world, factory, text, where, problems);
            }
        }
        finally
        {
            world.EditMode = editMode;
        }

        return problems;

        void AddScript(CheckProblem where, IScript script)
        {
            try
            {
                scripts.Add((where,
                    script is LazyLoadScript { UnparsedScript: { } unparsed } ? unparsed : script.Save()));
            }
            catch (Exception ex)
            {
                problems.Add(where with { Message = ex.Message });
            }
        }
    }

    private static bool IsBuiltIn(Element element) =>
        element.MetaFields[MetaFieldDefinitions.Library] &&
        element.MetaFields[MetaFieldDefinitions.Filename] is { } filename &&
        WorldModel.IsBuiltInLibrary(Path.GetFileName(filename));

    // The library an element comes from, or null for the game file itself.
    private static string? LibraryFilename(Element element) =>
        element.MetaFields[MetaFieldDefinitions.Library]
            ? Path.GetFileName(element.MetaFields[MetaFieldDefinitions.Filename])
            : null;

    private static CheckProblem Where(Element element, string attribute, string? filename) =>
        new("", Kind(element), element.Name, attribute, filename);

    private static string Kind(Element element) => element.ElemType switch
    {
        ElementType.Object => element.Type switch
        {
            ObjectType.Exit => "exit",
            ObjectType.Command => "command",
            ObjectType.Game => "game",
            ObjectType.TurnScript => "turn script",
            _ => "object"
        },
        ElementType.ObjectType => "type",
        _ => element.ElemType.ToString().ToLowerInvariant()
    };

    private static void CheckScript(WorldModel world, ScriptFactory factory, string text, CheckProblem where,
        List<CheckProblem> problems)
    {
        void OnError(object? sender, ScriptFactory.AddErrorEventArgs e) =>
            problems.Add(where with { Message = e.Error });

        // Each expression and call once per script: a script that repeats one only needs one report.
        var expressions = new HashSet<string>();
        var calls = new HashSet<(string, int)>();
        factory.ErrorHandler += OnError;
        world.ExpressionCreated = e => expressions.Add(e);
        world.FunctionCallCreated = (name, argCount) => calls.Add((name, argCount));
        try
        {
            factory.CreateScript(text, new ScriptContext(world), lazy: false);
        }
        catch (Exception ex)
        {
            problems.Add(where with { Message = ex.Message });
        }
        finally
        {
            factory.ErrorHandler -= OnError;
            world.ExpressionCreated = null;
            world.FunctionCallCreated = null;
        }

        foreach (var (name, argCount) in calls)
        {
            CheckCall(world, name, argCount, where, problems);
        }

        foreach (var expression in expressions)
        {
            CheckExpression(world, expression, where, problems);
        }
    }

    private static void CheckExpression(WorldModel world, string expression, CheckProblem where, List<CheckProblem> problems)
    {
        if (expression.Trim().Length == 0)
        {
            problems.Add(where with { Message = "An expression is empty: a condition or argument with nothing in it, such as 'if () { }' or 'MoveObject (coin, )'" });
            return;
        }

        LogicalExpression parsed;
        try
        {
            parsed = QuestNCalcExpressionFactory.GetInstance().Create(expression, ExpressionOptions.NoStringTypeCoercion);
        }
        catch (Exception ex)
        {
            var reason = ex.InnerException?.Message ?? ex.Message;
            problems.Add(where with { Message = $"Can't parse the expression '{expression}': {reason}" });
            return;
        }

        foreach (var function in Functions(parsed))
        {
            var name = function.Identifier.Name;
            var argCount = function.Parameters.Count;
            if (EvaluatorFunctions.Contains(name, StringComparer.InvariantCultureIgnoreCase) ||
                BuiltInFunctions.Contains(name))
            {
                continue;
            }

            var procedure = world.Procedure(name);
            if (procedure == null)
            {
                problems.Add(where with { Message = $"Unknown function '{name}' in '{expression}'" });
                continue;
            }

            var paramCount = procedure.Fields[FieldDefinitions.ParamNames]?.Count ?? 0;
            if (argCount > paramCount)
            {
                problems.Add(where with
                {
                    Message = $"'{name}' takes {Plural(paramCount, "parameter")}, but is given {argCount} in '{expression}'"
                });
            }
        }
    }

    // A function called as a statement, "DoThing (a, b)". Unlike a call in an expression, too few
    // arguments is an error from ASL 520 on, and with no arguments at all FunctionCallScript doesn't
    // check the count.
    private static void CheckCall(WorldModel world, string name, int argCount, CheckProblem where,
        List<CheckProblem> problems)
    {
        var procedure = world.Procedure(name);
        if (procedure == null)
        {
            problems.Add(where with { Message = $"Unknown script command or function '{name}'" });
            return;
        }

        var paramCount = procedure.Fields[FieldDefinitions.ParamNames]?.Count ?? 0;
        if (argCount == 0 || argCount == paramCount)
        {
            return;
        }

        if (argCount > paramCount || world.Version >= WorldModelVersion.v520)
        {
            problems.Add(where with
            {
                Message = $"'{name}' takes {Plural(paramCount, "parameter")}, but is given {argCount}"
            });
        }
    }

    private static string Article(string noun) => ("aeiou".Contains(noun[0]) ? "an " : "a ") + noun;

    private static string Plural(int n, string word) => n == 1 ? $"1 {word}" : $"{n} {word}s";

    private static IEnumerable<Function> Functions(LogicalExpression expression)
    {
        var children = expression switch
        {
            Function f => [..f.Parameters],
            BinaryExpression b => [b.LeftExpression, b.RightExpression],
            TernaryExpression t => [t.LeftExpression, t.MiddleExpression, t.RightExpression],
            UnaryExpression u => [u.Expression],
            LogicalExpressionList list => [..list],
            _ => new List<LogicalExpression>()
        };

        if (expression is Function function)
        {
            yield return function;
        }

        foreach (var child in children)
        {
            foreach (var f in Functions(child))
            {
                yield return f;
            }
        }
    }
}
