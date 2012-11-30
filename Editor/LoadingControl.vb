Imports System.Runtime.InteropServices

Public Class LoadingControl

    <DllImport("user32.dll")> _
    Private Shared Function HideCaret(hWnd As IntPtr) As Boolean
    End Function

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lblStatus.Text = ""

    End Sub

    Public Sub UpdateStatus(status As String)
        lblStatus.SelectionStart = lblStatus.Text.Length
        If (lblStatus.Text.Length > 0) Then
            lblStatus.SelectedText = Environment.NewLine
        End If
        lblStatus.SelectionStart = lblStatus.Text.Length
        lblStatus.SelectedText = status
        lblStatus.SelectionStart = lblStatus.Text.Length
        lblStatus.ScrollToCaret()
    End Sub

    Private Sub lblStatus_GotFocus(sender As Object, e As EventArgs) Handles lblStatus.GotFocus
        HideCaret(lblStatus.Handle)
    End Sub

    Public Sub Clear()
        lblStatus.Text = ""
    End Sub
End Class
