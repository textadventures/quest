using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableScripts : IEditableScripts, IDataWrapper
{
    private static readonly EditableDataWrapper<IScript, EditableScripts> Wrapper;
    private readonly EditorController _controller;

    private readonly List<IEditableScript> _scripts;
    private bool _replacingScripts;
    // Set by InitialiseScript, which every constructor path calls
    private IMultiScript _underlyingScript = null!;

    static EditableScripts()
    {
        Wrapper = new EditableDataWrapper<IScript, EditableScripts>(GetNewInstance);
    }

    private EditableScripts(EditorController controller)
    {
        _controller = controller;
        _scripts = new List<IEditableScript>();
    }

    private EditableScripts(EditorController controller, IScript script)
        : this(controller)
    {
        InitialiseScript(script);
    }

    public event EventHandler<EditableScriptsUpdatedEventArgs>? Updated;
    public event EventHandler<DataWrapperUpdatedEventArgs>? UnderlyingValueUpdated;

    #region IDataWrapper Members

    public object GetUnderlyingValue()
    {
        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);
        return _underlyingScript;
    }

    #endregion

    public string DisplayString()
    {
        return DisplayString(-1, string.Empty);
    }

    public string DisplayString(int index, string newValue)
    {
        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);

        var count = 0;
        var result = new StringBuilder();
        foreach (var script in _scripts)
        {
            if (result.Length > 0)
            {
                result.Append(Environment.NewLine);
            }

            if (index == count)
            {
                result.Append(newValue);
            }
            else
            {
                result.Append(script.DisplayString());
            }

            count++;
        }

        return result.ToString();
    }

    public void Swap(int index1, int index2)
    {
        _underlyingScript.Swap(index1, index2);
    }

    public void Cut(int[] indexes)
    {
        Copy(indexes);
        Remove(indexes);
    }

    public void Copy(int[] indexes)
    {
        var scripts = new List<IScript>();
        foreach (var index in indexes)
        {
            scripts.Add(_underlyingScript.Scripts.ElementAt(index));
        }

        _controller.SetClipboardScript(scripts);
    }

    public void Paste(int index, bool useTransaction)
    {
        if (useTransaction)
        {
            _controller.StartTransaction("Paste script");
        }

        foreach (var script in _controller.GetClipboardScript())
        {
            _underlyingScript.Insert(index, _controller.ScriptFactory.Clone(script));
            index++;
        }

        if (useTransaction)
        {
            _controller.EndTransaction();
        }
    }

    public IEditableScripts Clone(string parent, string attribute)
    {
        var clonedScript = (IScript) _underlyingScript.Clone();
        var parentElement = _controller.WorldModel.Elements.Get(parent);
        parentElement.Fields.Set(attribute, clonedScript);
        clonedScript = (IScript) parentElement.Fields.Get(attribute)!;
        var result = new EditableScripts(_controller, clonedScript);

        return result;
    }

    public string? Owner
    {
        get
        {
            if (_underlyingScript == null)
            {
                return null;
            }

            if (_underlyingScript.Owner == null)
            {
                return null;
            }

            return _underlyingScript.Owner.Name;
        }
    }

    public string Code
    {
        get
        {
            if (_underlyingScript == null)
            {
                return string.Empty;
            }

            var result = Engine.Utility.IndentScript(_underlyingScript.Save(), 0, "  ");
            if (result.StartsWith(Environment.NewLine))
            {
                result = result.Substring(Environment.NewLine.Length);
            }

            if (result.EndsWith(Environment.NewLine))
            {
                result = result.Substring(0, result.Length - Environment.NewLine.Length);
            }

            return result;
        }
        set
        {
            _controller.StartTransaction("Editing script in code view");
            _underlyingScript.LoadCode(value);
            ClearScripts();
            InitialiseScript(_underlyingScript);
            _controller.EndTransaction();
        }
    }

    public static EditableScripts GetInstance(EditorController controller, IScript script)
    {
        return Wrapper.GetInstance(controller, script);
    }

    private static EditableScripts GetNewInstance(EditorController controller, IScript script)
    {
        return new EditableScripts(controller, script);
    }

    public static void Clear()
    {
        Wrapper.Clear();
    }

    private void InitialiseScript(IScript script)
    {
        InitialiseMultiScript((IMultiScript) script);

        foreach (var scriptItem in _underlyingScript.Scripts)
        {
            _scripts.Add(_controller.ScriptFactory.CreateEditableScript(scriptItem));
        }

        foreach (var editableScript in _scripts)
        {
            editableScript.Updated += script_Updated;
        }

        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);
    }

    [MemberNotNull(nameof(_underlyingScript))]
    private void InitialiseMultiScript(IMultiScript script)
    {
        if (_underlyingScript != null)
        {
            _underlyingScript.ScriptUpdated -= multiScript_ScriptUpdated;
        }

        _underlyingScript = script;
        _underlyingScript.ScriptUpdated += multiScript_ScriptUpdated;
        _underlyingScript.UndoLog = _controller.WorldModel.UndoLogger;
    }

    private void multiScript_ScriptUpdated(object? sender, ScriptUpdatedEventArgs e)
    {
        if (_adding)
        {
            return;
        }

        // Has the update to the MultiScript removed one of the scripts? If so we need
        // to remove it from this wrapper too.
        if (e.RemovedScript != null)
        {
            foreach (var es in _scripts.ToArray())
            {
                var s = (EditableScriptBase) es;
                if (s.Script == e.RemovedScript)
                {
                    _scripts.Remove(es);
                }
            }
        }

        if (e.AddedScript != null)
        {
            Add(_controller.ScriptFactory.CreateEditableScript(e.AddedScript), true);
        }

        if (e.InsertedScript != null)
        {
            Add(_controller.ScriptFactory.CreateEditableScript(e.InsertedScript), e.Index, true);
        }

        if (e.ScriptsReplaced)
        {
            _replacingScripts = true;
            _scripts.Clear();
            foreach (var script in ((MultiScript) sender!).Scripts)
            {
                Add(_controller.ScriptFactory.CreateEditableScript(script), true);
            }

            _replacingScripts = false;
        }

        if (Updated != null)
        {
            Updated(this, new EditableScriptsUpdatedEventArgs());
        }

        if (UnderlyingValueUpdated != null)
        {
            UnderlyingValueUpdated(this, new DataWrapperUpdatedEventArgs());
        }

        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);
    }

    private void script_Updated(object? sender, EditableScriptUpdatedEventArgs e)
    {
        if (Updated != null)
        {
            Updated(this, new EditableScriptsUpdatedEventArgs((IEditableScript) sender!, e));
        }

        if (UnderlyingValueUpdated != null)
        {
            UnderlyingValueUpdated(this, new DataWrapperUpdatedEventArgs());
        }

        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);
    }

    private void ClearScripts()
    {
        foreach (var script in _scripts)
        {
            script.Updated -= script_Updated;
        }

        _scripts.Clear();
    }

    #region IEditableScripts Members

    public IEnumerable<IEditableScript> Scripts => _scripts.AsReadOnly();

    private void Add(EditableScriptBase script, bool fromUpdate)
    {
        Add(script, null, fromUpdate);
    }

    private void Add(EditableScriptBase script, int? index, bool fromUpdate)
    {
        script.Updated += script_Updated;
        if (index.HasValue)
        {
            _scripts.Insert(index.Value, script);
        }
        else
        {
            _scripts.Add(script);
        }

        if (_underlyingScript == null)
        {
            InitialiseMultiScript(new MultiScript(_controller.WorldModel));
        }

        if (!fromUpdate)
        {
            // Add underlying script to multiscript.
            // We don't always want to do this - we might be responding
            // to a multiscript update in the first place so no point adding the same
            // script again!

            _adding = true;
            _underlyingScript.Add(script.Script);
            _adding = false;
        }

        Debug.Assert(_replacingScripts || _underlyingScript.Scripts.Count() == _scripts.Count);
    }

    // TO DO: This is a temporary hacky flag to prevent re-entrant updates. What we should be doing instead is
    // never adding to our own wrapped _scripts collection unless we receive an update from the underlying
    // MultiScript.
    private bool _adding;

    public void AddNew(string keyword, string elementName)
    {
        _controller.WorldModel.UndoLogger.StartTransaction(string.Format("Add '{0}' script to '{1}'", keyword,
            elementName));
        AddNewInternal(keyword);
        _controller.WorldModel.UndoLogger.EndTransaction();
    }

    internal void AddNewInternal(string? keyword)
    {
        EditableScriptBase script;
        if (!string.IsNullOrEmpty(keyword))
        {
            script = _controller.ScriptFactory.CreateEditableScript(keyword);
        }
        else
        {
            script = _controller.ScriptFactory.CreateEditableFunctionCallScript();
        }

        Add(script, false);
    }

    public IEditableScript this[int index] => _scripts[index];

    public void Remove(int[] indexes)
    {
        var desc = indexes.Length == 0
            ? string.Format("Remove '{0}' script", _scripts[indexes[0]].DisplayString())
            : string.Format("Remove {0} scripts", indexes.Length);

        _controller.WorldModel.UndoLogger.StartTransaction(desc);

        var indexesDescending = from index in indexes
            orderby index descending
            select index;

        foreach (var index in indexesDescending)
        {
            _scripts.Remove(_scripts[index]);
            _underlyingScript.Remove(index);
        }

        _controller.WorldModel.UndoLogger.EndTransaction();

        Debug.Assert(_underlyingScript.Scripts.Count() == _scripts.Count);
    }

    public int Count => _scripts.Count;

    #endregion
}