using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TextAdventures.Quest;

namespace WebPlayer.Components;

public partial class Runner : ComponentBase, IPlayerHelperUI
{
    [Parameter] public required string Id { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;
    private PlayerHelper PlayerHelper = null!;
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        await JS.InvokeVoidAsync("WebPlayer.setDotNetHelper",
            DotNetObjectReference.Create(this));
        
        var filename = Id switch
        {
            "asl4" => "/Users/alexwarren/Code/quest/examples/test.asl",
            "asl5" => "/Users/alexwarren/Code/quest/examples/test.aslx",
            _ => throw new NotImplementedException()
        };

        // TODO: Note libraryFolder is only used for .quest-save and .zip,
        // check if they are needed and if there's a better way of getting this.
        
        var game = GameLauncher.GetGame(filename, null);
        PlayerHelper = new PlayerHelper(game, this);
        
        // TODO: Add playerHelper event handlers
        
        var result = PlayerHelper.Initialise(this, out var errors);

        if (result)
        {
            PlayerHelper.Game.Begin();
            await OutputTextNow(PlayerHelper.ClearBuffer());
        }
        else
        {
            // TODO: Display errors somewhere
            // string.Join(", ", errors);
        }
    }

    [JSInvokable]
    public async Task UiSendCommandAsync(string command, int tickCount, IDictionary<string, string> metadata)
    {
        PlayerHelper.SendCommand(command, tickCount, metadata);
        await OutputTextNow(PlayerHelper.ClearBuffer());
        await ClearJavaScriptBuffer();
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
        if (alignment.Length == 0) alignment = "left";
        OutputText(PlayerHelper.ClearBuffer());
        AddJavaScriptToBuffer("createNewDiv", alignment);
    }

    void IPlayerHelperUI.BindMenu(string linkid, string verbs, string text, string elementId)
    {
        AddJavaScriptToBuffer("bindMenu", linkid, verbs, text, elementId);
    }

    void IPlayer.ShowMenu(MenuData menuData)
    {
        // TODO
    }

    void IPlayer.DoWait()
    {
        // TODO
    }

    void IPlayer.DoPause(int ms)
    {
        // TODO
    }

    void IPlayer.ShowQuestion(string caption)
    {
        // TODO
    }

    void IPlayer.SetWindowMenu(MenuData menuData)
    {
        // TODO
    }

    string IPlayer.GetNewGameFile(string originalFilename, string extensions)
    {
        throw new NotImplementedException();
    }

    void IPlayer.PlaySound(string filename, bool synchronous, bool looped)
    {
        // TODO
    }

    void IPlayer.StopSound()
    {
        // TODO
    }

    void IPlayer.WriteHTML(string html)
    {
        OutputText(html);
    }

    string IPlayer.GetURL(string file)
    {
        throw new NotImplementedException();
    }

    void IPlayer.LocationUpdated(string location)
    {
        AddJavaScriptToBuffer("updateLocation", location);
    }
    

    void IPlayer.UpdateGameName(string name)
    {
        AddJavaScriptToBuffer("setGameName", name);
    }

    void IPlayer.ClearScreen()
    {
        OutputText(PlayerHelper.ClearBuffer());
        AddJavaScriptToBuffer("clearScreen");
    }

    void IPlayer.ShowPicture(string filename)
    {
        // TODO
    }

    void IPlayer.SetPanesVisible(string data)
    {
        AddJavaScriptToBuffer("panesVisible", data == "on");
    }

    void IPlayer.SetStatusText(string text)
    {
        AddJavaScriptToBuffer("updateStatus", text.Replace(Environment.NewLine, "<br />"));
    }

    void IPlayer.SetBackground(string colour)
    {
        // TODO
    }

    void IPlayer.SetForeground(string colour)
    {
        // TODO
    }

    void IPlayer.SetLinkForeground(string colour)
    {
        // TODO
    }

    void IPlayer.RunScript(string function, object[] parameters)
    {
        // TODO
    }

    void IPlayer.Quit()
    {
        // TODO
    }

    void IPlayer.SetFont(string fontName)
    {
        // TODO
    }

    void IPlayer.SetFontSize(string fontSize)
    {
        // TODO
    }

    void IPlayer.Speak(string text)
    {
        // TODO
    }

    void IPlayer.RequestSave(string html)
    {
        // TODO
    }

    void IPlayer.Show(string element)
    {
        DoShowHide(element, true);
    }

    void IPlayer.Hide(string element)
    {
        DoShowHide(element, false);
    }
    
    private static readonly Dictionary<string, string> ElementMap = new()
    {
        { "Panes", "#gamePanes" },
        { "Location", "#location" },
        { "Command", "#txtCommandDiv" }
    };

    private static string? GetElementId(string code)
    {
        ElementMap.TryGetValue(code, out var id);
        return id;
    }
    
    private void DoShowHide(string element, bool show)
    {
        var jsElement = GetElementId(element);
        if (string.IsNullOrEmpty(jsElement)) return;
        AddJavaScriptToBuffer(show ? "uiShow" : "uiHide", jsElement);
    }

    void IPlayer.SetCompassDirections(IEnumerable<string> dirs)
    {
        // TODO
    }

    void IPlayer.SetInterfaceString(string name, string text)
    {
        // TODO
    }

    void IPlayer.SetPanelContents(string html)
    {
        // TODO
    }

    void IPlayer.Log(string text)
    {
        // TODO
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