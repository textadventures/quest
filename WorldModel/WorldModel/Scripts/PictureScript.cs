using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class PictureScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "picture"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new PictureScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
    }

    public class PictureScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_filename;

        public PictureScript(WorldModel worldModel, IFunction<string> function)
        {
            m_worldModel = worldModel;
            m_filename = function;
        }

        public override void Execute(Context c)
        {
            string filename = m_filename.Execute(c);
            string path = m_worldModel.GetExternalPath(filename);
            m_worldModel.PlayerUI.ShowPicture(path);
        }

        public override string Save()
        {
            return SaveScript("picture", m_filename.Save());
        }

        public override object GetParameter(int index)
        {
            return m_filename.Save();
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_filename = new Expression<string>((string)value, m_worldModel);
        }

        public override string Keyword
        {
            get
            {
                return "picture";
            }
        }
    }
}
