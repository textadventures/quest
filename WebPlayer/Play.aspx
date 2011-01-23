<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.min.js"></script>
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/themes/redmond/jquery-ui.css" />
    <script type="text/javascript" src="player.js"></script>
    <style type="text/css">
        a.cmdlink
        {
            text-decoration: underline;
            color: Blue;
            cursor: pointer;
        }
        div#status
        {
            background: #E0E0FF;
            height: 20px;
        }
        div#updating
        {
            float: right;
        }
        div#divOutput
        {
            padding: 8px;
        }
        .hiddenbutton
        {
            display: none;
        }
        #dialog
        {
            display: none;
        }
    </style>
    <title>Player</title>
</head>
<body onkeydown="globalKey(event);">
    <form id="playerform" runat="server" defaultbutton="cmdSubmit">
    <asp:ScriptManager ID="ctlScriptManager" runat="server">
    </asp:ScriptManager>
    <h1>Player</h1>
    <div id="status">
        <div id="updating">
            <asp:UpdateProgress ID="ctlUpdateProgress" runat="server">
                <ProgressTemplate>
                    <img src="updating.gif" alt="Updating..." />
                </ProgressTemplate>
            </asp:UpdateProgress>
        </div>
        <div id="location"></div>
    </div>
    <div id="divOutput" runat="server" style="height: 400px; border: 1px solid silver;
        overflow: auto;">
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
    <input type="text" id="txtCommand" style="width: 400px" onkeydown="commandKey(event);" />
    <a id="endWaitLink" onclick="endWait();" class="cmdlink" style="display: none">Click
        here or press a key to continue...</a>
    </form>
    <div id="dialog" title="Menu">
        <p>Caption</p>
        <select id="dialogOptions" size="3">
        </select>
    </div>
</body>
</html>
