using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public delegate void PrintTextHandler(string text);
    public delegate void RequestHandler(Request request, string data);
    public delegate void UpdateListHandler(ListType listType, List<ListData> items);
    public delegate void FinishedHandler();
    public delegate void ObjectsUpdatedHandler();
    public delegate void ErrorHandler(string errorMessage);

    public interface IASL
    {
        bool Initialise(IPlayer player);
        void Begin();
        void SendCommand(string command);
        void SendEvent(string eventName, string param);
        event PrintTextHandler PrintText;
        event RequestHandler RequestRaised;
        event UpdateListHandler UpdateList;
        event FinishedHandler Finished;
        event ErrorHandler LogError;
        List<string> Errors { get; }
        string Filename { get; }
        string SaveFilename { get; }
        void Finish();
        IWalkthrough Walkthrough { get; }
        void Save(string filename);
        string SaveExtension { get; }
        void FinishWait();
        void SetMenuResponse(string response);
        void SetQuestionResponse(bool response);
        IEnumerable<string> GetExternalScripts();
    }

    public interface IPlayer
    {
        void ShowMenu(MenuData menuData);
        void DoWait();
        void ShowQuestion(string caption);
        void SetWindowMenu(MenuData menuData);
        string GetNewGameFile(string originalFilename, string extensions);
        void PlaySound(string filename, bool synchronous, bool looped);
        void StopSound();
        void WriteHTML(string html);

        void LocationUpdated(string location);
        void UpdateGameName(string name);
        void ClearScreen();
        void ShowPicture(string filename);
        void SetPanesVisible(string data);
        void SetStatusText(string text);
        void SetBackground(string colour);
        void SetForeground(string colour);
        void RunScript(string script);
        void Quit();
        void SetFont(string fontName);
        void SetFontSize(string fontSize);
        void Speak(string text);
        void RequestSave();
        void RequestLoad();
        void RequestRestart();
    }

    public enum HyperlinkType
    {
        ObjectLink,
        ExitLink
    }

    public enum Request
    {
        Quit,
        Load,
        Save,
        UpdateLocation,
        GameName,
        FontName,
        FontSize,
        Background,
        Foreground,
        LinkForeground,
        RunScript,
        SetStatus,
        ClearScreen,
        PanesVisible,
        ShowPicture,
        Speak,
        Restart
    }

    public enum ListType
    {
        InventoryList,
        ExitsList,
        ObjectsList
    }

    public class MenuData
    {
        private string m_caption;
        private IDictionary<string, string> m_options;
        private bool m_allowCancel;

        public MenuData(string caption, IDictionary<string, string> options, bool allowCancel)
        {
            m_caption = caption;
            m_options = options;
            m_allowCancel = allowCancel;
        }

        public string Caption
        {
            get { return m_caption; }
        }

        public IDictionary<string, string> Options
        {
            get { return m_options; }
        }

        public bool AllowCancel
        {
            get { return m_allowCancel; }
        }
    }

    public class ListData
    {
        private string m_text;
        private IEnumerable<string> m_verbs;

        public ListData(string text, IEnumerable<string> verbs)
        {
            m_text = text;
            m_verbs = verbs;
        }

        public string Text
        {
            get { return m_text; }
        }

        public IEnumerable<string> Verbs
        {
            get { return m_verbs; }
        }
    }

    public interface IWalkthrough
    {
        List<string> Steps { get; }
    }
}
