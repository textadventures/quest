using Microsoft.AspNetCore.StaticFiles;

namespace QuestViva.WebPlayer;

public static class LocalResources
{
    private static readonly Dictionary<string, Func<string, Stream?>> ResourceProviders = new();

    public static void AddResourceProvider(string resourcesId, Func<string, Stream?> resourceProvider)
    {
        ResourceProviders.TryAdd(resourcesId, resourceProvider);
    }
    
    public static IResult GetResource(string resourcesId, string name)
    {
        if (!ResourceProviders.TryGetValue(resourcesId, out var resourceProvider))
            return Results.StatusCode(StatusCodes.Status404NotFound);
        
        var stream = resourceProvider(name);
        if (stream == null) return Results.StatusCode(StatusCodes.Status404NotFound);
    
        new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
        return Results.Stream(stream, contentType);    
    }
}