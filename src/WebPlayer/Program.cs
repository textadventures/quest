using Microsoft.AspNetCore.StaticFiles;
using TextAdventures.Quest;
using WebPlayer;
using WebPlayer.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();

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

app.MapGet("/res/{name}", UiResources.GetResource);
app.MapGet("/res/{dir}/{name}", (string dir, string name) => UiResources.GetResource($"{dir}.{name}"));
app.MapGet("/res/{dir1}/{dir2}/{name}", (string dir1, string dir2, string name) => UiResources.GetResource($"{dir1}.{dir2}.{name}"));

app.Map("/game/{resourcesId}/{name}", GameResources.GetResource);

app.Run();