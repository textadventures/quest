namespace QuestViva.Common;

public delegate void PrintTextHandler(string text);

public delegate void UpdateListHandler(ListType listType, List<ListData> items);

public delegate void FinishedHandler();

public delegate void ErrorHandler(Exception ex);

public interface IGame
{
    List<string> Errors { get; }
    string GameID { get; }
    Task<bool> Initialise(IPlayer player);
    void Begin();
    Task SendCommand(string command);
    Task SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata);
    Task SendEvent(string eventName, string param);
    event PrintTextHandler? PrintText;
    event UpdateListHandler? UpdateList;
    event FinishedHandler? Finished;
    event ErrorHandler? LogError;
    void Finish();
    byte[] Save(string html);
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
    void ShowQuestion(string caption);
    void SetWindowMenu(MenuData menuData);
    void PlaySound(string filename, bool synchronous, bool looped);
    void StopSound();
    void WriteHTML(string html);
    string GetURL(string filename);
    void LocationUpdated(string location);
    void UpdateGameName(string name);
    void ClearScreen();
    void ShowPicture(string filename);
    void SetPanesVisible(string data);
    void SetStatusText(string text);
    void SetBackground(string colour);
    void SetForeground(string colour);
    void SetLinkForeground(string colour);
    void RunScript(string function, object[]? parameters);
    void Quit();
    void SetFont(string fontName);
    void SetFontSize(string fontSize);
    void Speak(string text);
    void RequestSave(string html);
    void Show(string element);
    void Hide(string element);
    void SetCompassDirections(IEnumerable<string> dirs);
    void SetInterfaceString(string name, string text);
    void SetPanelContents(string html);
    void Log(string text);
    string? GetUIOption(UIOption option);
}

public enum ListType
{
    InventoryList,
    ExitsList,
    ObjectsList
}

public enum UIOption
{
    UseGameColours,
    UseGameFont,
    OverrideForeground,
    OverrideLinkForeground,
    OverrideFontName,
    OverrideFontSize
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