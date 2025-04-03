using Microsoft.AspNetCore.StaticFiles;

namespace QuestViva.WebPlayer;

public static class LocalResources
{
    private static readonly Dictionary<string, Func<string, Stream?>> ResourceStreamProviders = new();

    public static void AddResourceStreamProvider(string resourcesId, Func<string, Stream?> resourceStreamProvider)
    {
        ResourceStreamProviders.TryAdd(resourcesId, resourceStreamProvider);
    }
    
    public static IResult GetResource(string resourcesId, string name)
    {
        if (!ResourceStreamProviders.TryGetValue(resourcesId, out var resourceStreamProvider))
            return Results.StatusCode(StatusCodes.Status404NotFound);
        
        var stream = resourceStreamProvider(name);
        if (stream == null) return Results.StatusCode(StatusCodes.Status404NotFound);
    
        new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
        return Results.Stream(stream, contentType);    
    }
}