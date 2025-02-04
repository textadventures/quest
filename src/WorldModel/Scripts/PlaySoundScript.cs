using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class PlaySoundScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "play sound"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new PlaySoundScript(scriptContext,
                new Expression<string>(parameters[0], scriptContext),
                new Expression<bool>(parameters[1], scriptContext),
                new Expression<bool>(parameters[2], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class PlaySoundScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_filename;
        private IFunction<bool> m_synchronous;
        private IFunction<bool> m_loop;

        public PlaySoundScript(ScriptContext scriptContext, IFunction<string> function, IFunction<bool> synchronous, IFunction<bool> loop)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_filename = function;
            m_synchronous = synchronous;
            m_loop = loop;
        }

        protected override ScriptBase CloneScript()
        {
            return new PlaySoundScript(m_scriptContext, m_filename.Clone(), m_synchronous.Clone(), m_loop.Clone());
        }

        public override void Execute(Context c)
        {
            string filename = m_filename.Execute(c);
            m_worldModel.PlaySound(filename, m_synchronous.Execute(c), m_loop.Execute(c));
        }

        public override string Save()
        {
            return SaveScript("play sound", m_filename.Save(), m_synchronous.Save(), m_loop.Save());
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_filename.Save();
                case 1:
                    return m_synchronous.Save();
                case 2:
                    return m_loop.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_filename = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_synchronous = new Expression<bool>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_loop = new Expression<bool>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string Keyword
        {
            get
            {
                return "play sound";
            }
        }
    }

    public class StopSoundScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "stop sound"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new StopSoundScript(WorldModel);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 0 }; }
        }
    }

    public class StopSoundScript : ScriptBase
    {
        private WorldModel m_worldModel;

        public StopSoundScript(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new StopSoundScript(m_worldModel);
        }

        public override void Execute(Context c)
        {
            m_worldModel.PlayerUI.StopSound();
        }

        public override string Save()
        {
            return "stop sound";
        }

        public override object GetParameter(int index)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override void SetParameterInternal(int index, object value)
        {
            throw new ArgumentOutOfRangeException();
        }

        public override string Keyword
        {
            get
            {
                return "stop sound";
            }
        }
    }
}
