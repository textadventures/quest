Public Class OptionsDialog

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Me.Hide()
    End Sub

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        Me.Hide()
    End Sub

    Private Sub cmdForeground_Click(sender As System.Object, e As System.EventArgs) Handles cmdForeground.Click
        EditColour(cmdForeground)
    End Sub

    Private Sub cmdBackground_Click(sender As System.Object, e As System.EventArgs) Handles cmdBackground.Click
        EditColour(cmdBackground)
    End Sub

    Private Sub cmdLink_Click(sender As System.Object, e As System.EventArgs) Handles cmdLink.Click
        EditColour(cmdLink)
    End Sub

    Private Sub EditColour(button As Button)
        dlgColor.Color = button.BackColor
        dlgColor.ShowDialog()
        button.BackColor = dlgColor.Color
    End Sub

    Private Sub chkOverrideColours_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkOverrideColours.CheckedChanged
        UpdateColoursEnabledState()
    End Sub

    Private Sub UpdateColoursEnabledState()
        cmdBackground.Enabled = Not chkOverrideColours.Checked
        cmdForeground.Enabled = Not chkOverrideColours.Checked
        cmdLink.Enabled = Not chkOverrideColours.Checked
        lblBackground.Enabled = Not chkOverrideColours.Checked
        lblForeground.Enabled = Not chkOverrideColours.Checked
        lblLink.Enabled = Not chkOverrideColours.Checked
    End Sub

End Class