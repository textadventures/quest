using QuestViva.Common;

namespace QuestViva.PlayerCore;

// The IPlayer behind a HeadlessSession. Every IPlayer member is a UI hook; a headless run no-ops
// all but the text channels and the ones the session inspects (menu / question / wait / pause).
internal sealed class HeadlessPlayer(Action<string> emit) : IPlayer
{
    public MenuData? PendingMenu;
    public string? PendingQuestion;
    public bool IsWaiting;
    public bool IsPausing;

    public void ShowMenu(MenuData menuData) => PendingMenu = menuData;
    public void ShowQuestion(string caption, bool inline) => PendingQuestion = caption;
    public void DoWait() => IsWaiting = true;
    public void DoPause(int ms) => IsPausing = true;

    public void WriteHTML(string html) => emit(html);

    // v540+ games deliver their text as JS calls: JS.addText(html) arrives here as
    // RunScriptAsync("addText", [html]), or "QuestUI.addText" with the current Core library.
    // That is the real output channel.
    public Task RunScriptAsync(string function, object?[]? parameters)
    {
        if (function is "addText" or "QuestUI.addText" && parameters is { Length: > 0 })
        {
            emit(parameters[0]?.ToString() ?? "");
        }

        return Task.CompletedTask;
    }

    public void SetWindowMenu(MenuData menuData) { }

    // A synchronous `play sound` is a suspend, not just a UI hook: PlaySoundScript parks the
    // script on the same TCS `wait` uses, and only resumes on FinishWait(), which a browser player
    // calls when the audio ends. Headless there is no audio, so without this the rest of the script
    // (often the ending's msg + finish) never runs, while the game still accepts commands - a
    // silent half-dead session. Flagging IsWaiting hands the resume to the session's wait loop.
    public Task PlaySoundAsync(string filename, bool synchronous, bool looped)
    {
        if (synchronous)
        {
            IsWaiting = true;
        }

        return Task.CompletedTask;
    }

    public void StopSound() { }
    public Task<string> GetUrlAsync(string filename) => Task.FromResult(filename);
    public void LocationUpdated(string location) { }
    public void UpdateGameName(string name) { }
    public void ClearScreen() { }
    public Task ShowPictureAsync(string filename) => Task.CompletedTask;
    public void SetPanesVisible(string data) { }
    public void SetStatusText(string text) { }
    public void SetBackground(string colour) { }
    public void SetForeground(string colour) { }
    public void SetLinkForeground(string colour) { }
    public void Quit() { }
    public void SetFont(string fontName) { }
    public void SetFontSize(string fontSize) { }
    public void Speak(string text) { }
    public void RequestSave(string? html) { }
    public void Show(string element) { }
    public void Hide(string element) { }
    public void SetCompassDirections(IEnumerable<string> dirs) { }
    public void SetInterfaceString(string name, string text) { }
    public void SetPanelContents(string html) { }
    public void Log(string text) { }
    public void SetTurnPending(bool pending) { }
}
