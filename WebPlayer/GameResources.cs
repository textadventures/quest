using Microsoft.AspNetCore.StaticFiles;

namespace WebPlayer;

public static class GameResources
{
    private static readonly Dictionary<string, Func<string, Stream?>> ResourceProviders = new();
    
    private static string Key(string provider, string id) => $"{provider}.{id}";

    public static void AddResourceProvider(string gameProvider, string gameId, Func<string, Stream?> resourceProvider)
    {
        if (ResourceProviders.ContainsKey(Key(gameProvider, gameId))) return;
        
        ResourceProviders[Key(gameProvider, gameId)] = resourceProvider;
    }
    
    private static readonly object ResourceStreamLock = new();
    
    public static IResult GetResource(string provider, string id, string name)
    {
        var key = Key(provider, id);
        if (!ResourceProviders.TryGetValue(key, out var resourceProvider)) return Results.StatusCode(StatusCodes.Status404NotFound);

        // Workaround for ZipArchive not being thread-safe - if multiple requests are made simultaneously for resources
        // in the same zip file, some of those requests can fail.
        lock (ResourceStreamLock)
        {
            var stream = resourceProvider(name);
            if (stream == null) return Results.StatusCode(StatusCodes.Status404NotFound);
        
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            return Results.Stream(stream, contentType);    
        }
    }
}