<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.min.js"></script>
    <script type="text/javascript" src="js/jquery.jplayer.min.js"></script>
    <link rel="Stylesheet" type="text/css" href="player.css" />
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/themes/redmond/jquery-ui.css" />
    <script type="text/javascript" src="player.js"></script>
    <title>Player</title>
</head>
<body onkeydown="globalKey(event);" onload="init();">
    <form id="playerform" runat="server" defaultbutton="cmdSubmit">
    <asp:ScriptManager ID="ctlScriptManager" runat="server">
    </asp:ScriptManager>
    <div id="gameBorder">
        <div id="status">
            <div id="updating">
                <asp:UpdateProgress ID="ctlUpdateProgress" runat="server">
                    <ProgressTemplate>
                        <img src="updating.gif" alt="Updating..." />
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </div>
            <div id="location">
            </div>
        </div>
        <div id="gamePanes">
            <h2>Inventory</h2>
            <input id="Button1" type="button" value="Look at" />
            <input id="Button2" type="button" value="Use" />
            <input id="Button3" type="button" value="Drop" />
            <select id="lstInventory" size="8" class="elementList">
            </select>
            <h2>Places &amp; Objects</h2>
            <input id="Button4" type="button" value="Look at" />
            <input id="Button5" type="button" value="Take" />
            <input id="Button6" type="button" value="Speak to" />
            <select id="lstPlacesObjects" size="8" class="elementList">
            </select>
            <h2>Compass</h2>
            <input id="Button7" type="button" value="NW" />
            <input id="Button8" type="button" value="N" />
            <input id="Button9" type="button" value="NE" />
            <br />
            <input id="Button10" type="button" value="W" />
            <input id="Button11" type="button" value="out" />
            <input id="Button12" type="button" value="E" />
            <br />
            <input id="Button13" type="button" value="SW" />
            <input id="Button14" type="button" value="S" />
            <input id="Button15" type="button" value="SE" />

        </div>
        <div id="gameContent">
            <div id="divOutput">
                <h1 id="gameTitle">
                    Loading...</h1>
            </div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="fldCommand" runat="server" Value="" />
                    <asp:HiddenField ID="fldUIMsg" runat="server" Value="" />
                    <asp:Button ID="cmdSubmit" runat="server" Width="20px" OnClick="cmdSubmit_Click"
                        CssClass="hiddenbutton" />
                    <asp:Timer ID="tmrInit" runat="server" Interval="50" OnTick="InitTimerTick">
                    </asp:Timer>
                    <asp:Timer ID="tmrTick" runat="server" Interval="1000" OnTick="TimerTick" Enabled="False">
                    </asp:Timer>
                </ContentTemplate>
            </asp:UpdatePanel>
            <input type="text" id="txtCommand" onkeydown="commandKey(event);" placeholder="Type here..."
                autofocus />
            <a id="endWaitLink" onclick="endWait();" class="cmdlink" style="display: none">Click
                here or press a key to continue...</a>
        </div>
    </div>
    </form>
    <div id="dialog" title="Menu">
        <p id="dialogCaption">
        </p>
        <select id="dialogOptions" size="3">
        </select>
    </div>
    <div id="msgbox" title="Question">
        <p id="msgboxCaption">
        </p>
    </div>
    <div id="jquery_jplayer" style="width: 0; height: 0">
    </div>
    <div id="audio_embed">
    </div>
</body>
</html>
