using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TextAdventures.Quest;

namespace WebPlayer.Components;

public partial class Runner : ComponentBase, IPlayerHelperUI
{
    [Parameter] public required string Id { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;
    private PlayerHelper PlayerHelper { get; set; } = null!;
    private List<(string, object?[]?)> JavaScriptBuffer { get; } = [];
    private bool Finished { get; set; } = false;
    private ListHandler ListHandler { get; }
    
    public Runner()
    {
        ListHandler = new ListHandler(AddJavaScriptToBuffer);
    }
    
    private void AddJavaScriptToBuffer(string identifier, params object?[]? args)
    {
        JavaScriptBuffer.Add((identifier, args));
    }

    private async Task ClearJavaScriptBuffer()
    {
        foreach (var (identifier, args) in JavaScriptBuffer)
        {
            await JS.InvokeVoidAsync(identifier, args);
        }
        JavaScriptBuffer.Clear();
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
        PlayerHelper = new PlayerHelper(game, this)
        {
            UseGameColours = true,
            UseGameFont = true
        };

        PlayerHelper.Game.LogError += LogError;
        PlayerHelper.Game.UpdateList += UpdateList;
        PlayerHelper.Game.Finished += GameFinished;
        if (game is IASLTimer gameTimer)
        {
            gameTimer.RequestNextTimerTick += RequestNextTimerTick;
        }
        
        var result = PlayerHelper.Initialise(this, out var errors);

        if (result)
        {
            PlayerHelper.Game.Begin();
            await ClearBuffer();
        }
        else
        {
            throw new NotImplementedException();
            // TODO: Display errors somewhere
            // string.Join(", ", errors);
        }
    }

    private void RequestNextTimerTick(int seconds)
    {
        AddJavaScriptToBuffer("requestNextTimerTick", seconds);
    }

    private void LogError(string errormessage)
    {
        throw new NotImplementedException();
    }

    private void UpdateList(ListType listType, List<ListData> items)
    {
        ListHandler.UpdateList(listType, items);
    }
    
    private void GameFinished()
    {
        throw new NotImplementedException();
    }

    private async Task UiActionAsync(Action action)
    {
        if (Finished) return;
        action();
        await ClearBuffer();
    }

    [JSInvokable]
    public async Task UiSendCommandAsync(string command, int tickCount, IDictionary<string, string> metadata)
    {
        await UiActionAsync(() => PlayerHelper.SendCommand(command, tickCount, metadata));
    }

    [JSInvokable]
    public async Task UiEndWaitAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.FinishWait());
    }

    [JSInvokable]
    public async Task UiEndPauseAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.FinishPause());
    }

    [JSInvokable]
    public async Task UiChoiceAsync(string choice)
    {
        await UiActionAsync(() => PlayerHelper.Game.SetMenuResponse(choice));
    }

    [JSInvokable]
    public async Task UiChoiceCancelAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.SetMenuResponse(null));
    }

    [JSInvokable]
    public async Task UiTickAsync(int tickCount)
    {
        await UiActionAsync(() => PlayerHelper.GameTimer.Tick(tickCount));
    }
    
    // TODO: Other UiActions:
    /*
       case "msgbox":
           m_player.SetQuestionResponse(args[1]);
           break;
       case "event":
           SendEvent(args[1]);
           break;
       case "tick":
           m_player.Tick(tickCount);
           break;
       case "save":
           string unescapedHtml = args[1].Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
           m_player.RequestSave(unescapedHtml);
           break;
     */

    private async Task ClearBuffer()
    {
        OutputText(PlayerHelper.ClearBuffer());
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
        AddJavaScriptToBuffer("showMenu", menuData.Caption, menuData.Options, menuData.AllowCancel);
    }

    void IPlayer.DoWait()
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.DoPause(int ms)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.ShowQuestion(string caption)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.SetWindowMenu(MenuData menuData)
    {
        // Do nothing - only implemented for desktop player
    }

    string IPlayer.GetNewGameFile(string originalFilename, string extensions)
    {
        throw new NotImplementedException();
    }

    void IPlayer.PlaySound(string filename, bool synchronous, bool looped)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.StopSound()
    {
        // TODO
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    void IPlayer.SetForeground(string colour)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.SetLinkForeground(string colour)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.RunScript(string function, object[] parameters)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.Quit()
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.SetFont(string fontName)
    {
        PlayerHelper.SetFont(fontName);
    }

    void IPlayer.SetFontSize(string fontSize)
    {
        PlayerHelper.SetFontSize(fontSize);
    }

    void IPlayer.Speak(string text)
    {
        // Do nothing
    }

    void IPlayer.RequestSave(string html)
    {
        // TODO
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    void IPlayer.SetInterfaceString(string name, string text)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.SetPanelContents(string html)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.Log(string text)
    {
        // TODO
        AddJavaScriptToBuffer("console.log", text);
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