﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <!-- <% Response.Write(GetVersionHeader()); %> -->
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-2.1.1.min.js"></script>
    <script type="text/javascript" src="https://ajax.aspnetcdn.com/ajax/jquery.ui/1.11.2/jquery-ui.min.js"></script>
    <script type="text/javascript" src="js/jquery.jplayer.min.js"></script>
    <script type="text/javascript" src="js/jquery.multi-open-accordion-1.5.3.js"></script>
    <link rel="Stylesheet" type="text/css" href="https://ajax.aspnetcdn.com/ajax/jquery.ui/1.11.1/themes/redmond/jquery-ui.min.css" />
    <link rel="Stylesheet" type="text/css" href="<%= CacheBuster("playercore.css") %>" />
    <link rel="Stylesheet" type="text/css" href="<%= CacheBuster("player.css") %>" />
    <script type="text/javascript" src="<%= CacheBuster("playercore.js") %>"></script>
    <script type="text/javascript" src="<%= CacheBuster("player.js") %>"></script>
    <script type="text/javascript" src="<%= CacheBuster("playerweb.js") %>"></script>
    <script type="text/javascript" src="paper.js"></script>
    <script type="text/paperscript" src="<%= CacheBuster("grid.js") %>" canvas="gridCanvas"></script>
    <% Response.Write(GetHead()); %>
</head>
<body onload="init('<%= ApiRoot() %>', '<%= GameSessionLogId() %>');">
    <% Response.Write(GetBodyHeader()); %>
    <div style="display: none">
        <form id="playerform" runat="server" defaultbutton="cmdSubmit">
            <asp:ScriptManager ID="ctlScriptManager" runat="server">
            </asp:ScriptManager>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="fldUIMsg" runat="server" Value="" />
                    <asp:HiddenField ID="fldUITickCount" runat="server" Value="" />
                    <asp:Button ID="cmdSubmit" runat="server" Width="20px" OnClick="cmdSubmit_Click"
                        CssClass="hiddenbutton" />
                    <asp:Timer ID="tmrInit" runat="server" Interval="50" OnTick="InitTimerTick">
                    </asp:Timer>
                </ContentTemplate>
            </asp:UpdatePanel>
        </form>
    </div>
    <asp:UpdateProgress ID="ctlUpdateProgress" runat="server" DisplayAfter="500">
        <ProgressTemplate>
        <div id="updating">
            <img src="updating.gif" alt="Updating..." />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <% Response.Write(GetUI()); %>
    <div id="jquery_jplayer" style="width: 0; height: 0">
    </div>
    <div id="audio_embed">
    </div>
    <% Response.Write(GetBodyFooter()); %>
</body>
</html>
