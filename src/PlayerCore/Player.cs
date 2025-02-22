#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using QuestViva.Common;

namespace QuestViva.PlayerCore;

public class Player : IPlayerHelperUI
{
    private PlayerHelper PlayerHelper { get; set; }
    private ListHandler ListHandler { get; }
    private List<(string, object?[]?)> JavaScriptBuffer { get; } = [];
    private bool Finished { get; set; } = false;
    private string ResourcesId { get; }
    public IJSRuntime JSRuntime { get; }
    public BlazorJSInterop JSInterop { get; }

    public Player(IGame game, string resourcesId, IJSRuntime jsRuntime)
    {
        ResourcesId = resourcesId;
        JSRuntime = jsRuntime;
        ListHandler = new ListHandler(AddJavaScriptToBuffer);
        JSInterop = new BlazorJSInterop(this);
        
        PlayerHelper = new PlayerHelper(game, this)
        {
            UseGameColours = true,
            UseGameFont = true
        };
        
        PlayerHelper.Game.LogError += LogError;
        PlayerHelper.Game.UpdateList += UpdateList;
        PlayerHelper.Game.Finished += GameFinished;
        if (game is IGameTimer gameTimer)
        {
            gameTimer.RequestNextTimerTick += RequestNextTimerTick;
        }
    }

    public async Task Initialise()
    {
        await JSRuntime.InvokeVoidAsync("WebPlayer.setDotNetHelper",
            DotNetObjectReference.Create(JSInterop));
        
        var (result, errors) = await PlayerHelper.Initialise(this);

        if (result)
        {
            RegisterExternalScripts();
            RegisterExternalStylesheets();
            
            PlayerHelper.Game.Begin();
            await ClearBuffer();
        }
        else
        {
            OutputText(string.Join("<br/>", errors));
            await ClearBuffer();
        }
    }

    public Stream? GetResource(string filename)
    {
        return PlayerHelper.Game.GetResource(filename);
    }
    
    private void AddJavaScriptToBuffer(string identifier, params object?[]? args)
    {
        JavaScriptBuffer.Add((identifier, args));
    }
    
