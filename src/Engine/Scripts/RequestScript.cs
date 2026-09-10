using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

// Any changes here should also be reflected in CoreEditorScriptsOutput.aslx (validvalues for "request" command)
// and also in the documentation https://github.com/textadventures/quest/blob/gh-pages/scripts/request.md
internal enum Request
{
    Quit,
    UpdateLocation,
    GameName,
    FontName,
    FontSize,
    Background,
    Foreground,
    LinkForeground,
    RunScript,
    SetStatus,
    ClearScreen,
    PanesVisible,
    ShowPicture,
    Show,
    Hide,
    SetCompassDirections,
    Pause,
    Wait,
    SetInterfaceString,
    RequestSave,
    SetPanelContents,
    Log,
    Speak
}

public class RequestScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "request";

    protected override int[] ExpectedParameters
    {
        get { return new[] {2}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new RequestScript(scriptContext, parameters[0], new Expression<string>(parameters[1], scriptContext));
    }
}

public class RequestScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _data;
    private Request _request;

    public RequestScript(ScriptContext scriptContext, string request, IFunction<string> data)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _data = data;
        _request = (Request) Enum.Parse(typeof(Request), request);
    }

    public override string Keyword => "request";

    protected override ScriptBase CloneScript()
    {
        return new RequestScript(_scriptContext, _request.ToString(), _data.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var data = await _data.ExecuteAsync(c);

        // TO DO: Replace with dictionary mapping the enum to lambda functions
        switch (_request)
        {
            case Request.UpdateLocation:
                _worldModel.PlayerUi.LocationUpdated(data);
                break;
            case Request.GameName:
                _worldModel.PlayerUi.UpdateGameName(data);
                break;
            case Request.ClearScreen:
                _worldModel.PlayerUi.ClearScreen();
                _worldModel.OutputLogger!.Clear();
                break;
            case Request.ShowPicture:
                await _worldModel.PlayerUi.ShowPictureAsync(data);
                // TO DO: Picture should be added to the OutputLogger, but the data we
                // get here includes the full path/URL - we want the original filename
                // only, so this would be a breaking change.
                break;
            case Request.PanesVisible:
                _worldModel.PlayerUi.SetPanesVisible(data);
                break;
            case Request.Background:
                _worldModel.PlayerUi.SetBackground(data);
                break;
            case Request.Foreground:
                _worldModel.PlayerUi.SetForeground(data);
                break;
            case Request.RunScript:
                if (_worldModel.Version == WorldModelVersion.v500)
                {
                    // v500 games used Frame.js functions for static panel feature. This is now implemented natively
                    // in Player and WebPlayer.
                    if (data == "beginUsingTextFrame")
                    {
                        return;
                    }

                    if (data.StartsWith("setFramePicture;"))
                    {
                        var frameArgs = data.Split(';');
                        _worldModel.PlayerUi.SetPanelContents("<img src=\"" + frameArgs[1].Trim() +
                                                               "\" onload=\"setPanelHeight()\"/>");
                        return;
                    }

                    if (data == "clearFramePicture")
                    {
                        _worldModel.PlayerUi.SetPanelContents("");
                    }
                }

                var jsArgs = data.Split(';').Select(a => a.Trim()).ToArray();
                var functionName = jsArgs[0];
                if (jsArgs.Length == 0)
                {
                    await _worldModel.PlayerUi.RunScriptAsync(functionName, null);
                }
                else
                {
                    await _worldModel.PlayerUi.RunScriptAsync(functionName, jsArgs.Skip(1).ToArray());
                }

                break;
            case Request.Quit:
                _worldModel.Finish();
                break;
            case Request.FontName:
                if (_worldModel.Version >= WorldModelVersion.v540)
                {
                    throw new InvalidOperationException(
                        "FontName request is not supported for games with WorldModel version 540 or later.");
                }

                _worldModel.PlayerUi.SetFont(data);
                ((LegacyOutputLogger) _worldModel.OutputLogger!).SetFontName(data);
                break;
            case Request.FontSize:
                if (_worldModel.Version >= WorldModelVersion.v540)
                {
                    throw new InvalidOperationException(
                        "FontSize request is not supported for games with WorldModel version 540 or later.");
                }

                _worldModel.PlayerUi.SetFontSize(data);
                ((LegacyOutputLogger) _worldModel.OutputLogger!).SetFontSize(data);
                break;
            case Request.LinkForeground:
                _worldModel.PlayerUi.SetLinkForeground(data);
                break;
            case Request.Show:
                _worldModel.PlayerUi.Show(data);
                break;
            case Request.Hide:
                _worldModel.PlayerUi.Hide(data);
                break;
            case Request.SetCompassDirections:
                _worldModel.PlayerUi.SetCompassDirections(data.Split(';'));
                break;
            case Request.SetStatus:
                _worldModel.PlayerUi.SetStatusText(data.Replace("\n", Environment.NewLine));
                break;
            case Request.Pause:
                if (_worldModel.Version >= WorldModelVersion.v550 && _worldModel.Version < WorldModelVersion.v600)
                {
                    throw new Exception(
                        "The 'Pause' request is not supported for games with WorldModel version 550–580. Use the 'SetTimeout' function instead, or set the game's WorldModel version to 600 or later.");
                }

                int ms;
                if (int.TryParse(data, out ms))
                {
                    await _worldModel.DoPauseAsync(ms);
                }

                break;
            case Request.Wait:
                if (_worldModel.Version >= WorldModelVersion.v540 && _worldModel.Version < WorldModelVersion.v600)
                {
                    throw new Exception(
                        "The 'Wait' request is not supported for games with WorldModel version 540–580. Use the 'wait' script command instead, or set the game's WorldModel version to 600 or later.");
                }

                await _worldModel.DoWaitAsync();
                break;
            case Request.SetInterfaceString:
                var args = data.Split('=');
                _worldModel.PlayerUi.SetInterfaceString(args[0], args[1]);
                break;
            case Request.RequestSave:
                _worldModel.PlayerUi.RequestSave(null);
                break;
            case Request.SetPanelContents:
                _worldModel.PlayerUi.SetPanelContents(data);
                break;
            case Request.Log:
                _worldModel.PlayerUi.Log(data);
                break;
            case Request.Speak:
                _worldModel.PlayerUi.Speak(data);
                break;
            default:
                throw new ArgumentOutOfRangeException("request", "Unhandled request type");
        }
    }

    public override string Save()
    {
        return SaveScript("request", _request.ToString(), _data.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _request.ToString();
            case 1:
                return _data.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _request = (Request) Enum.Parse(typeof(Request), (string) value!);
                break;
            case 1:
                _data = new Expression<string>((string) value!, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}