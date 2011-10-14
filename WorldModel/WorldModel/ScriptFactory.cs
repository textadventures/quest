using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Text.RegularExpressions;

namespace AxeSoftware.Quest
{
    public interface IScriptFactory
    {
        IScript CreateScript(string line, Element proc);
        IScript CreateScript(string line);
    }

    public class ScriptFactory : IScriptFactory
    {
        private Dictionary<string, IScriptConstructor> m_scriptConstructors = new Dictionary<string, IScriptConstructor>();

        public delegate void AddErrorHandler(string error);
        public event AddErrorHandler ErrorHandler;
        private WorldModel m_worldModel;
        private SetScriptConstructor m_setConstructor;
        private FunctionCallScriptConstructor m_procConstructor;

        public ScriptFactory(WorldModel worldModel)
        {
            m_worldModel = worldModel;

            // Use Reflection to create instances of all IScriptConstructors
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IScriptConstructor)))
            {
                AddConstructor((IScriptConstructor)Activator.CreateInstance(t));
            }

            m_setConstructor = (SetScriptConstructor)InitScriptConstructor(new SetScriptConstructor());
            m_procConstructor = (FunctionCallScriptConstructor)InitScriptConstructor(new FunctionCallScriptConstructor());
        }

        private void AddConstructor(IScriptConstructor constructor)
        {
            if (constructor.Keyword != null)
            {
                m_scriptConstructors.Add(constructor.Keyword, InitScriptConstructor(constructor));
            }
        }

        private IScriptConstructor InitScriptConstructor(IScriptConstructor constructor)
        {
            constructor.WorldModel = m_worldModel;
            constructor.ScriptFactory = this;
            return constructor;
        }

        /// <summary>
        /// For single-line scripts only - used by the editor to create new script lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public IScript CreateSimpleScript(string line)
        {
            MultiScript result = (MultiScript)CreateScript(line);
            return result.Scripts.First();
        }

        /// <summary>
        /// For creating a new function call script, used by the editor
        /// </summary>
        /// <returns></returns>
        public IScript CreateBlankFunctionCallScript()
        {
            return new FunctionCallScript(m_worldModel, "");
        }

        public IScript CreateScript(string line)
        {
            return CreateScript(line, null);
        }

        public IScript CreateScript(string line, Element proc)
        {
            string remainingScript;
            IScript newScript;
            MultiScript result = new MultiScript();
            bool finished = false;
            IScript lastIf = null;
            bool dontAdd;
            bool addedError;
            IfScriptConstructor ifConstructor = null;

            line = Utility.RemoveSurroundingBraces(line);

            while (!finished)
            {
                try
                {
                    line = Utility.GetScript(line, out remainingScript);
                }
                catch (Exception ex)
                {
                    AddError(string.Format("Error adding script '{0}': {1}", line, ex.Message));
                    break;
                }

                if (line != null)
                {
                    line = line.Trim();
                    line = Utility.RemoveComments(line);
                }

                if (!string.IsNullOrEmpty(line))
                {
                    newScript = null;
                    dontAdd = false;
                    addedError = false;

                    if (line.StartsWith("else"))
                    {
                        if (lastIf == null)
                        {
                            AddError("Unexpected 'else' (error with parent 'if'?):" + line);
                        }
                        else
                        {
                            if (line.StartsWith("else if"))
                            {
                                ifConstructor.AddElseIf(lastIf, line, proc);
                            }
                            else
                            {
                                ifConstructor.AddElse(lastIf, line, proc);
                            }
                        }
                        dontAdd = true;
                    }
                    else
                    {
                        lastIf = null;
                        IScriptConstructor constructor = GetScriptConstructor(line);

                        if (constructor != null)
                        {
                            try
                            {
                                newScript = constructor.Create(line, proc);
                                if (constructor.Keyword == "if")
                                {
                                    ifConstructor = (IfScriptConstructor)constructor;
                                    lastIf = newScript;
                                }
                            }
                            catch (Ciloci.Flee.ExpressionCompileException ex)
                            {
                                AddError(string.Format("Error compiling expression in '{0}': {1}", line, ex.Message));
                                addedError = true;
                            }
                            catch (Exception ex)
                            {
                                AddError(string.Format("Error adding script '{0}': {1}", line, ex.Message));
                                addedError = true;
                            }
                        }

                        if (!addedError)
                        {
                            if (newScript == null)
                            {
                                // See if the script is like "myvar = 2". newScript will be null otherwise.
                                newScript = m_setConstructor.Create(line, proc);
                            }

                            if (newScript == null)
                            {
                                // See if the script calls a procedure defined by the game
                                newScript = m_procConstructor.Create(line, proc);
                            }
                        }
                    }

                    if (!dontAdd && !addedError)
                    {
                        if (newScript == null)
                        {
                            if (!addedError) AddError(string.Format("Unrecognised script command '{0}'", line));
                        }
                        else
                        {
                            newScript.Line = line;
                            result.Add(newScript);
                        }
                    }
                }

                line = remainingScript;
                if (string.IsNullOrEmpty(line)) finished = true;
            }

            return result;
        }

        private static Regex s_nonWordCharacterRegex = new Regex(@"^\W");

        private IScriptConstructor GetScriptConstructor(string line)
        {
            IScriptConstructor constructor = null;
            int strength = 0;
            foreach (IScriptConstructor c in m_scriptConstructors.Values)
            {
                if (line.StartsWith(c.Keyword))
                {
                    // The line must start with the script keyword, and then the following
                    // character must be a non-word character. For example "msgfunction" is not
                    // a match for "msg".

                    if (line.Length == c.Keyword.Length || s_nonWordCharacterRegex.IsMatch(line.Substring(c.Keyword.Length)))
                    {
                        if (c.Keyword.Length > strength)
                        {
                            constructor = c;
                            strength = c.Keyword.Length;
                        }
                    }
                }
            }
            return constructor;
        }

        private void AddError(string error)
        {
            if (ErrorHandler != null) ErrorHandler(error);
        }
    }
}
