namespace QuestViva.Common;

public delegate void PrintTextHandler(string text);

public delegate void UpdateListHandler(ListType listType, List<ListData> items);

public delegate void FinishedHandler();

public delegate void ErrorHandler(Exception ex);

public interface IGame
{
    List<string> Errors { get; }
    string? GameID { get; }
    Task<bool> Initialise(IPlayer player);
    Task Begin();
    Task SendCommand(string command);
    Task SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata);
    Task SendEvent(string eventName, string param);
    event PrintTextHandler? PrintText;
    event UpdateListHandler? UpdateList;
    event FinishedHandler? Finished;
    event ErrorHandler? LogError;

    // Raised on every genuine suspension boundary (wait/get input/ask/show menu opening or
    // resolving), not just once per SendCommand/Begin/etc. call - a player implementation that
    // buffers JS calls (e.g. WasmPlayer's WasmPlayerBridge) should flush on every occurrence,
    // not only when the outermost await returns. A script can open its *next* suspension (e.g.
    // a puzzle loop's next get input) before the one currently resolving has finished draining
    // deferred on-ready work, so the first signal after a command starts isn't necessarily the
    // last one relevant to that command's own output.
    event Action? TurnSuspended;
    void Finish();
    Task<byte[]> SaveAsync(string html);
    Task FinishWait();
    Task FinishPause();

    Task SetMenuResponse(string? response);
    Task SetQuestionResponse(bool response);

    event Action<int>? RequestNextTimerTick;
    Task Tick(int elapsedTime);

    IEnumerable<string>? GetExternalScripts();
    IEnumerable<string>? GetExternalStylesheets();

    Stream? GetResourceStream(string filename);
    IEnumerable<string> GetResourceNames();
}

public interface IPlayer
{
    void ShowMenu(MenuData menuData);
    void DoWait();
    void DoPause(int ms);
    // inline: as MenuData.Inline - the engine has already drawn the Yes/No links itself.
    void ShowQuestion(string caption, bool inline);
    void SetWindowMenu(MenuData menuData);
    Task PlaySoundAsync(string filename, bool synchronous, bool looped);
    void StopSound();
    void WriteHTML(string html);
    Task<string> GetUrlAsync(string filename);
    void LocationUpdated(string location);
    void UpdateGameName(string name);
    void ClearScreen();
    Task ShowPictureAsync(string filename);
    void SetPanesVisible(string data);
    void SetStatusText(string text);
    void SetBackground(string colour);
    void SetForeground(string colour);
    void SetLinkForeground(string colour);
    Task RunScriptAsync(string function, object?[]? parameters);
    void SetFont(string fontName);
    void SetFontSize(string fontSize);
    void Speak(string text);
    void RequestSave(string? html);
    void Show(string element);
    void Hide(string element);
    void SetCompassDirections(IEnumerable<string> dirs);
    void SetInterfaceString(string name, string text);
    void SetPanelContents(string html);
    void Log(string text);
    void SetTurnPending(bool pending);
}

public enum ListType
{
    InventoryList,
    ExitsList,
    ObjectsList,
    ElementMenuVerbs
}

public class MenuData
{
    public MenuData(string caption, IDictionary<string, string> options, bool allowCancel)
    {
        Caption = caption;
        Options = options;
        AllowCancel = allowCancel;
    }

    public string Caption { get; }

    public IDictionary<string, string> Options { get; }

    public bool AllowCancel { get; }

    // True when the engine has already drawn this menu as numbered links in the transcript
    // (v600+ games - see WorldModel.ShowInlinePromptAsync). The call is then purely a
    // notification that a menu is awaiting a response, which is what a headless IPlayer such as
    // the walkthrough runner needs; players that draw their own UI should show nothing.
    public bool Inline { get; init; }
}

public class ListData
{
    public ListData(string text, IEnumerable<string> verbs)
        : this(text, verbs, null, text)
    {
    }

    public ListData(string text, IEnumerable<string> verbs, string? elementId, string elementName)
    {
        Text = text;
        Verbs = verbs;
        ElementId = elementId;
        ElementName = elementName;
    }

    public string Text { get; }

    public IEnumerable<string> Verbs { get; }

    public string? ElementId { get; }

    public string ElementName { get; }
}