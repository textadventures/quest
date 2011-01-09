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

        function enterCommand(command) {
            $("#txtCommand").val(command);
            $("#cmdSubmit").click();
        }

    </script>
    <style type="text/css">
        a.cmdlink { text-decoration: underline; color: Blue; cursor: pointer };
    </style>
    <title>Player</title>
</head>
<body>
    <form id="playerform" runat="server">
    <asp:ScriptManager ID="ctlScriptManager" runat="server">
    </asp:ScriptManager>
    <h1>Player</h1>
    <div id="divOutput" runat="server" style="height: 400px; border: 1px solid silver; overflow: auto;">
        Output<br />
    </div>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:TextBox ID="txtCommand" runat="server" Width="500px"></asp:TextBox>
            <asp:Button ID="cmdSubmit" runat="server" Width="20px" OnClick="cmdSubmit_Click" />
            <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                <ProgressTemplate>
                    Working
                </ProgressTemplate>
            </asp:UpdateProgress>
        </ContentTemplate>
    </asp:UpdatePanel>
    </form>
</body>
</html>
