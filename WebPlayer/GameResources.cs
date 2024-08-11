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
    
    public static IResult GetResource(string provider, string id, string name)
    {
        var key = Key(provider, id);
        if (!ResourceProviders.TryGetValue(key, out var resourceProvider)) return Results.StatusCode(StatusCodes.Status404NotFound);
        
        var stream = resourceProvider(name);
        if (stream == null) return Results.StatusCode(StatusCodes.Status404NotFound);
        
        new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
        return Results.Stream(stream, contentType);
    }
}