#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;
// We store the parameters internally as a QuestList<string>, so we can edit them in the Editor
// using the standard string list editor control.

// Note that we haven't implemented any functionality to keep the two lists in sync, because
// when playing or editing a game we only actually care about one of the lists, and when playing
// a game there is no mechanism for modifying a script command.
internal class FunctionCallParameters
{
    public FunctionCallParameters(WorldModel worldModel, IList<IFunction<object>> parameters)
    {
        Parameters = parameters;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramString = param.Save();
                ParametersAsQuestList.Add(paramString);
            }
        }

        // Hook the list up to the undo logger only after populating it. The list doesn't exist
        // outside this constructor yet, so its initial contents are never something the user could
        // want to undo - but if a script is cloned while an undo transaction is open (which is what
        // "Make editable copy" does), every one of those initial Adds would otherwise be logged into
        // that transaction, and undoing it would strip the parameters back out of the clone again.
        if (worldModel.EditMode)
        {
            ParametersAsQuestList.UndoLog = worldModel.UndoLogger;
        }
    }

    public IList<IFunction<object>> Parameters { get; }

    public QuestList<string> ParametersAsQuestList { get; } = new();
}