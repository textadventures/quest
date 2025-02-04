using Microsoft.AspNetCore.StaticFiles;

namespace QuestViva.WebPlayer;

public static class GameResources
{
    private static readonly Dictionary<string, Func<string, Stream?>> ResourceProviders = new();

    public static void AddResourceProvider(string resourcesId, Func<string, Stream?> resourceProvider)
    {
        ResourceProviders.TryAdd(resourcesId, resourceProvider);
    }
    
    private static readonly object ResourceStreamLock = new();
    
    public static IResult GetResource(string resourcesId, string name)
    {
        if (!ResourceProviders.TryGetValue(resourcesId, out var resourceProvider))
            return Results.StatusCode(StatusCodes.Status404NotFound);

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