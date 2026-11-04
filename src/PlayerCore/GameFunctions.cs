using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.PlayerCore;

// A function a game can call, and where its definition comes from. Library is the library file it's
// defined in, or null for the game file itself; BuiltIn says whether that library ships with Quest
// (Core.aslx, English.aslx, ...) rather than being one the author included. Overrides is the library
// whose function of the same name the game's own copy replaces, when it is one. Script is the
// function's body, as ASL.
public sealed record FunctionInfo(
    string Name,
    IReadOnlyList<string> Parameters,
    string? ReturnType,
    string? Library,
    bool BuiltIn,
    string? Overrides,
    string Script);

// Reads the functions a game defines, including those of its libraries, from its file as it is now.
//
// Where a function comes from matters to an author: a game that's still being edited (.aslx) gets
// Core's functions from the copy built into the engine, so they change when Quest does, unless the
// game has its own copy of one, which then stays as it is. A published .quest file has its libraries
// inlined, so every function in it loads as the game's own and Library is always null.
public static class GameFunctions
{
    private static readonly string[] Quest4Extensions = [".asl", ".cas", ".qsg"];

    public static async Task<IReadOnlyList<FunctionInfo>> LoadAsync(GameData gameData)
    {
        if (Quest4Extensions.Contains(Path.GetExtension(gameData.Filename).ToLowerInvariant()))
        {
            throw new NotSupportedException("Games from Quest 4 and earlier have no functions to list.");
        }

        var world = new WorldModel(gameData, null);
        if (!await world.InitialiseEdit())
        {
            throw new InvalidOperationException(
                "The game didn't load: " + string.Join("; ", world.Errors) + ". Run check to see why.");
        }

        return world.Elements.GetElements(ElementType.Function)
            .Select(f => Describe(world, f))
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static FunctionInfo Describe(WorldModel world, Element function)
    {
        var library = LibraryOf(function);
        var overridden = world.Elements.GetOverriddenLibraryElement(function.Name);
        string script;
        try
        {
            script = function.Fields[FieldDefinitions.Script]?.Save() ?? "";
        }
        catch (Exception ex)
        {
            script = $"[The script couldn't be read: {ex.Message}]";
        }

        return new FunctionInfo(
            function.Name,
            function.Fields[FieldDefinitions.ParamNames]?.ToList() ?? [],
            function.Fields[FieldDefinitions.ReturnType],
            library,
            library != null && WorldModel.IsBuiltInLibrary(library),
            overridden != null ? LibraryOf(overridden) : null,
            script);
    }

    private static string? LibraryOf(Element element) =>
        element.MetaFields[MetaFieldDefinitions.Library]
            ? Path.GetFileName(element.MetaFields[MetaFieldDefinitions.Filename])
            : null;
}
