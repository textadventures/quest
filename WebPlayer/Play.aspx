<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.min.js"></script>
    <script type="text/javascript" src="js/jquery.jplayer.min.js"></script>
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/themes/redmond/jquery-ui.css" />
    <script type="text/javascript" src="player.js"></script>
    <style type="text/css">
        body
        {
            margin: 0px;
            font-family: Arial, Helvetica, sans-serif;
        }
        a.cmdlink
        {
            text-decoration: underline;
            color: Blue;
            cursor: pointer;
        }
        div#gameBorder
        {
            width: 800px;
            margin-left: auto;
            margin-right: auto;
            border: 1px solid silver;
            margin-top: 0;
        }
        div#gameContent
        {
            padding: 20px;
        }
        div#status
        {
            background: #E0E0FF;
            height: 20px;
            position: fixed;
            top: 0px;
            z-index: 100;
            width: 796px;
            margin-left: auto;
            margin-right: auto;
            font-size: 14pt;
            padding: 2px;
        }
        div#updating
        {
            float: right;
        }
        div#divOutput
        {
            padding: 8px; /* height: 400px; */
        }
        #txtCommand
        {
            width: 100%;
            font-size: 14pt;
        }
        .hiddenbutton
        {
            display: none;
        }
        #dialog
        {
            display: none;
        }
        #msgbox
        {
            display: none;
        }
    </style>
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
