<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript">
        function addText(text) {
            $("#divOutput").html($("#divOutput").html() + text);

            // TO DO: Add text within its own <div> with its own id, then use
            // jQuery's .position() to get actual position of latest text, and use
            // that value for scrollTop.
            $("#divOutput").scrollTop($("#divOutput").scrollTop() + 500);
        }

        function updateLocation(text) {
            $("#location").html("<b>" + text + "</b>");
        }

        function enterCommand(command) {
            $("#fldCommand").val(command);
            $("#cmdSubmit").click();
        }

        function keyPressCode(e) {
            var keynum
            if (window.event) { // IE
                keynum = e.keyCode
            } else if (e.which) { // Netscape/Firefox/Opera
                keynum = e.which
            }
            return keynum;
        }

        function commandKey(e) {
            switch (keyPressCode(e)) {
                case 13:

                    runCommand();
                    break;
                //                case 38: 
                //                    thisCommand--; 
                //                    if (thisCommand == 0) thisCommand = numCommands; 
                //                    command.value = commandsList[thisCommand]; 
                //                    break; 
                //                case 40: 
                //                    thisCommand++; 
                //                    if (thisCommand > numCommands) thisCommand = 1; 
                //                    command.value = commandsList[thisCommand]; 
                //                    break; 
                //                case 27: 
                //                    thisCommand = numCommands + 1; 
                //                    command.value = ''; 
                //                    break; 
            }
        }

        function runCommand() {
            //numCommands++;
            //commandsList[numCommands] = command.value;
            //thisCommand = numCommands + 1;

            // hitting Enter automatically causes the form to be submitted
            $("#fldCommand").val($("#txtCommand").val());
            $("#txtCommand").val("");
        }


    </script>
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
    </style>
    <title>Player</title>
</head>
<body>
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
            <asp:Button ID="cmdSubmit" runat="server" Width="20px" OnClick="cmdSubmit_Click"
                CssClass="hiddenbutton" />
            <asp:Timer ID="tmrInit" runat="server" Interval="50" OnTick="InitTimerTick">
            </asp:Timer>
            <asp:Timer ID="tmrTick" runat="server" Interval="1000" OnTick="TimerTick" Enabled="False">
            </asp:Timer>
        </ContentTemplate>
    </asp:UpdatePanel>
    <input type="text" id="txtCommand" style="width: 400px" onkeydown="commandKey(event);" />
    </form>
</body>
</html>
