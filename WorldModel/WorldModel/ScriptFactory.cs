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
        private CallProcedureScriptConstructor m_procConstructor;

        // TO DO: This will also cut off any strings that happen to have two consecutive slashes in them.
        // Easiest solution I think would be run this regex on all lines, if there are lines that match
        // then just iterate through the length of the string one character at a time, set flag when within
        // a quoted string, then if find two consecutive slashes outside a string, trim the line.
        private Regex m_removeComments = new Regex("//.*");

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
            m_procConstructor = (CallProcedureScriptConstructor)InitScriptConstructor(new CallProcedureScriptConstructor());
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
            IScriptConstructor constructor = GetScriptConstructor(line);
            return constructor.Create(line, null);
        }

        public IScript CreateScript(string line)
        {
            return CreateScript(line, null);
        }

        public IScript CreateScript(string line, Element proc)
        {
            string remainingScript;
            IScript newScript;
            MultiScript result = null;
            bool finished = false;
            IScript lastIf = null;
            bool dontAdd;
            bool addedError;
            IfScriptConstructor ifConstructor = null;

            line = Utility.RemoveSurroundingBraces(line);

            while (!finished)
            {
                line = Utility.GetScript(line, out remainingScript);
                if (line != null)
                {
                    line = line.Trim();
                    line = RemoveComments(line);
                }

                if (!string.IsNullOrEmpty(line))
                {
                    newScript = null;
                    dontAdd = false;
                    addedError = false;

                    if (lastIf != null && line.StartsWith("else"))
                    {
                        if (line.StartsWith("else if"))
                        {
                            ifConstructor.AddElseIf(lastIf, line, proc);
                        }
                        else
                        {
                            ifConstructor.AddElse(lastIf, line, proc);
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

                    if (!dontAdd)
                    {
                        if (newScript == null)
                        {
                            if (!addedError) AddError(string.Format("Unrecognised script command '{0}'", line));
                        }
                        else
                        {
                            newScript.Line = line;

                            if (result == null)
                            {
                                result = new MultiScript(newScript);
                            }
                            else
                            {
                                result.Add(newScript);
                            }
                        }
                    }
                }

                line = remainingScript;
                if (string.IsNullOrEmpty(line)) finished = true;
            }

            return result;
        }

        private IScriptConstructor GetScriptConstructor(string line)
        {
            IScriptConstructor constructor = null;
            int strength = 0;
            foreach (IScriptConstructor c in m_scriptConstructors.Values)
            {
                if (line.StartsWith(c.Keyword))
                {
                    if (c.Keyword.Length > strength)
                    {
                        constructor = c;
                        strength = c.Keyword.Length;
                    }
                }
            }
            return constructor;
        }

        private void AddError(string error)
        {
            if (ErrorHandler != null) ErrorHandler(error);
        }

        private string RemoveComments(string input)
        {
            return m_removeComments.Replace(input, "");
        }
    }
}
