using Microsoft.AspNetCore.StaticFiles;
using TextAdventures.Quest;
using WebPlayer.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/res/{name}", (string name) =>
{
    var result = PlayerHelper.GetResource(name);
    if (result != null)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(name, out var contentType);
        return Results.Content(result, contentType);
    }
 
    return Results.StatusCode(StatusCodes.Status404NotFound);
});

app.Run();