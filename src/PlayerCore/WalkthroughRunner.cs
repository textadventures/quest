using QuestViva.Common;

namespace QuestViva.PlayerCore;

internal class WalkthroughRunner(IGameDebug game, string walkthrough)
{
    public delegate Task ClearBufferEventHandler();

    public delegate Task OutputEventHandler(string text);

    private readonly IGame _game = (IGame) game;
    private bool _cancelled;
    private IDictionary<string, string> _menuOptions;
    private bool _pausing;
    private bool _showingMenu;
    private bool _showingQuestion;
    private bool _waiting;

    public int Steps => game.Walkthroughs.Walkthroughs[walkthrough].Steps.Length;

    public event OutputEventHandler Output;

    public event ClearBufferEventHandler ClearBuffer;

    public async Task Run()
    {
        var delay = 0;
        var dateStart = DateTime.Now;
        foreach (var cmd in game.Walkthroughs.Walkthroughs[walkthrough].Steps)
        {
            if (_cancelled)
            {
                break;
            }

            if (_showingMenu)
            {
                await SetMenuResponse(cmd);
            }
            else if (_showingQuestion)
            {
                await SetQuestionResponse(cmd);
            }
            else if (cmd.StartsWith("assert:"))
            {
                var expr = cmd.Substring(7);
                WriteLine("<br><b>Assert:</b> " + expr);
                if (game.Assert(expr))
                {
                    WriteLine("<br><span style=\"color:green\"><b>Pass</b></span>");
                }
                else
                {
                    WriteLine("<br><span style=\"color:red\"><b>Failed</b></span>");
                    return;
                }
            }
            else if (cmd.StartsWith("label:"))
            {
                // ignore
            }
            else if (cmd.StartsWith("delay:"))
            {
                delay = int.Parse(cmd[6..].Trim());
            }
            else if (cmd.StartsWith("event:"))
            {
                var eventData = cmd.Substring(6).Split([';'], 2);
                var eventName = eventData[0];
                var param = eventData[1];
                _game.SendEvent(eventName, param);
            }
            else if (cmd.StartsWith("runtime:"))
            {
                var dateEnd = DateTime.Now;
                var diff = dateEnd.Subtract(dateStart);
                var res = $"{diff.Hours}:{diff.Minutes}:{diff.Seconds}";
                WriteLine("Walkthrough Runtime: " + res);
            }
            else
            {
                _game.SendCommand(cmd);
            }

            if (ClearBuffer != null)
            {
                await ClearBuffer.Invoke();
            }

            if (_cancelled)
            {
                break;
            }

            do
            {
                if (_waiting)
                {
                    _waiting = false;
                    await FinishWait();
                }

                if (_pausing)
                {
                    _pausing = false;
                    await FinishPause();
                }
            } while ((_waiting || _pausing) && !_cancelled);

            Thread.Sleep(delay);
        }
    }

    public void ShowMenu(MenuData menu)
    {
        _showingMenu = true;
        WriteLine("Menu: " + menu.Caption);
        _menuOptions = menu.Options;
    }

    private async Task SetMenuResponse(string response)
    {
        if (response.StartsWith("menu:"))
        {
            _showingMenu = false;
            var menuResponse = response[5..];
            WriteLine("  - " + menuResponse);
            if (_menuOptions.ContainsKey(menuResponse))
            {
                await _game.SetMenuResponse(menuResponse);
            }
            else
            {
                throw new Exception("Menu response was not an option");
            }
        }
        else
        {
            throw new Exception("No menu response defined in walkthrough");
        }
    }

    public void ShowQuestion(string question)
    {
        _showingQuestion = true;
        WriteLine("Question: " + question);
    }

    private async Task SetQuestionResponse(string response)
    {
        if (response.StartsWith("answer:"))
        {
            _showingQuestion = false;
            var questionResponse = response.Substring(7);
            WriteLine("  - " + questionResponse);
            switch (questionResponse)
            {
                case "yes":
                    await _game.SetQuestionResponse(true);
                    break;
                case "no":
                    await _game.SetQuestionResponse(false);
                    break;
                default:
                    throw new Exception("Question response was invalid");
            }
        }
        else
        {
            throw new Exception("Question response not defined in walkthrough");
        }
    }

    public void BeginWait()
    {
        _waiting = true;
    }

    private async Task FinishWait()
    {
        await _game.FinishWait();
    }

    public void BeginPause()
    {
        _pausing = true;
    }

    private async Task FinishPause()
    {
        await _game.FinishPause();
    }

    private void WriteLine(string text)
    {
        Output?.Invoke(text);
    }

    public void Cancel()
    {
        _cancelled = true;
    }
}