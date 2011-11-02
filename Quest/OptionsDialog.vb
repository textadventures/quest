Public Class OptionsDialog

    Private fontFamily As String
    Private fontSize As Single
    Private fontStyle As FontStyle

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        fontFamily = Options.Instance.GetStringValue(OptionNames.FontFamily)
        fontSize = Options.Instance.GetSingleValue(OptionNames.FontSize)
        fontStyle = DirectCast(Options.Instance.GetIntValue(OptionNames.FontStyle), FontStyle)
        chkUseDefaultColours.Checked = Options.Instance.GetBooleanValue(OptionNames.UseGameColours)
        cmdForeground.BackColor = Options.Instance.GetColourValue(OptionNames.ForegroundColour)
        cmdBackground.BackColor = Options.Instance.GetColourValue(OptionNames.BackgroundColour)
        cmdLink.BackColor = Options.Instance.GetColourValue(OptionNames.LinkColour)
        chkUseDefaultFont.Checked = Options.Instance.GetBooleanValue(OptionNames.UseGameFont)
        txtGamesFolder.Text = Options.Instance.GetStringValue(OptionNames.GamesFolder)
        chkShowSandpit.Checked = Options.Instance.GetBooleanValue(OptionNames.ShowSandpit)
        chkPlaySounds.Checked = Options.Instance.GetBooleanValue(OptionNames.PlaySounds)
        UpdateSampleText()
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Me.Hide()
    End Sub

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        If Options.Instance.GetStringValue(OptionNames.GamesFolder) <> txtGamesFolder.Text Then
            Dim oldFolder As String = Options.Instance.GetStringValue(OptionNames.GamesFolder)
            Dim newFolder As String = txtGamesFolder.Text
            Dim fileCount As Integer = System.IO.Directory.GetFiles(oldFolder).Count()

            If fileCount > 0 Then
                Dim result As MsgBoxResult = MsgBox(
                    String.Format(
                        "You changed the Download Folder. Would you like to move the existing {0} files?{3}{3}From: {1}{3}To: {2}",
                        fileCount,
                        Options.Instance.GetStringValue(OptionNames.GamesFolder),
                        txtGamesFolder.Text,
                        Environment.NewLine
                    ), MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question)

                If result = MsgBoxResult.Yes Then
                    MoveDownloadFolder(oldFolder, newFolder)
                ElseIf result = MsgBoxResult.Cancel Then
                    Return
                End If
            End If

        End If

        Options.Instance.SetBooleanValue(OptionNames.UseGameColours, chkUseDefaultColours.Checked)
        Options.Instance.SetColourValue(OptionNames.ForegroundColour, cmdForeground.BackColor)
        Options.Instance.SetColourValue(OptionNames.BackgroundColour, cmdBackground.BackColor)
        Options.Instance.SetColourValue(OptionNames.LinkColour, cmdLink.BackColor)
        Options.Instance.SetBooleanValue(OptionNames.UseGameFont, chkUseDefaultFont.Checked)
        Options.Instance.SetStringValue(OptionNames.FontFamily, fontFamily)
        Options.Instance.SetSingleValue(OptionNames.FontSize, fontSize)
        Options.Instance.SetIntValue(OptionNames.FontStyle, fontStyle)
        Options.Instance.SetStringValue(OptionNames.GamesFolder, txtGamesFolder.Text)
        Options.Instance.SetBooleanValue(OptionNames.ShowSandpit, chkShowSandpit.Checked)
        Options.Instance.SetBooleanValue(OptionNames.PlaySounds, chkPlaySounds.Checked)
        Me.Hide()
    End Sub

    Private Sub MoveDownloadFolder(oldFolder As String, newFolder As String)
        Try
            For Each file As String In System.IO.Directory.GetFiles(oldFolder)
                Dim fileName As String = System.IO.Path.GetFileName(file)
                Dim newFileName As String = System.IO.Path.Combine(newFolder, fileName)
                System.IO.File.Move(file, newFileName)
            Next
        Catch ex As Exception
            MsgBox("Failed to move files: " + ex.Message, MsgBoxStyle.Critical)
        End Try
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
        UpdateSampleText()
    End Sub

    Private Sub chkUseDefaultColours_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkUseDefaultColours.CheckedChanged
        UpdateColoursEnabledState()
        UpdateSampleText()
    End Sub

    Private Sub UpdateColoursEnabledState()
        cmdBackground.Enabled = Not chkUseDefaultColours.Checked
        cmdForeground.Enabled = Not chkUseDefaultColours.Checked
        cmdLink.Enabled = Not chkUseDefaultColours.Checked
        lblBackground.Enabled = Not chkUseDefaultColours.Checked
        lblForeground.Enabled = Not chkUseDefaultColours.Checked
        lblLink.Enabled = Not chkUseDefaultColours.Checked
    End Sub

    Private Sub chkUseDefaultFont_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkUseDefaultFont.CheckedChanged
        cmdFont.Enabled = Not chkUseDefaultFont.Checked
        UpdateSampleText()
    End Sub

    Private Sub UpdateSampleText()
        If chkUseDefaultColours.Checked Then
            lblFontSample.BackColor = Color.White
            lblFontSample.ForeColor = Color.Black
        Else
            lblFontSample.BackColor = cmdBackground.BackColor
            lblFontSample.ForeColor = cmdForeground.BackColor
        End If

        If chkUseDefaultFont.Checked Then
            lblFontSample.Font = New Font("Arial", 9, FontStyle.Regular)
        Else
            lblFontSample.Font = New Font(fontFamily, fontSize, fontStyle)
        End If
    End Sub

    Private Sub cmdFont_Click(sender As System.Object, e As System.EventArgs) Handles cmdFont.Click
        dlgFont.Font = New Font(fontFamily, fontSize, fontStyle)
        dlgFont.ShowDialog()
        fontFamily = dlgFont.Font.FontFamily.Name
        fontSize = dlgFont.Font.Size
        fontStyle = dlgFont.Font.Style
        UpdateSampleText()
    End Sub

    Private Sub cmdGamesFolder_Click(sender As System.Object, e As System.EventArgs) Handles cmdGamesFolder.Click
        dlgFolderBrowser.SelectedPath = txtGamesFolder.Text
        If dlgFolderBrowser.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtGamesFolder.Text = dlgFolderBrowser.SelectedPath
        End If
    End Sub
End Class