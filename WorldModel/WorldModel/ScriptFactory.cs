using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;
using System.Text.RegularExpressions;

namespace TextAdventures.Quest
{
    public interface IScriptFactory
    {
        IScript CreateScript(string line, ScriptContext scriptContext, bool lazy, bool addExceptionsToLog = true);
        IScript CreateScript(string line, ScriptContext scriptContext);
        IScript CreateScript(string line);
    }

    public class ScriptFactory : IScriptFactory
    {
        private bool m_lazyLoadingEnabled = true;

        public class AddErrorEventArgs : EventArgs
        {
            public string Error { get; set; }
        }

        private Dictionary<string, IScriptConstructor> m_scriptConstructors = new Dictionary<string, IScriptConstructor>();

        public event EventHandler<AddErrorEventArgs> ErrorHandler;
        private WorldModel m_worldModel;
        private SetScriptConstructor m_setConstructor;
        private FunctionCallScriptConstructor m_procConstructor;

        public ScriptFactory(WorldModel worldModel)
        {
            m_worldModel = worldModel;

            // Use Reflection to create instances of all IScriptConstructors
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
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
            IMultiScript result = (IMultiScript)CreateScript(line);
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
            return CreateScript(line, new ScriptContext(m_worldModel));
        }

        public IScript CreateScript(string line, ScriptContext scriptContext)
        {
            return CreateScript(line, scriptContext, m_lazyLoadingEnabled);
        }

        public IScript CreateScript(string line, ScriptContext scriptContext, bool lazy, bool addExceptionsToLog = true)
        {
            MultiScript result = new MultiScript(m_worldModel);
            bool finished = false;
            IScript lastIf = null;
            IScript lastComment = null;
            IScript lastFirstTime = null;
            IfScriptConstructor ifConstructor = null;
            
            if (lazy)
            {
                return new LazyLoadScript(this, line, scriptContext);
            }

            line = Utility.RemoveSurroundingBraces(line);
            line = Utility.RemoveComments(line, m_worldModel.EditMode);

            while (!finished)
            {
                string remainingScript;
                try
                {
                    line = Utility.GetScript(line, out remainingScript);
                }
                catch (Exception ex)
                {
                    if (addExceptionsToLog)
                    {
                        AddError(string.Format("Error adding script '{0}': {1}", line, ex.Message));
                        break;
                    }
                    throw;
                }

                if (line != null)
                {
                    line = line.Trim();
                }

                if (!string.IsNullOrEmpty(line))
                {
                    IScript newScript = null;
                    bool dontAdd = false;
                    bool addedError = false;

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
                                ifConstructor.AddElseIf(lastIf, line, scriptContext);
                            }
                            else
                            {
                                ifConstructor.AddElse(lastIf, line, scriptContext);
                            }
                        }
                        dontAdd = true;
                    }
                    else if (line.StartsWith("otherwise"))
                    {
                        if (lastFirstTime == null)
                        {
                            AddError("Unexpected 'otherwise' (error with parent 'firsttime'?):" + line);
                        }
                        else
                        {
                            FirstTimeScriptConstructor.AddOtherwiseScript(lastFirstTime, line, this);
                        }
                        dontAdd = true;
                    }
                    else
                    {
                        IScriptConstructor constructor = GetScriptConstructor(line);

                        if (constructor is CommentScriptConstructor)
                        {
                            if (lastComment != null)
                            {
                                ((CommentScript)lastComment).AddLine(line);
                                dontAdd = true;
                            }
                            else
                            {
                                newScript = constructor.Create(line, scriptContext);
                                lastComment = newScript;
                            }
                        }
                        else
                        {
                            lastIf = null;
                            lastComment = null;
                            lastFirstTime = null;

                            if (constructor != null)
                            {
                                try
                                {
                                    if (m_lazyLoadingEnabled)
                                    {
                                        newScript = new LazyLoadScript(this, constructor, line, scriptContext);
                                    }
                                    else
                                    {
                                        newScript = constructor.Create(line, scriptContext);
                                    }

                                    if (constructor.Keyword == "if")
                                    {
                                        ifConstructor = (IfScriptConstructor)constructor;
                                        lastIf = newScript;
                                    }
                                    if (constructor.Keyword == "firsttime")
                                    {
                                        lastFirstTime = newScript;
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
                                    newScript = m_setConstructor.Create(line, scriptContext);
                                }

                                if (newScript == null)
                                {
                                    // See if the script calls a procedure defined by the game
                                    newScript = m_procConstructor.Create(line, scriptContext);
                                }
                            }
                        }
                    }

                    if (!dontAdd && !addedError)
                    {
                        if (newScript == null)
                        {
                            AddError(string.Format("Unrecognised script command '{0}'", line));
                        }
                        else
                        {
                            if (!m_lazyLoadingEnabled)
                            {
                                newScript.Line = line;
                            }
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

                    if (line.Length == c.Keyword.Length || s_nonWordCharacterRegex.IsMatch(line.Substring(c.Keyword.Length)) || c is CommentScriptConstructor || c is JSScriptConstructor)
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
            if (ErrorHandler != null) ErrorHandler(this, new AddErrorEventArgs { Error = error });
        }

        internal WorldModel WorldModel
        {
            get { return m_worldModel; }
        }
    }
}
