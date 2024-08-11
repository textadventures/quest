using Microsoft.AspNetCore.StaticFiles;

namespace WebPlayer;

public static class GameResources
{
    private static readonly Dictionary<string, byte[]> Resources = new();
    
    private static string Key(string provider, string id, string name) => $"{provider}.{id}.{name}";

    public static void AddResource(string provider, string id, string name, Stream data)
    {
        if (Resources.ContainsKey(Key(provider, id, name))) return;
        
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        Resources[Key(provider, id, name)] = ms.ToArray();
    }
    
    public static IResult GetResource(string provider, string id, string name)
    {
        var key = Key(provider, id, name);
        if (!Resources.TryGetValue(key, out var bytes)) return Results.StatusCode(StatusCodes.Status404NotFound);
        new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
        return Results.Bytes(bytes, contentType);
    }
}