using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TextAdventures.Quest;

namespace WebPlayer.Components.Pages;

public partial class Quest : ComponentBase, IPlayerHelperUI
{
    [Parameter] public required string Id { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string input = string.Empty;

    private MarkupString uiHtml = (MarkupString) PlayerHelper.GetResource("playercore.htm");
    
    private readonly List<(string, object?[]?)> _javaScriptBuffer = [];
    
    private void AddJavaScriptToBuffer(string identifier, params object?[]? args)
    {
        _javaScriptBuffer.Add((identifier, args));
    }

    private async Task ClearJavaScriptBuffer()
    {
        foreach (var (identifier, args) in _javaScriptBuffer)
        {
            await JS.InvokeVoidAsync(identifier, args);
        }
        _javaScriptBuffer.Clear();
    }

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
        var playerHelper = new PlayerHelper(game, this);
        
        // TODO: Add playerHelper event handlers
        
        var result = playerHelper.Initialise(this, out var errors);

        if (result)
        {
            playerHelper.Game.Begin();
            await OutputTextNow(playerHelper.ClearBuffer());
        }
        else
        {
            // TODO: Display errors somewhere
            // string.Join(", ", errors);
        }
    }

    private async Task OutputTextNow(string text)
    {
        OutputText(text);
        await ClearJavaScriptBuffer();
    }
    
    private void OutputText(string text)
    {
        if (text.Length == 0) return;
        AddJavaScriptToBuffer("addTextAndScroll", text);
    }

    void IPlayerHelperUI.SetAlignment(string alignment)
    {
    }

    void IPlayerHelperUI.BindMenu(string linkid, string verbs, string text, string elementId)
    {
    }

    void IPlayer.ShowMenu(MenuData menuData)
    {
    }

    void IPlayer.DoWait()
    {
    }

    void IPlayer.DoPause(int ms)
    {
    }

    void IPlayer.ShowQuestion(string caption)
    {
    }

    void IPlayer.SetWindowMenu(MenuData menuData)
    {
    }

    string IPlayer.GetNewGameFile(string originalFilename, string extensions)
    {
        throw new NotImplementedException();
    }

    void IPlayer.PlaySound(string filename, bool synchronous, bool looped)
    {
    }

    void IPlayer.StopSound()
    {
    }

    void IPlayer.WriteHTML(string html)
    {
    }

    string IPlayer.GetURL(string file)
    {
        throw new NotImplementedException();
    }

    void IPlayer.LocationUpdated(string location)
    {
    }

    void IPlayer.UpdateGameName(string name)
    {
        AddJavaScriptToBuffer("setGameName", name);
    }

    void IPlayer.ClearScreen()
    {
    }

    void IPlayer.ShowPicture(string filename)
    {
    }

    void IPlayer.SetPanesVisible(string data)
    {
    }

    void IPlayer.SetStatusText(string text)
    {
    }

    void IPlayer.SetBackground(string colour)
    {
    }

    void IPlayer.SetForeground(string colour)
    {
    }

    void IPlayer.SetLinkForeground(string colour)
    {
    }

    void IPlayer.RunScript(string function, object[] parameters)
    {
    }

    void IPlayer.Quit()
    {
    }

    void IPlayer.SetFont(string fontName)
    {
    }

    void IPlayer.SetFontSize(string fontSize)
    {
    }

    void IPlayer.Speak(string text)
    {
    }

    void IPlayer.RequestSave(string html)
    {
    }

    void IPlayer.Show(string element)
    {
    }

    void IPlayer.Hide(string element)
    {
    }

    void IPlayer.SetCompassDirections(IEnumerable<string> dirs)
    {
    }

    void IPlayer.SetInterfaceString(string name, string text)
    {
    }

    void IPlayer.SetPanelContents(string html)
    {
    }

    void IPlayer.Log(string text)
    {
    }

    string IPlayer.GetUIOption(UIOption option)
    {
        throw new NotImplementedException();
    }

    void IPlayerHelperUI.OutputText(string text)
    {
        OutputText(text);
    }
}