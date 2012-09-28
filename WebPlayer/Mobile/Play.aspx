<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Mobile.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width; maximum-scale=1.0; minimum-scale=1.0;" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.7.1.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.16/jquery-ui.min.js"></script>
    <script type="text/javascript" src="../js/jquery.jplayer.min.js"></script>
    <script type="text/javascript" src="../js/jjmenu.js"></script>
    <script type="text/javascript" src="../js/jquery.multi-open-accordion-1.5.3.js"></script>
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.16/themes/redmond/jquery-ui.css" />
    <link rel="Stylesheet" type="text/css" href="../playercore.css" />
    <link rel="Stylesheet" type="text/css" href="player.css" />
    <link rel="Stylesheet" type="text/css" href="../js/jjmenu.css" />
    <script type="text/javascript" src="../playercore.js"></script>
    <script type="text/javascript" src="../player.js"></script>
    <script type="text/javascript" src="playermobile.js"></script>
    <% Response.Write(GetHead()); %>
</head>
<body onkeydown="globalKey(event);" onload="init();">
    <% Response.Write(GetBodyHeader()); %>    
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
    <div id="gameBorder">
        <div id="gamePanes" class="pane">
            <button type="button" class="backButton"></button>
            <h2 id="inventoryLabel" class="ui-widget-header">Inventory</h2>
            <div>
                <p id="inventoryEmpty" class="emptyListLabel">Empty</p>
                <ul class="elementList" id="inventoryList"></ul>
            </div>
            <h2 id="statusVarsLabel" class="ui-widget-header">Status</h2>
            <div id="statusVarsAccordion">
                <div id="statusVars">
                </div>
            </div>
        </div>
        <div id="gameObjects" class="pane">
            <button type="button" class="backButton"></button>
            <h2 id="placesObjectsLabel" class="ui-widget-header">Places &amp; Objects</h2>
            <div>
                <p id="placesObjectsEmpty" class="emptyListLabel">None</p>
                <ul class="elementList" id="objectsList"></ul>
            </div>
        </div>
        <div id="gameExits" class="pane">
            <button type="button" class="backButton"></button>
            <h2 id="compassLabel" class="ui-widget-header">Compass</h2>
            <div>
                <table id="compassTable">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <button id="cmdCompassNW" class="compassbutton" type="button" title="go northwest"
                                            onclick="compassClick(_compassDirs[0]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassN" class="compassbutton" type="button" title="go north"
                                            onclick="compassClick(_compassDirs[1]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassNE" class="compassbutton" type="button" title="go northeast"
                                            onclick="compassClick(_compassDirs[2]);"></button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassW" class="compassbutton" type="button" title="go west"
                                            onclick="compassClick(_compassDirs[3]);"></button>
                                    </td>
                                    <td>
                                        &nbsp;
                                    </td>
                                    <td>
                                        <button id="cmdCompassE" class="compassbutton" type="button" title="go east"
                                            onclick="compassClick(_compassDirs[4]);"></button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassSW" class="compassbutton" type="button" title="go southwest"
                                            onclick="compassClick(_compassDirs[5]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassS" class="compassbutton" type="button" title="go south"
                                            onclick="compassClick(_compassDirs[6]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassSE" class="compassbutton" type="button" title="go southeast"
                                            onclick="compassClick(_compassDirs[7]);"></button>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <button id="cmdCompassU" class="compassbutton" type="button" title="go up"
                                            onclick="compassClick(_compassDirs[8]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassIn" class="compassbutton" type="button" title="go in"
                                            onclick="compassClick(_compassDirs[10]);">in</button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassD" class="compassbutton" type="button" title="go down"
                                            onclick="compassClick(_compassDirs[9]);"></button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassOut" class="compassbutton" type="button" title="go out"
                                            onclick="compassClick(_compassDirs[11]);">out</button>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <div id="gameMore" class="pane">
            <button type="button" class="backButton"></button>
            <h2 class="ui-widget-header">More</h2>
            <div>
                <div id="controlButtons">
                    <button type="button" id="cmdSave" runat="server">Save</button>
                    <button type="button" id="cmdLook">Look</button>
                    <button type="button" id="cmdRestart">Restart</button>
                    <button type="button" id="cmdUndo">Undo</button>
                    <button type="button" id="cmdWait">Wait</button>
                </div>
            </div>
        </div>

        <div id="gameContent">
            <div id="divOutput">
                <h1 id="gameTitle">
                    Loading...</h1>
            </div>
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
            <div id="sessionTimeoutDiv">
                <b>Sorry, your session has expired and the game has finished.</b>
            </div>
            <div id="gameOverDiv">
                <b>Game over.</b>
            </div>
            <div id="txtCommandDiv">
                <table id="inputBar">
                    <tr>
                        <td>
                            <input type="text" id="txtCommand" onkeydown="return commandKey(event);" placeholder="Type here..."
                                autofocus autocorrect="off" autocapitalize="off" />
                        </td>
                        <td id="inputBarButtons">
                            <div id="tabButtonDiv">
                                <div id="tabButton" class="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" style="width: 30px; height: 30px;">
                                +
                                </button>
                            </div>
                        </td>
                    </tr>
                </table>
                <a id="endWaitLink" onclick="endWait();" class="cmdlink" style="display: none">Tap here to continue...</a>
            </div>
        </div>
    </div>
    <% Response.Write(GetBodyFooter()); %>
</body>
</html>
