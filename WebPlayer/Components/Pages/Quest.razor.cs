using Microsoft.AspNetCore.Components;
using TextAdventures.Quest;

namespace WebPlayer.Components.Pages;

public partial class Quest : ComponentBase
{
    private readonly List<string> output = [];
    private string input = string.Empty;
    private string status = string.Empty;

    private PlayerHelper? playerHelper;

    private void Submit()
    {
        if (input.Length == 0) return;
        
        output.Add(input);
        input = "";
    }

    private void Load(string filename)
    {
        // TODO: Note libraryFolder is only used for .quest-save and .zip,
        // check if they are needed and if there's a better way of getting this.
        
        var game = GameLauncher.GetGame(filename, null);
        var playerHelperUi = new PlayerHelperUI(AddText);
        playerHelper = new PlayerHelper(game, playerHelperUi);
        
        // TODO: Add playerHelper event handlers

        var result = playerHelper.Initialise(playerHelperUi, out var errors);

        if (result)
        {
            status = "OK!";
            playerHelper.Game.Begin();
            AddText(playerHelper.ClearBuffer());
        }
        else
        {
            status = string.Join(", ", errors);
        }
    }

    private void LoadASL4()
    {
        Load("/Users/alexwarren/Code/quest/examples/test.asl");
    }

    private void LoadASL5()
    {
        Load("/Users/alexwarren/Code/quest/examples/test.aslx");
    }

    private void AddText(string text)
    {
        output.Add(text);
    }
}