    private async Task ClearJavaScriptBuffer()
    {
        while (JavaScriptBuffer.Count != 0)
        {
            var buffer = JavaScriptBuffer.ToArray();
            JavaScriptBuffer.Clear();
        
            foreach (var (identifier, args) in buffer)
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync(identifier, args);
                }
                catch (Exception ex)
                {
                    AddJavaScriptToBuffer("console.error", ex.Message);
                }
            }
        }
    }
    
    private void RequestNextTimerTick(int seconds)
    {
        AddJavaScriptToBuffer("requestNextTimerTick", seconds);
    }
    
    private void LogError(Exception ex)
    {
        OutputText("[Sorry, an error occurred]");
        AddJavaScriptToBuffer("console.error", ex.Message);
    }

    private void UpdateList(ListType listType, List<ListData> items)
    {
        ListHandler.UpdateList(listType, items);
    }
    
    private void GameFinished()
    {
        AddJavaScriptToBuffer("gameFinished");
        Finished = true;
    }

    void IPlayerHelperUI.SetAlignment(string alignment)
    {
        if (alignment.Length == 0) alignment = "left";
        OutputText(PlayerHelper.ClearBuffer());
        AddJavaScriptToBuffer("createNewDiv", alignment);
    }

    void IPlayerHelperUI.BindMenu(string linkid, string verbs, string text, string elementId)
    {
        AddJavaScriptToBuffer("bindMenu", linkid, verbs, text, elementId);
    }

    void IPlayer.ShowMenu(MenuData menuData)
    {
        AddJavaScriptToBuffer("showMenu", menuData.Caption, menuData.Options, menuData.AllowCancel);
    }

    void IPlayer.DoWait()
    {
        AddJavaScriptToBuffer("beginWait");
    }

    void IPlayer.DoPause(int ms)
    {
        AddJavaScriptToBuffer("beginPause", ms);
    }

    void IPlayer.ShowQuestion(string caption)
    {
        AddJavaScriptToBuffer("showQuestion", caption);
    }

    void IPlayer.SetWindowMenu(MenuData menuData)
    {
        // Do nothing - only implemented for desktop player
    }

    string IPlayer.GetNewGameFile(string originalFilename, string extensions)
    {
        throw new NotImplementedException();
    }

    void IPlayer.PlaySound(string filename, bool synchronous, bool looped)
    {
        string? functionName = null;
        if (filename.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase)) functionName = "playWav";
        if (filename.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase)) functionName = "playMp3";

        if (functionName == null) return;
        
        var url = GetURL(filename);
            
        AddJavaScriptToBuffer(
            functionName,
            url,
            synchronous,
            looped);
    }

    void IPlayer.StopSound()
    {
        AddJavaScriptToBuffer("stopAudio");
    }

    void IPlayer.WriteHTML(string html)
    {
        OutputText(html);
    }

    string IPlayer.GetURL(string file)
    {
        return GetURL(file);
    }
    
    string GetURL(string file)
    {
        // We might be running on a case-sensitive file system, so look up the correct casing
        var resource = PlayerHelper.Game.GetResourceNames()
            .FirstOrDefault(n => string.Equals(n, file, StringComparison.InvariantCultureIgnoreCase));
        
        return $"/game/{ResourcesId}/{resource ?? file}";
    }

    void IPlayer.LocationUpdated(string location)
    {
        AddJavaScriptToBuffer("updateLocation", location);
    }
    
    void IPlayer.UpdateGameName(string name)
    {
        AddJavaScriptToBuffer("setGameName", name);
    }

    void IPlayer.ClearScreen()
    {
        OutputText(PlayerHelper.ClearBuffer());
        AddJavaScriptToBuffer("clearScreen");
    }

    void IPlayer.ShowPicture(string filename)
    {
        OutputText(PlayerHelper.ClearBuffer());
        var url = GetURL(filename);
        OutputText($"<img src=\"{url}\" onload=\"scrollToEnd();\" /><br />");
    }

    void IPlayer.SetPanesVisible(string data)
    {
        AddJavaScriptToBuffer("panesVisible", data == "on");
    }

    void IPlayer.SetStatusText(string text)
    {
        AddJavaScriptToBuffer("updateStatus", text.Replace(Environment.NewLine, "<br />"));
    }

    void IPlayer.SetBackground(string colour)
    {
        AddJavaScriptToBuffer("setBackground", colour);
    }

    void IPlayer.SetForeground(string colour)
    {
        AddJavaScriptToBuffer("setForeground", colour);
        PlayerHelper.SetForeground(colour);
    }

    void IPlayer.SetLinkForeground(string colour)
    {
        PlayerHelper.SetLinkForeground(colour);
    }

    void IPlayer.RunScript(string function, object[] parameters)
    {
        // Clear text buffer before running custom JavaScript, otherwise text written
        // before now may appear after inserted HTML.
        OutputText(PlayerHelper.ClearBuffer());
        AddJavaScriptToBuffer(function, parameters);
    }

    void IPlayer.Quit()
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.SetFont(string fontName)
    {
        PlayerHelper.SetFont(fontName);
    }

    void IPlayer.SetFontSize(string fontSize)
    {
        PlayerHelper.SetFontSize(fontSize);
    }

    void IPlayer.Speak(string text)
    {
        // Do nothing
    }

    void IPlayer.RequestSave(string html)
    {
        // TODO
        throw new NotImplementedException();
    }

    void IPlayer.Show(string element)
    {
        DoShowHide(element, true);
    }

    void IPlayer.Hide(string element)
    {
        DoShowHide(element, false);
    }
    
    private static readonly Dictionary<string, string> ElementMap = new()
    {
        { "Panes", "#gamePanes" },
        { "Location", "#location" },
        { "Command", "#txtCommandDiv" }
    };

    private static string? GetElementId(string code)
    {
        ElementMap.TryGetValue(code, out var id);
        return id;
    }
    
    private void DoShowHide(string element, bool show)
    {
        var jsElement = GetElementId(element);
        if (string.IsNullOrEmpty(jsElement)) return;
        AddJavaScriptToBuffer(show ? "uiShow" : "uiHide", jsElement);
    }

    void IPlayer.SetCompassDirections(IEnumerable<string> dirs)
    {
        AddJavaScriptToBuffer("setCompassDirections", dirs);
    }

    void IPlayer.SetInterfaceString(string name, string text)
    {
        AddJavaScriptToBuffer("setInterfaceString", name, text);
    }

    void IPlayer.SetPanelContents(string html)
    {
        AddJavaScriptToBuffer("setPanelContents", html);
    }

    void IPlayer.Log(string text)
    {
        AddJavaScriptToBuffer("console.log", text);
    }

    string? IPlayer.GetUIOption(UIOption option)
    {
        if (option == UIOption.UseGameColours || option == UIOption.UseGameFont)
        {
            return "true";
        }

        return null;
    }

    void IPlayerHelperUI.OutputText(string text)
    {
        OutputText(text);
    }
    
    private void RegisterExternalScripts()
    {
        var scripts = PlayerHelper.Game.GetExternalScripts();
        if (scripts == null) return;
        
        foreach (var script in scripts)
        {
            var url = GetURL(script);
            AddJavaScriptToBuffer("addExternalScript", url);
        }
    }

    private void RegisterExternalStylesheets()
    {
        // TODO
        
        // var stylesheets = m_player.GetExternalStylesheets();
        // if (stylesheets == null) return;
        //
        // foreach (var stylesheet in stylesheets)
        // {
        //     m_buffer.AddJavaScriptToBuffer("addExternalStylesheet", new StringParameter(stylesheet));
        // }
    }
    
    private async Task ClearBuffer()
    {
        OutputText(PlayerHelper.ClearBuffer());
        await ClearJavaScriptBuffer();
    }
    
    private void OutputText(string text)
    {
        if (text.Length == 0) return;
        AddJavaScriptToBuffer("addTextAndScroll", text);
    }
    
    private async Task UiActionAsync(Action action)
    {
        if (Finished) return;
        action();
        await ClearBuffer();
    }
    
    public async Task UiSendCommandAsync(string command, int tickCount, IDictionary<string, string> metadata)
    {
        await UiActionAsync(() => PlayerHelper.SendCommand(command, tickCount, metadata));
    }
    
    public async Task UiEndWaitAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.FinishWait());
    }
    
    public async Task UiEndPauseAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.FinishPause());
    }
    
    public async Task UiChoiceAsync(string choice)
    {
        await UiActionAsync(() => PlayerHelper.Game.SetMenuResponse(choice));
    }
    
    public async Task UiChoiceCancelAsync()
    {
        await UiActionAsync(() => PlayerHelper.Game.SetMenuResponse(null));
    }
    
    public async Task UiTickAsync(int tickCount)
    {
        await UiActionAsync(() => PlayerHelper.GameTimer.Tick(tickCount));
    }
    
    public async Task UiSetQuestionResponseAsync(bool response)
    {
        await UiActionAsync(() => PlayerHelper.Game.SetQuestionResponse(response));
    }
    
    public async Task UiSendEventAsync(string eventName, string param)
    {
        await UiActionAsync(() => PlayerHelper.Game.SendEvent(eventName, param));
    }
    
    public async Task UiSaveGameAsync(string html)
    {
        await UiActionAsync(() =>
        {
            var data = PlayerHelper.Game.Save(html);
            AddJavaScriptToBuffer("saveGameResponse", data);
        });
    }
}