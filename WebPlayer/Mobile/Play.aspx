<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Mobile.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width; maximum-scale=1.0;" />
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
                <h3 id="inventoryLabel"><a href="#">Inventory</a></h3>
                <div>
                    <select id="lstInventory" size="8" class="elementList">
                    </select>
                    <div class="verbButtons">
                        <button id="cmdInventory1" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory2" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory3" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory4" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory5" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory6" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory7" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory8" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdInventory9" type="button" onclick="paneButtonClick('#lstInventory',$(this).data('verb'));" style="display:none"></button>
                    </div>
                </div>
                <h3 id="statusVarsLabel"><a href="#">Status</a></h3>
                <div id="statusVarsAccordion">
                    <div id="statusVars">
                    </div>
                </div>
                <h3 id="placesObjectsLabel"><a href="#">Places &amp; Objects</a></h3>
                <div>
                    <select id="lstPlacesObjects" size="8" class="elementList">
                    </select>
                    <div class="verbButtons">
                        <button id="cmdPlacesObjects1" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects2" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects3" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects4" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects5" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects6" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects7" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects8" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                        <button id="cmdPlacesObjects9" type="button" onclick="paneButtonClick('#lstPlacesObjects',$(this).data('verb'));" style="display:none"></button>
                    </div>
                </div>
                <h3 id="compassLabel"><a href="#">Compass</a></h3>
                <div>
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
            <div id="txtCommandDiv">
                <nobr>
                    <span id="txtCommandPrompt">&gt;</span>
                    <input type="text" id="txtCommand" onkeydown="return commandKey(event);" placeholder="Type here..."
                        autofocus />
                    <a id="endWaitLink" onclick="endWait();" class="cmdlink" style="display: none">Click
                        here or press a key to continue...</a>
                </nobr>
            </div>
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
