using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TextAdventures.Quest;

namespace WebPlayer.Components.Pages;

public partial class Quest : ComponentBase
{
    [Parameter] public required string Id { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string input = string.Empty;
    private PlayerHelper? playerHelper;

    private MarkupString uiHtml = (MarkupString) PlayerHelper.GetResource("playercore.htm");

    private void Submit()
    {
        if (input.Length == 0) return;
        // TODO...
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        
        var filename = Id switch
        {
            "asl4" => "/Users/alexwarren/Code/quest/examples/test.asl",
            "asl5" => "/Users/alexwarren/Code/quest/examples/test.aslx",
            _ => throw new NotImplementedException()
        };

        // TODO: Note libraryFolder is only used for .quest-save and .zip,
        // check if they are needed and if there's a better way of getting this.
        
        var game = GameLauncher.GetGame(filename, null);
        var playerHelperUi = new PlayerHelperUI();
        playerHelper = new PlayerHelper(game, playerHelperUi);
        
        // TODO: Add playerHelper event handlers

        var result = playerHelper.Initialise(playerHelperUi, out var errors);

        if (result)
        {
            playerHelper.Game.Begin();
            await OutputText(playerHelper.ClearBuffer());
        }
        else
        {
            // TODO: Display errors somewhere
            // string.Join(", ", errors);
        }
    }

    private async Task OutputText(string text)
    {
        if (text.Length == 0) return;
        await JS.InvokeVoidAsync("addTextAndScroll", text);
    }
}