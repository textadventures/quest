using MudBlazor.Services;
using QuestViva.Common;
using QuestViva.WebEditor;
using QuestViva.WebEditor.Components;
using QuestViva.WebEditor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(o =>
    {
        o.EnableDetailedErrors = builder.Environment.IsDevelopment();
        o.MaximumReceiveMessageSize = 5 * 1024 * 1024;
    });

builder.Services.AddMudServices();

builder.Services.AddSingleton<IConfig, EditorConfig>();
builder.Services.AddScoped<EditorService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
