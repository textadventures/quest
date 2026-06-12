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

    Private Sub lblStatus_GotFocus(sender As Object, e As EventArgs)
        HideCaret(lblStatus.Handle)
    End Sub

    Public Sub Clear()
        lblStatus.Text = ""
    End Sub

    Private Const LogoDesignHeight As Integer = 385

    Protected Overrides Sub OnLayout(levent As System.Windows.Forms.LayoutEventArgs)
        MyBase.OnLayout(levent)
        ' Keep logo height proportional to screen DPI only — not the user's font-size
        ' preference. The guard prevents the resulting re-layout from looping.
        Dim target As Integer = CInt(LogoDesignHeight * DeviceDpi / 96.0F)
        If PictureBox1 IsNot Nothing AndAlso PictureBox1.Height <> target Then
            PictureBox1.Height = target
        End If
    End Sub
End Class
