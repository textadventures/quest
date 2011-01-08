<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="WebPlayer.Play" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery/jquery-1.4.4.min.js"></script>
    <script type="text/javascript">
        function ClearInput() {
            //$("#txtCommand").val("");
        }
    </script>
    <title>Player</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ctlScriptManager" runat="server">
    </asp:ScriptManager>
    <h1>
        Player</h1>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div id="divOutput" runat="server" style="height: 400px; border: 1px solid silver; overflow: auto;">
                Output<br />
            </div>
            <asp:TextBox ID="txtCommand" runat="server" Width="500px"></asp:TextBox>
            <asp:Button ID="cmdSubmit" runat="server" Width="20px" OnClick="cmdSubmit_Click"
                OnClientClick="ClearInput()" />
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
