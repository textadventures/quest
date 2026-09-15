using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;

namespace QuestViva.WebPlayer;

public static class UiResources
{
    public static IResult GetResource(string name)
    {
        var result = GetUiResourceBytes(name);
        if (result != null)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            return Results.Bytes(result, contentType);
        }

        return Results.StatusCode(StatusCodes.Status404NotFound);
    }

    private static Stream? GetUiResource(string name)
    {
        return Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"QuestViva.WebPlayer.Resources.{name}");
    }

    public static string? GetUiResourceString(string name)
    {
        using var stream = GetUiResource(name);
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static byte[]? GetUiResourceBytes(string name)
    {
        using var stream = GetUiResource(name);
        if (stream == null)
        {
            return null;
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
