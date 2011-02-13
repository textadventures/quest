using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class InsertScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "insert"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new InsertScript(WorldModel, new Expression<string>(parameters[0], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 1 }; }
        }
        #endregion
    }

    public class InsertScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_filename;

        public InsertScript(WorldModel worldModel, IFunction<string> filename)
        {
            m_worldModel = worldModel;
            m_filename = filename;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            string filename = m_filename.Execute(c);
            string path = m_worldModel.GetExternalPath(filename);
            m_worldModel.PlayerUI.WriteHTML(System.IO.File.ReadAllText(path));
        }

        #endregion

        public override string Save()
        {
            return SaveScript("msg", m_filename.Save());
        }

        public override string GetParameter(int index)
        {
            return m_filename.Save();
        }

        public override void SetParameterInternal(int index, string value)
        {
            m_filename = new Expression<string>(value, m_worldModel);
        }

        public override string Keyword
        {
            get
            {
                return "insert";
            }
        }
    }
}
