using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace QuestViva.PlayerCore;

public class BlazorJSInterop(Player player)
{
    private Player Player { get; } = player;
    
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

    [JSInvokable]
    public async Task ShowDebugger()
    {
        await Player.ShowDebugger();
    }
}