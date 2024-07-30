using System.Diagnostics;
using TextAdventures.Quest;

namespace WebPlayer;

public class PlayerHelperUI : IPlayerHelperUI
{
    public void ShowMenu(MenuData menuData)
    {
    }

    public void DoWait()
    {
    }

    public void DoPause(int ms)
    {
    }

    public void ShowQuestion(string caption)
    {
    }

    public void SetWindowMenu(MenuData menuData)
    {
    }

    public string GetNewGameFile(string originalFilename, string extensions)
    {
        throw new NotImplementedException();
    }

    public void PlaySound(string filename, bool synchronous, bool looped)
    {
    }

    public void StopSound()
    {
    }

    public void WriteHTML(string html)
    {
    }

    public string GetURL(string file)
    {
        throw new NotImplementedException();
    }

    public void LocationUpdated(string location)
    {
    }

    public void UpdateGameName(string name)
    {
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
    }

    public void SetBackground(string colour)
    {
    }

    public void SetForeground(string colour)
    {
    }

    public void SetLinkForeground(string colour)
    {
    }

    public void RunScript(string function, object[] parameters)
    {
    }

    public void Quit()
    {
    }

    public void SetFont(string fontName)
    {
    }

    public void SetFontSize(string fontSize)
    {
    }

    public void Speak(string text)
    {
    }

    public void RequestSave(string html)
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

    public void SetInterfaceString(string name, string text)
    {
    }

    public void SetPanelContents(string html)
    {
    }

    public void Log(string text)
    {
        Debug.WriteLine(text);
    }

    public string GetUIOption(UIOption option)
    {
        throw new NotImplementedException();
    }

    public void OutputText(string text)
    {
    }

    public void SetAlignment(string alignment)
    {
    }

    public void BindMenu(string linkid, string verbs, string text, string elementId)
    {
    }
}