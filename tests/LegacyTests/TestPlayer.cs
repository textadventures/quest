using QuestViva.Common;

namespace QuestViva.LegacyTests;

internal class TestPlayer : IPlayer
{
    private readonly List<string> m_output = new();

    public int BufferLength => m_output.Count;

    public MenuData LatestMenu { get; set; }

    public bool IsWaiting { get; set; }

    public string QuestionData { get; set; }

    public string Location { get; private set; } = string.Empty;

    public string GameName { get; private set; } = string.Empty;

    public string StatusText { get; private set; } = string.Empty;

    public string Background { get; private set; } = string.Empty;

    public string Foreground { get; private set; } = string.Empty;

    public string FontName { get; private set; } = string.Empty;

    public string FontSize { get; private set; } = string.Empty;

    public void ShowMenu(MenuData menuData)
    {
        LatestMenu = menuData;
    }

    public void DoWait()
    {
        IsWaiting = true;
    }

    public void ShowQuestion(string caption)
    {
        QuestionData = caption;
    }

    public void SetWindowMenu(MenuData menuData)
    {
    }

    public void PlaySound(string filename, bool synchronous, bool looped)
    {
    }

    public void StopSound()
    {
    }

    public void WriteHTML(string html)
    {
        throw new NotImplementedException();
    }

    public void LocationUpdated(string location)
    {
        Location = location;
    }

    public void UpdateGameName(string name)
    {
        GameName = name;
    }

    public void ClearScreen()
    {
    }

    public void ShowPicture(string filename)
    {
    }

    public void SetPanesVisible(string data)
    {
    }

    public void SetStatusText(string text)
    {
        StatusText = text;
    }

    public void SetBackground(string colour)
    {
        Background = colour;
    }

    public void SetForeground(string colour)
    {
        Foreground = colour;
    }

    public void RunScript(string function, object[] parameters)
    {
    }

    public void Quit()
    {
    }

    public void SetFont(string fontName)
    {
        FontName = fontName;
    }

    public void SetFontSize(string fontSize)
    {
        FontSize = fontSize;
    }

    public void Speak(string text)
    {
    }

    public void RequestSave(string html)
    {
    }

    public void SetLinkForeground(string colour)
    {
    }

    public void Show(string element)
    {
    }

    public void Hide(string element)
    {
    }

    public void SetCompassDirections(IEnumerable<string> dirs)
    {
    }

    public string GetURL(string file)
    {
        return file;
    }

    public void DoPause(int ms)
    {
    }

    public void SetInterfaceString(string name, string text)
    {
    }

    public void SetPanelContents(string html)
    {
    }

    public void Log(string text)
    {
    }

    public string GetUIOption(UIOption option)
    {
        return null;
    }

    public void ClearBuffer()
    {
        m_output.Clear();
    }

    public string Buffer(int index)
    {
        return m_output[index];
    }

    public void PrintText(string text)
    {
        if (!text.StartsWith("<output>") || !text.EndsWith("</output>"))
        {
            throw new ArgumentException("Invalid output format");
        }

        // remove <output> and </output>
        text = text.Substring(8, text.Length - 17);
        m_output.Add(text);
    }
}