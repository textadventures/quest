using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuestViva.Common
{
    public delegate void PrintTextHandler(string text);
    public delegate void UpdateListHandler(ListType listType, List<ListData> items);
    public delegate void FinishedHandler();
    public delegate void ErrorHandler(Exception ex);

    public interface IGame
    {
        Task<bool> Initialise(IPlayer player, bool? isCompiled = null);
        void Begin();
        void SendCommand(string command);
        void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata);
        void SendEvent(string eventName, string param);
        event PrintTextHandler? PrintText;
        event UpdateListHandler? UpdateList;
        event FinishedHandler? Finished;
        event ErrorHandler? LogError;
        List<string> Errors { get; }
        void Finish();
        byte[] Save(string html);
        void FinishWait();
        void FinishPause();
        
        void SetMenuResponse(string? response);
        void SetQuestionResponse(bool response);
        
        event Action<int>? RequestNextTimerTick;
        void Tick(int elapsedTime);
        
        IEnumerable<string>? GetExternalScripts();
        IEnumerable<string> GetExternalStylesheets();
        
        string? TempFolder { get; set; }
        System.IO.Stream? GetResource(string filename);
        string GetResourcePath(string filename);
        string GameID { get; }
        IEnumerable<string> GetResourceNames();
    }

    public interface IPlayer
    {
        void ShowMenu(MenuData menuData);
        void DoWait();
        void DoPause(int ms);
        void ShowQuestion(string caption);
        void SetWindowMenu(MenuData menuData);
        string GetNewGameFile(string originalFilename, string extensions);
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
        private string? m_elementId;
        private string m_elementName;

        public ListData(string text, IEnumerable<string> verbs)
            : this(text, verbs, null, text)
        {
        }

        public ListData(string text, IEnumerable<string> verbs, string? elementId, string elementName)
        {
            m_text = text;
            m_verbs = verbs;
            m_elementId = elementId;
            m_elementName = elementName;
        }

        public string Text
        {
            get { return m_text; }
        }

        public IEnumerable<string> Verbs
        {
            get { return m_verbs; }
        }

        public string? ElementId
        {
            get { return m_elementId; }
        }

        public string ElementName
        {
            get { return m_elementName; }
        }
    }
}
