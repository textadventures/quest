using Microsoft.AspNetCore.StaticFiles;
using TextAdventures.Quest;

namespace WebPlayer;

public static class UiResources
{
    public static IResult GetResource(string name)
    {
        var result = PlayerHelper.GetUiResourceBytes(name);
        if (result != null)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
            return Results.Bytes(result, contentType);
        }

        return Results.StatusCode(StatusCodes.Status404NotFound);
    }
}