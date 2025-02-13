using System.IO;
using System.Reflection;

namespace QuestViva.Common;

public static class VersionInfo
{
    private static string? _version;
    public static string Version => _version ??= GetVersion();

    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "QuestViva.Common.VERSION";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return "Unknown";

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }
}