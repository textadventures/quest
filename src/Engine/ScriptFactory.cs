#nullable disable
using System.Reflection;
using System.Text.RegularExpressions;
using Ciloci.Flee;
using QuestViva.Engine.Scripts;
using QuestViva.Utility;

namespace QuestViva.Engine;

public interface IScriptFactory
{
    IScript CreateScript(string line, ScriptContext scriptContext, bool lazy, bool addExceptionsToLog = true);
    IScript CreateScript(string line, ScriptContext scriptContext);
    IScript CreateScript(string line);
}

public partial class ScriptFactory : IScriptFactory
{
    private readonly bool m_lazyLoadingEnabled = true;
    private readonly FunctionCallScriptConstructor m_procConstructor;

    private readonly Dictionary<string, IScriptConstructor> m_scriptConstructors = new();
    private readonly SetScriptConstructor m_setConstructor;

    public ScriptFactory(WorldModel worldModel)
    {
        WorldModel = worldModel;

        // Use Reflection to create instances of all IScriptConstructors
        foreach (var t in Classes.GetImplementations(Assembly.GetExecutingAssembly(),
                     typeof(IScriptConstructor)))
        {
            AddConstructor((IScriptConstructor) Activator.CreateInstance(t));
        }

        m_setConstructor = (SetScriptConstructor) InitScriptConstructor(new SetScriptConstructor());
        m_procConstructor = (FunctionCallScriptConstructor) InitScriptConstructor(new FunctionCallScriptConstructor());
    }

    internal WorldModel WorldModel { get; }

    public IScript CreateScript(string line)
    {
        return CreateScript(line, new ScriptContext(WorldModel));
    }

    public IScript CreateScript(string line, ScriptContext scriptContext)
    {
        return CreateScript(line, scriptContext, m_lazyLoadingEnabled);
    }

    public IScript CreateScript(string line, ScriptContext scriptContext, bool lazy, bool addExceptionsToLog = true)
    {
        var result = new MultiScript(WorldModel);
        var finished = false;
        IScript lastIf = null;
        IScript lastComment = null;
        IScript lastFirstTime = null;
        IfScriptConstructor ifConstructor = null;

        if (lazy)
        {
            return new LazyLoadScript(this, line, scriptContext);
        }

        line = Utility.RemoveSurroundingBraces(line);
        line = Utility.RemoveComments(line, WorldModel.EditMode);

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
                var dontAdd = false;
                var addedError = false;

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
                    var constructor = GetScriptConstructor(line);

                    if (constructor is CommentScriptConstructor)
                    {
                        if (lastComment != null)
                        {
                            ((CommentScript) lastComment).AddLine(line);
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
                                    ifConstructor = (IfScriptConstructor) constructor;
                                    lastIf = newScript;
                                }

                                if (constructor.Keyword == "firsttime")
                                {
                                    lastFirstTime = newScript;
                                }
                            }
                            catch (ExpressionCompileException ex)
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
            if (string.IsNullOrEmpty(line))
            {
                finished = true;
            }
        }

        return result;
    }

    public event EventHandler<AddErrorEventArgs> ErrorHandler;

    private void AddConstructor(IScriptConstructor constructor)
    {
        if (constructor.Keyword != null)
        {
            m_scriptConstructors.Add(constructor.Keyword, InitScriptConstructor(constructor));
        }
    }

    private IScriptConstructor InitScriptConstructor(IScriptConstructor constructor)
    {
        constructor.WorldModel = WorldModel;
        constructor.ScriptFactory = this;
        return constructor;
    }

    /// <summary>
    ///     For single-line scripts only - used by the editor to create new script lines
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public IScript CreateSimpleScript(string line)
    {
        var result = (IMultiScript) CreateScript(line);
        return result.Scripts.First();
    }

    /// <summary>
    ///     For creating a new function call script, used by the editor
    /// </summary>
    /// <returns></returns>
    public IScript CreateBlankFunctionCallScript()
    {
        return new FunctionCallScript(WorldModel, "");
    }

    [GeneratedRegex(@"^\W")]
    private static partial Regex s_nonWordCharacterRegex();

    private IScriptConstructor GetScriptConstructor(string line)
    {
        IScriptConstructor constructor = null;
        var strength = 0;
        foreach (var c in m_scriptConstructors.Values)
        {
            if (line.StartsWith(c.Keyword))
            {
                // The line must start with the script keyword, and then the following
                // character must be a non-word character. For example "msgfunction" is not
                // a match for "msg".

                if (line.Length == c.Keyword.Length ||
                    s_nonWordCharacterRegex().IsMatch(line.Substring(c.Keyword.Length)) ||
                    c is CommentScriptConstructor || c is JSScriptConstructor)
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
        if (ErrorHandler != null)
        {
            ErrorHandler(this, new AddErrorEventArgs {Error = error});
        }
    }

    public class AddErrorEventArgs : EventArgs
    {
        public string Error { get; set; }
    }
}