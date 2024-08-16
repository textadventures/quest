using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using QuestViva.PlayerCore;
using TextAdventures.Quest;

namespace WebPlayer.Components;

public partial class Runner : ComponentBase
{
    [Parameter] public required IGameDataProvider GameDataProvider { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;
    private QuestViva.PlayerCore.Player Player { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JS.InvokeVoidAsync("WebPlayer.setDotNetHelper",
            DotNetObjectReference.Create(this));
        
        var gameData = await GameDataProvider.GetData();
        
        // TODO: Is there a better way of getting libraryFolder?
        var game = GameLauncher.GetGame(gameData, null);
        
        Player = new QuestViva.PlayerCore.Player(game, GameDataProvider.ResourcesId, InvokeJs);
        
        GameResources.AddResourceProvider(GameDataProvider.ResourcesId, Player.GetResource);

        await Player.Initialise();
    }

    private async Task InvokeJs(string identifier, params object?[]? args)
    {
        await JS.InvokeVoidAsync(identifier, args);
    }

    [JSInvokable]
    public async Task UiSendCommandAsync(string command, int tickCount, IDictionary<string, string> metadata)
    {
        await Player.UiSendCommandAsync(command, tickCount, metadata);
    }

    [JSInvokable]
    public async Task UiEndWaitAsync()
    {
        await Player.UiEndWaitAsync();
    }

    [JSInvokable]
    public async Task UiEndPauseAsync()
    {
        await Player.UiEndPauseAsync();
    }

    [JSInvokable]
    public async Task UiChoiceAsync(string choice)
    {
        await Player.UiChoiceAsync(choice);
    }
    
    [JSInvokable]
    public async Task UiChoiceCancelAsync()
    {
        await Player.UiChoiceCancelAsync();
    }

    [JSInvokable]
    public async Task UiTickAsync(int tickCount)
    {
        await Player.UiTickAsync(tickCount);
    }

    [JSInvokable]
    public async Task UiSetQuestionResponseAsync(bool response)
    {
        await Player.UiSetQuestionResponseAsync(response);
    }
    
    [JSInvokable]
    public async Task UiSendEventAsync(string eventName, string param)
    {
        await Player.UiSendEventAsync(eventName, param);
    }
    
    [JSInvokable]
    public async Task UiSaveGameAsync(string html)
    {
        await Player.UiSaveGameAsync(html); 
    }
}