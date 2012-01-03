using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    // Any changes here should also be reflected in CoreEditorScripts.aslx (validvalues for "request" command)
    // and also on the wiki http://quest5.net/wiki/Request
    internal enum Request
    {
        Quit,
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
        Show,
        Hide,
        SetCompassDirections,
        Pause,
        Wait,
        SetInterfaceString,
        RequestSave,
        SetPanelContents
    }

    public class RequestScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "request"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new RequestScript(WorldModel, parameters[0], new Expression<string>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class RequestScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private Request m_request;
        private IFunction<string> m_data;

        public RequestScript(WorldModel worldModel, string request, IFunction<string> data)
        {
            m_worldModel = worldModel;
            m_data = data;
            m_request = (Request)(Enum.Parse(typeof(Request), request));
        }

        protected override ScriptBase CloneScript()
        {
            return new RequestScript(m_worldModel, m_request.ToString(), m_data.Clone());
        }

        public override void Execute(Context c)
        {
            string data = m_data.Execute(c);

            // TO DO: Replace with dictionary mapping the enum to lambda functions
            switch (m_request)
            {
                case Request.UpdateLocation:
                    m_worldModel.PlayerUI.LocationUpdated(data);
                    break;
                case Request.GameName:
                    m_worldModel.PlayerUI.UpdateGameName(data);
                    break;
                case Request.ClearScreen:
                    m_worldModel.PlayerUI.ClearScreen();
                    break;
                case Request.ShowPicture:
                    m_worldModel.PlayerUI.ShowPicture(data);
                    break;
                case Request.PanesVisible:
                    m_worldModel.PlayerUI.SetPanesVisible(data);
                    break;
                case Request.Background:
                    m_worldModel.PlayerUI.SetBackground(data);
                    break;
                case Request.Foreground:
                    m_worldModel.PlayerUI.SetForeground(data);
                    break;
                case Request.RunScript:
                    m_worldModel.PlayerUI.RunScript(data);
                    break;
                case Request.Quit:
                    m_worldModel.PlayerUI.Quit();
                    m_worldModel.Finish();
                    break;
                case Request.FontName:
                    m_worldModel.PlayerUI.SetFont(data);
                    break;
                case Request.FontSize:
                    m_worldModel.PlayerUI.SetFontSize(data);
                    break;
                case Request.LinkForeground:
                    m_worldModel.PlayerUI.SetLinkForeground(data);
                    break;
                case Request.Show:
                    m_worldModel.PlayerUI.Show(data);
                    break;
                case Request.Hide:
                    m_worldModel.PlayerUI.Hide(data);
                    break;
                case Request.SetCompassDirections:
                    m_worldModel.PlayerUI.SetCompassDirections(data.Split(';'));
                    break;
                case Request.SetStatus:
                    m_worldModel.PlayerUI.SetStatusText(data.Replace("\n", Environment.NewLine));
                    break;
                case Request.Pause:
                    int ms;
                    if (int.TryParse(data, out ms)){
                        m_worldModel.StartPause(ms);
                    }
                    break;
                case Request.Wait:
                    m_worldModel.StartWait();
                    break;
                case Request.SetInterfaceString:
                    string[] args = data.Split('=');
                    m_worldModel.PlayerUI.SetInterfaceString(args[0], args[1]);
                    break;
                case Request.RequestSave:
                    m_worldModel.PlayerUI.RequestSave();
                    break;
                case Request.SetPanelContents:
                    m_worldModel.PlayerUI.SetPanelContents(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", "Unhandled request type");
            }
        }

        public override string Save()
        {
            return SaveScript("request", m_request.ToString(), m_data.Save());
        }

        public override string Keyword
        {
            get
            {
                return "request";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_request.ToString();
                case 1:
                    return m_data.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_request = (Request)(Enum.Parse(typeof(Request), (string)value));
                    break;
                case 1:
                    m_data = new Expression<string>((string)value, m_worldModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
