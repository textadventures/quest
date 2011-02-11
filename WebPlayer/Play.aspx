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
            <h2>
                Inventory</h2>
            <input id="cmdInventory1" type="button" value="Look at" onclick="paneButtonClick('#lstInventory','look at');" />
            <input id="cmdInventory2" type="button" value="Use" onclick="paneButtonClick('#lstInventory','use');" />
            <input id="cmdInventory3" type="button" value="Drop" onclick="paneButtonClick('#lstInventory','drop');" />
            <select id="lstInventory" size="8" class="elementList">
            </select>
            <div id="statusVars"></div>
            <h2>
                Places &amp; Objects</h2>
            <div id="objectVerbs">
                <input id="cmdPlacesObjects1" type="button" value="Look at" onclick="paneButtonClick('#lstPlacesObjects','look at');" />
                <input id="cmdPlacesObjects2" type="button" value="Take" onclick="paneButtonClick('#lstPlacesObjects','take');" />
                <input id="cmdPlacesObjects3" type="button" value="Speak to" onclick="paneButtonClick('#lstPlacesObjects','speak to');" />
            </div>
            <div id="placeVerbs">
                <input id="cmdPlacesObjectsGoTo" type="button" value="Go to" onclick="paneButtonClick('#lstPlacesObjects','go to');" />
            </div>
            <select id="lstPlacesObjects" size="8" class="elementList" onclick="updateVerbs();">
            </select>
            <h2>
                Compass</h2>
            <table>
                <tr>
                    <td>
                        <table>
                            <tr>
                                <td>
                                    <input id="cmdCompassNW" class="compassbutton" type="button" value="&#8598;" title="go northwest"
                                        onclick="compassClick('northwest');" />
                                </td>
                                <td>
                                    <input id="cmdCompassN" class="compassbutton" type="button" value="&#8593;" title="go north"
                                        onclick="compassClick('north');" />
                                </td>
                                <td>
                                    <input id="cmdCompassNE" class="compassbutton" type="button" value="&#8599;" title="go northeast"
                                        onclick="compassClick('northeast');" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <input id="cmdCompassW" class="compassbutton" type="button" value="&#8592;" title="go west"
                                        onclick="compassClick('west');" />
                                </td>
                                <td>
                                    <input id="cmdCompassOut" class="compassbutton" type="button" value="out" title="go out"
                                        onclick="compassClick('out');" />
                                </td>
                                <td>
                                    <input id="cmdCompassE" class="compassbutton" type="button" value="&#8594;" title="go east"
                                        onclick="compassClick('east');" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <input id="cmdCompassSW" class="compassbutton" type="button" value="&#8601;" title="go southwest"
                                        onclick="compassClick('southwest');" />
                                </td>
                                <td>
                                    <input id="cmdCompassS" class="compassbutton" type="button" value="&#8595;" title="go south"
                                        onclick="compassClick('south');" />
                                </td>
                                <td>
                                    <input id="cmdCompassSE" class="compassbutton" type="button" value="&#8600;" title="go southeast"
                                        onclick="compassClick('southeast');" />
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td>
                        <input id="cmdCompassU" class="compassbutton" type="button" value="&#8679;" title="go up"
                            onclick="compassClick('up');" /><br />
                        <input id="cmdCompassD" class="compassbutton" type="button" value="&#8681;" title="go down"
                            onclick="compassClick('down');" />
                    </td>
                </tr>
            </table>
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
