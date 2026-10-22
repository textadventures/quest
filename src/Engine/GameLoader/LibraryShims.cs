using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace QuestViva.Engine.GameLoader;

// Published games carry their own copy of the Core library, frozen as it was when they were last
// saved (see "Core Library Semantics" in CLAUDE.md), so fixing a Core function only reaches games
// saved after the fix. A shim is the exception: when a game is loaded to play, it patches one
// inlined library function so that a fix reaches games already published.
//
// Each shim applies only when both of these hold:
// - The game's ASL version is at or below AppliesUpTo. Games saved later carry the fixed library.
// - The function's body matches one of StockBodyHashes, the bodies that shipped in a release.
//   A package doesn't record which functions came from the library, so this is the only way to
//   tell a stock function from one the author rewrote. Anything unrecognised is left alone.
//
// Shims are frozen once added. Never change an existing entry's hashes or its resource file to
// track later edits to the library function: those edits haven't been tested against older
// games, and the shim would push them into every one. A fix for a later version gets a new entry.
internal static partial class LibraryShims
{
    private record Shim(
        WorldModelVersion AppliesUpTo,
        string FunctionName,
        string[] StockBodyHashes,
        string Resource);

    private static readonly Shim[] Shims =
    [
        // #2189: place rooms on the map that the player reaches without walking through a known exit
        new(WorldModelVersion.v600, "Grid_CalculateMapCoordinates",
            [
                "0748afc537fbdf0379598b6b805adc92a905209da5279723ecbefc90226878b8", // Quest 5.5 beta
                "f3396d4dd03e7ec1e5bee7c9718f4c050ba9b6481904f4102c46080a9130cd20", // Quest 5.5.1 to 5.9
                "c1c7650d335c3b5cdf9cbcfb9761b5a9e40dac40d3f886893cec7a5e4e66440f", // Quest 5.10 to Quest Viva 6.0 rc
                "a452fdbb0f4e80be6f42cadab415cc32a4aee17994ef0601365906ccfd34c9b5", // Quest Viva 6.0.0
            ],
            "Grid_CalculateMapCoordinates.v600.xml"),
    ];

    public static void Apply(WorldModel worldModel)
    {
        foreach (var shim in Shims)
        {
            if (worldModel.Version > shim.AppliesUpTo)
            {
                continue;
            }

            if (!worldModel.Elements.TryGetValue(ElementType.Function, shim.FunctionName, out var function))
            {
                continue;
            }

            var property = FieldDefinitions.Script.Property;
            var body = function.Fields.LazyFields.GetScript(property);
            if (body == null || !shim.StockBodyHashes.Contains(HashBody(body)))
            {
                continue;
            }

            var (before, after) = LoadResource(shim.Resource);
            function.Fields.LazyFields.ReplaceScript(property, $"{before}\n{body}\n{after}");
            Debug.WriteLine($"Applied library shim to {shim.FunctionName}");
        }
    }

    // Packages hold each script as re-serialised by whichever Quest version saved them, so
    // indentation and line breaks vary. Only whitespace is ignored.
    internal static string HashBody(string body)
    {
        var normalised = WhitespaceRegex().Replace(body, " ").Trim();
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalised)));
    }

    private static (string Before, string After) LoadResource(string resource)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QuestViva.Engine.Shims." + resource)
                           ?? throw new InvalidOperationException($"Missing library shim resource {resource}");
        using var reader = XmlReader.Create(stream);
        reader.ReadToFollowing("before");
        var before = reader.ReadElementContentAsString();
        reader.ReadToFollowing("after");
        var after = reader.ReadElementContentAsString();
        return (before, after);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
