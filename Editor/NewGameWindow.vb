Imports Microsoft.Win32

Public Class NewGameWindow

    Private Const k_regPath As String = "Software\Quest\Settings"
    Private Const k_regName As String = "LastTemplate"

    Public Cancelled As Boolean
    Private m_templates As Dictionary(Of String, String)
    Private m_manualFilename As Boolean = False

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetFilename()
    End Sub

    Private Sub SetFilename()
        If m_manualFilename Then Return
        Dim myDocsFolder As String = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim gamesFolder As String = System.IO.Path.Combine(myDocsFolder, "Quest Games")
        Dim filename As String = System.IO.Path.Combine(gamesFolder, GenerateFilename(txtGameName.Text))
        txtFilename.Text = filename
    End Sub

    Private Shared s_invalidChars As New List(Of Char) From
        {"\"c, "/"c, ":"c, "*"c, "?"c, """"c, "<"c, ">"c, "|"c}

    Private Function GenerateFilename(gameName As String) As String
        Dim result = gameName
        For Each invalidChar In s_invalidChars
            result = result.Replace(invalidChar, "")
        Next
        If result.Length = 0 Then Return String.Empty
        Return System.IO.Path.Combine(result, result + ".aslx")
    End Function

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(k_regPath)
        key.SetValue(k_regName, lstTemplate.Text)
        Me.Hide()
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Cancelled = True
        Me.Hide()
    End Sub

    Public Sub SetAvailableTemplates(templates As Dictionary(Of String, String))
        m_templates = templates

        For Each item In m_templates.Keys
            lstTemplate.Items.Add(item)
        Next

        If lstTemplate.Items.Count > 0 Then
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(k_regPath)
            Dim lastTemplate As String = TryCast(key.GetValue(k_regName), String)
            If lastTemplate Is Nothing Then lastTemplate = "English"

            Dim index As Integer = lstTemplate.Items.IndexOf(lastTemplate)
            lstTemplate.SelectedIndex = If(index = -1, 0, index)
        End If
    End Sub

    Private Sub txtGameName_TextChanged(sender As Object, e As System.EventArgs) Handles txtGameName.TextChanged
        SetFilename()
        cmdOK.Enabled = txtGameName.Text.Length > 0
    End Sub

    Private Sub cmdBrowse_Click(sender As System.Object, e As System.EventArgs) Handles cmdBrowse.Click
        ctlSaveDialog.FileName = txtFilename.Text
        If ctlSaveDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtFilename.Text = ctlSaveDialog.FileName
            m_manualFilename = True
        End If
    End Sub

    Private Sub NewGameWindow_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            Cancelled = True
        End If
    End Sub
End Class