using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.Legacy;
using TextAdventures.Quest;

namespace QuestViva.PlayerCore;

public class GameQuery(string filename)
{
    private PlayerHelper _helper;
    private readonly GameQueryUi _dummyUi = new GameQueryUi();
    private readonly List<string> _errors = [];
    private IGame _game;
    private V4Game _v4Game;
    private WorldModel _v5Game;

    public async Task<bool> Initialise()
    {
        var gameDataProvider = new FileGameDataProvider(filename, "dummy-resources-id");
        var gameData = await gameDataProvider.GetData();
        
        _game = GameLauncher.GetGame(gameData, null);
        _v4Game = _game as V4Game;
        _v5Game = _game as WorldModel;
        _helper = new PlayerHelper(_game, _dummyUi);

        try
        {
            var (initialised, errors) = await _helper.Initialise(_dummyUi);
            if (!initialised)
            {
                _errors.AddRange(errors);
                return false;
            }
        }
        catch (Exception ex)
        {
            _errors.Add(ex.Message);
            return false;
        }

        return true;
    }

    public string GameName => _dummyUi.GameName;

    public int ASLVersion
    {
        get
        {
            if (_v4Game != null)
            {
                return _v4Game.ASLVersion;
            }
            if (_v5Game != null)
            {
                return _v5Game.ASLVersion;
            }
            throw new InvalidOperationException();
        }
    }

    public string GameId => _game.GameID;

    public string Category
    {
        get
        {
            if (_v4Game != null)
            {
                return null;
            }
            if (_v5Game != null)
            {
                return _v5Game.Category;
            }
            throw new InvalidOperationException();
        }
    }

    public string Description
    {
        get
        {
            if (_v4Game != null)
            {
                return null;
            }
            if (_v5Game != null)
            {
                return _v5Game.Description;
            }
            throw new InvalidOperationException();
        }
    }

    public string Cover
    {
        get
        {
            if (_v4Game != null)
            {
                return null;
            }
            if (_v5Game != null)
            {
                return string.IsNullOrEmpty(_v5Game.Cover) ? null : _v5Game.GetResourcePath(_v5Game.Cover);
            }
            throw new InvalidOperationException();
        }
    }

    public IEnumerable<string> Errors => _errors.AsReadOnly();

    public IEnumerable<string> GetResourceNames() => _game.GetResourceNames();

    public Stream GetResource(string resourceName)
    {
        return _game.GetResource(resourceName);
    }

    private class GameQueryUi : IPlayerHelperUI
    {
        public string GameName { get; private set; }

        public void OutputText(string text)
        {
        }

        public void SetAlignment(string alignment)
        {
        }

        public void BindMenu(string linkid, string verbs, string text, string elementId)
        {
        }

        public void ShowMenu(MenuData menuData)
        {
            throw new NotImplementedException();
        }

        public void DoWait()
        {
            throw new NotImplementedException();
        }

        public void DoPause(int ms)
        {
            throw new NotImplementedException();
        }

        public void ShowQuestion(string caption)
        {
            throw new NotImplementedException();
        }

        public void SetWindowMenu(MenuData menuData)
        {
        }

        public string GetNewGameFile(string originalFilename, string extensions)
        {
            throw new NotImplementedException();
        }

        public void PlaySound(string filename, bool synchronous, bool looped)
        {
        }

        public void StopSound()
        {
        }

        public void WriteHTML(string html)
        {
        }

        public string GetURL(string file)
        {
            throw new NotImplementedException();
        }

        public void LocationUpdated(string location)
        {
        }

        public void UpdateGameName(string name)
        {
            GameName = name;
        }

        public void ClearScreen()
        {
        }

        public void ShowPicture(string filename)
        {
        }

        public void SetPanesVisible(string data)
        {
        }

        public void SetStatusText(string text)
        {
        }

        public void SetBackground(string colour)
        {
        }

        public void SetForeground(string colour)
        {
        }

        public void SetLinkForeground(string colour)
        {
        }

        public void RunScript(string function, object[] parameters)
        {
        }

        public void Quit()
        {
        }

        public void SetFont(string fontName)
        {
        }

        public void SetFontSize(string fontSize)
        {
        }

        public void Speak(string text)
        {
        }

        public void RequestSave(string html)
        {
            throw new NotImplementedException();
        }

        public void Show(string element)
        {
        }

        public void Hide(string element)
        {
        }

        public void SetCompassDirections(IEnumerable<string> dirs)
        {
        }

        public void SetInterfaceString(string name, string text)
        {
        }

        public void SetPanelContents(string html)
        {
        }

        public void Log(string text)
        {
        }

        public string GetUIOption(UIOption option)
        {
            return null;
        }
    }
}