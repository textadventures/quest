using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class CommentScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "//"; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            return new CommentScript(script.Substring(2).Trim());
        }

        public IScriptFactory ScriptFactory { set { } }

        public WorldModel WorldModel { get; set; }
    }

    public class CommentScript : ScriptBase
    {
        private string m_comment;

        public CommentScript(string comment)
        {
            m_comment = comment;
        }

        protected override ScriptBase CloneScript()
        {
            return new CommentScript(m_comment);
        }

        public override void Execute(Context c)
        {
        }

        public override string Save()
        {
            return "// " + string.Join(Environment.NewLine + "// ", m_comment.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
        }

        public override object GetParameter(int index)
        {
            return m_comment;
        }

        public override void SetParameterInternal(int index, object value)
        {
            m_comment = (string)value;
        }

        public override string Keyword
        {
            get
            {
                return "//";
            }
        }

        public void AddLine(string line)
        {
            if (!line.StartsWith("//"))
            {
                throw new ArgumentException("Expected comment line: " + line);
            }
            m_comment += Environment.NewLine + line.Substring(2).Trim();
        }
    }
}
