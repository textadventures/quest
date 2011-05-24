using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class InsertScriptConstructor : ScriptConstructorBase
    {
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

        protected override ScriptBase CloneScript()
        {
            return new InsertScript(m_worldModel, m_filename.Clone());
        }

        public override void Execute(Context c)
        {
            string filename = m_filename.Execute(c);
            string path = m_worldModel.GetExternalPath(filename);
            m_worldModel.PlayerUI.WriteHTML(System.IO.File.ReadAllText(path));
        }

        public override string Save()
        {
            return SaveScript("insert", m_filename.Save());
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
                return "insert";
            }
        }
    }
}
