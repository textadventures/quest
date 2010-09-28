using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;

namespace AxeSoftware.Quest.LegacyASL
{
    public class LegacyASL : IASL
    {
        //private ASL.QuestGame m_game;
        //private string m_filename;

        public LegacyASL(string filename)
        {
            //m_game = new ASL.QuestGame();
            //m_game.Output += new ASL.__QuestGame_OutputEventHandler(m_game_Output);
            //m_filename = filename;
        }

        void m_game_Output(ref string Text)
        {
            if (PrintText != null)
            {
                // need to replace formatting codes
                string output=Text.Replace("|", "");
                PrintText("<output>" + output + "</output>");
            }
        }

        #region IASL Members

        public bool Initialise()
        {
            //m_game.Initialise(ref m_filename);
            return true;
        }

        public string GetInterface()
        {
            return null;
        }

        public void Begin()
        {
            PrintText("<output>Games written for earlier versions of Quest are not currently supported.</output>");
            if (Finished != null) Finished();
        }

        public void SendCommand(string command)
        {
            //m_game.RunCommand(ref command);
        }

        public void SendEvent(string eventName, string param)
        {
            throw new NotImplementedException();
        }

#pragma warning disable 67
        public event PrintTextHandler PrintText;

        public event RequestHandler RequestRaised;

        public event UpdateListHandler UpdateList;

        public event MenuHandler ShowMenu;

        public event FinishedHandler Finished;
#pragma warning restore 67

        public List<string> Errors
        {
            get { throw new NotImplementedException(); }
        }

        public string Filename
        {
            get { return null; /* m_filename */ }
        }

        public string SaveFilename
        {
            get { return string.Empty; }
        }

        public void Finish()
        {
        }

        public IWalkthrough Walkthrough
        {
            get { throw new NotImplementedException(); }
        }

        public string Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
