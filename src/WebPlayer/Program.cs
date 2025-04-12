using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.PlayerCore;
using QuestViva.WebPlayer;
using QuestViva.WebPlayer.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o =>
    {
        o.EnableDetailedErrors = builder.Environment.IsDevelopment();
        // Set max message size to 5MB (the default is 32KB, which is too small to receive save game data)
        o.MaximumReceiveMessageSize = 5 * 1024 * 1024;
    });

builder.Services.Configure<WebPlayerConfig>(builder.Configuration);
builder.Services.AddSingleton<Config>();
builder.Services.AddSingleton<IConfig>(sp => sp.GetRequiredService<Config>());
builder.Services.AddSingleton<ITextAdventuresConfig>(sp => sp.GetRequiredService<Config>());
builder.Services.AddSingleton<WorldModelFactory>();
builder.Services.AddSingleton<GameLauncher>();

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
app.MapGet("/game/{resourcesId}/{name}", LocalResources.GetResource);
app.MapGet("/Play.aspx", async context =>
{
    var id = (string?)context.Request.Query["id"];
    if (!string.IsNullOrEmpty(id))
    {
        context.Response.Redirect(id.StartsWith("editor/") ? $"/editor/{id[7..]}" : $"/textadventures/{id}");
    }
    else
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing id parameter");
    }
});

app.Run();