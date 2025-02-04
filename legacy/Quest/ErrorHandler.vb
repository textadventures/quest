Imports System.Net
Imports System.Collections.Specialized

Public Class ErrorHandler

    Private Shared s_currentDialog As ErrorHandler

    Friend Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Public Shared Sub ShowError(text As String)
        If s_currentDialog Is Nothing Then
            s_currentDialog = New ErrorHandler
            s_currentDialog.ErrorText = text
            s_currentDialog.ShowDialog()
        End If
    End Sub

    Public WriteOnly Property ErrorText() As String
        Set(value As String)
            txtError.Text = value
        End Set
    End Property

    Private Sub cmdClose_Click(sender As System.Object, e As System.EventArgs) Handles cmdClose.Click
        Me.Close()
    End Sub

    'Private Sub cmdReport_Click(sender As System.Object, e As System.EventArgs) Handles cmdReport.Click
    '    Try
    '        Me.Cursor = Cursors.WaitCursor
    '        Using client As New WebClient
    '            Dim values As New NameValueCollection
    '            values.Add("product", My.Application.Info.ProductName)
    '            values.Add("version_label", My.Application.Info.Version.ToString)
    '            values.Add("version_x", My.Application.Info.Version.Major.ToString())
    '            values.Add("version_y", My.Application.Info.Version.Minor.ToString())
    '            values.Add("version_z", My.Application.Info.Version.Build.ToString())
    '            values.Add("version_build", My.Application.Info.Version.Revision.ToString())
    '            values.Add("platform", "Windows")
    '            values.Add("platform_version", System.Environment.OSVersion.VersionString)
    '            values.Add("message", txtError.Text)
    '            Dim result = client.UploadValues("http://www.textadventures.co.uk/reporterror.php", values)
    '            Dim resultString = New System.Text.UTF8Encoding().GetString(result)
    '            If resultString = "OK" Then
    '                MsgBox("Error report sent successfully.", MsgBoxStyle.Information, "Error Report")
    '            End If
    '        End Using
    '    Catch ex As Exception
    '        MsgBox("Error sending report:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical, "Error Report")
    '    Finally
    '        Me.Cursor = Cursors.Default
    '        Me.Close()
    '    End Try
    'End Sub

    Private Sub ErrorHandler_FormClosed(sender As Object, e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        s_currentDialog = Nothing
    End Sub

    Private Sub cmdCopy_Click(sender As Object, e As EventArgs) Handles cmdCopy.Click
        txtError.Copy()
    End Sub

    Private Sub lblIssueTracker_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lblIssueTracker.LinkClicked
        TextAdventures.Utility.Utility.LaunchURL("https://github.com/textadventures/quest/issues")
    End Sub
End Class