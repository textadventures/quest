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
        if (worldModel.EditMode)
        {
            ParametersAsQuestList.UndoLog = worldModel.UndoLogger;
        }

        Parameters = parameters;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var paramString = param.Save();
                ParametersAsQuestList.Add(paramString);
            }
        }
    }

    public IList<IFunction<object>> Parameters { get; }

    public QuestList<string> ParametersAsQuestList { get; } = new();
}