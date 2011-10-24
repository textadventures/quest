<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta name="viewport" content="width=device-width, user-scalable=no, initial-scale=0.9" />
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.min.js"></script>
    <script type="text/javascript" src="js/jquery.jplayer.min.js"></script>
    <script type="text/javascript" src="js/jjmenu.js"></script>
    <link rel="Stylesheet" type="text/css" href="playercore.css" />
    <link id="styleLink" runat="server" rel="Stylesheet" type="text/css" href="fixed.css" />
    <link rel="Stylesheet" type="text/css" href="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/themes/redmond/jquery-ui.css" />
    <link rel="Stylesheet" type="text/css" href="js/jjmenu.css" />
    <script type="text/javascript" src="playercore.js"></script>
    <script type="text/javascript" src="player.js"></script>
</head>
<body onkeydown="globalKey(event);" onload="init();">
    <form id="playerform" runat="server" defaultbutton="cmdSubmit">
    <asp:ScriptManager ID="ctlScriptManager" runat="server">
    </asp:ScriptManager>
    <div id="gameBorder">
        <div id="status">
            <div id="loginData">
                <asp:Label ID="loggedIn" runat="server"></asp:Label>
            </div>
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
                <table>
                    <tr>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <input id="cmdCompassNW" class="compassbutton" type="button" value="&#8598;" title="go northwest"
                                            onclick="compassClick(_compassDirs[0]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassN" class="compassbutton" type="button" value="&#8593;" title="go north"
                                            onclick="compassClick(_compassDirs[1]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassNE" class="compassbutton" type="button" value="&#8599;" title="go northeast"
                                            onclick="compassClick(_compassDirs[2]);" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <input id="cmdCompassW" class="compassbutton" type="button" value="&#8592;" title="go west"
                                            onclick="compassClick(_compassDirs[3]);" />
                                    </td>
                                    <td>
                                        &nbsp;
                                    </td>
                                    <td>
                                        <input id="cmdCompassE" class="compassbutton" type="button" value="&#8594;" title="go east"
                                            onclick="compassClick(_compassDirs[4]);" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <input id="cmdCompassSW" class="compassbutton" type="button" value="&#8601;" title="go southwest"
                                            onclick="compassClick(_compassDirs[5]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassS" class="compassbutton" type="button" value="&#8595;" title="go south"
                                            onclick="compassClick(_compassDirs[6]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassSE" class="compassbutton" type="button" value="&#8600;" title="go southeast"
                                            onclick="compassClick(_compassDirs[7]);" />
                                    </td>
                                </tr>
                            </table>
                        </td>
                        <td>
                            <table>
                                <tr>
                                    <td>
                                        <input id="cmdCompassU" class="compassbutton" type="button" value="&#8679;" title="go up"
                                            onclick="compassClick(_compassDirs[8]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassIn" class="compassbutton" type="button" value="in" title="go in"
                                            onclick="compassClick(_compassDirs[10]);" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <input id="cmdCompassD" class="compassbutton" type="button" value="&#8681;" title="go down"
                                            onclick="compassClick(_compassDirs[9]);" />
                                    </td>
                                    <td>
                                        <input id="cmdCompassOut" class="compassbutton" type="button" value="out" title="go out"
                                            onclick="compassClick(_compassDirs[11]);" />
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
            <input type="text" id="txtCommand" onkeydown="return commandKey(event);" placeholder="Type here..."
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
