<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.7.1.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.16/jquery-ui.min.js"></script>
    <script type="text/javascript" src="js/jquery.jplayer.min.js"></script>
    <script type="text/javascript" src="js/jjmenu.js"></script>
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.16/themes/redmond/jquery-ui.css" />
    <link rel="Stylesheet" type="text/css" href="playercore.css" />
    <link rel="Stylesheet" type="text/css" href="player.css" />
    <link rel="Stylesheet" type="text/css" href="js/jjmenu.css" />
    <script type="text/javascript" src="playercore.js"></script>
    <script type="text/javascript" src="player.js"></script>
    <% Response.Write(GetHead()); %>
</head>
<body onkeydown="globalKey(event);" onload="init();">
    <% Response.Write(GetBodyHeader()); %>
    <div id="updating">
        <asp:UpdateProgress ID="ctlUpdateProgress" runat="server" DisplayAfter="250">
            <ProgressTemplate>
                <img src="updating.gif" alt="Updating..." />
            </ProgressTemplate>
        </asp:UpdateProgress>
    </div>
    <div id="gameBorder">
        <div id="status" class="ui-widget-header">
            <div id="controlButtons">
                <button type="button" id="cmdSave" runat="server">Save</button>
            </div>
            <div id="location">
            </div>
        </div>
        <div id="gamePanes">
            <div id="gamePanesRunning">
                <h2 id="inventoryLabel">Inventory</h2>
                <input id="cmdInventory1" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory2" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory3" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory4" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory5" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory6" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory7" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory8" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <input id="cmdInventory9" type="button" value="" onclick="paneButtonClick('#lstInventory',this.value);" style="display:none" />
                <select id="lstInventory" size="8" class="elementList">
                </select>
                <div id="statusVars">
                </div>
                <h2 id="placesObjectsLabel">Places &amp; Objects</h2>
                <input id="cmdPlacesObjects1" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects2" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects3" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects4" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects5" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects6" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects7" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects8" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <input id="cmdPlacesObjects9" type="button" value="" onclick="paneButtonClick('#lstPlacesObjects',this.value);" style="display:none" />
                <select id="lstPlacesObjects" size="8" class="elementList">
                </select>
                <h2 id="compassLabel">Compass</h2>
                <table id="compassTable">
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <button id="cmdCompassNW" class="compassbutton" type="button" title="go northwest"
                                            onclick="compassClick(_compassDirs[0]);">&#8598;</button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassN" class="compassbutton" type="button" title="go north"
                                            onclick="compassClick(_compassDirs[1]);">&#8593;</button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassNE" class="compassbutton" type="button" title="go northeast"
                                            onclick="compassClick(_compassDirs[2]);">&#8599;</button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassW" class="compassbutton" type="button" title="go west"
                                            onclick="compassClick(_compassDirs[3]);">&#8592;</button>
                                    </td>
                                    <td>
                                        &nbsp;
                                    </td>
                                    <td>
                                        <button id="cmdCompassE" class="compassbutton" type="button" title="go east"
                                            onclick="compassClick(_compassDirs[4]);">&#8594;</button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassSW" class="compassbutton" type="button" title="go southwest"
                                            onclick="compassClick(_compassDirs[5]);">&#8601;</button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassS" class="compassbutton" type="button" title="go south"
                                            onclick="compassClick(_compassDirs[6]);">&#8595;</button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassSE" class="compassbutton" type="button" title="go southeast"
                                            onclick="compassClick(_compassDirs[7]);">&#8600;</button>
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <button id="cmdCompassU" class="compassbutton" type="button" title="go up"
                                            onclick="compassClick(_compassDirs[8]);">&#8679;</button>
                                    </td>
                                    <td>
                                        <button id="cmdCompassIn" class="compassbutton" type="button" title="go in"
                                            onclick="compassClick(_compassDirs[10]);">in</button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <button id="cmdCompassD" class="compassbutton" type="button" title="go down"
                                            onclick="compassClick(_compassDirs[9]);">&#8681;</button>
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
            <div id="gamePanesFinished">
                <h2>Game Over</h2>
                <p>This game has finished.</p>
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
            <div id="txtCommandSpacer"></div>
            <div id="txtCommandDiv">
            <span id="txtCommandPrompt">&gt;</span>
            <input type="text" id="txtCommand" onkeydown="return commandKey(event);" placeholder="Type here..."
                autofocus />
            </div>
            <a id="endWaitLink" onclick="endWait();" class="cmdlink" style="display: none">Click
                here or press a key to continue...</a>
        </div>
    </div>
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
    <% Response.Write(GetBodyFooter()); %>
</body>
</html>
