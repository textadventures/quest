Imports Microsoft.Win32

Public Class NewGameWindow

    Private Const k_regPath As String = "Software\Quest\Settings"
    Private Const k_regName As String = "LastTemplate"

    Public Cancelled As Boolean
    Private m_templates As Dictionary(Of String, TemplateData)
    Private m_manualFilename As Boolean = False

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetFilename()

        AddHandler picTextAdventure.Click, AddressOf SelectTextAdventure
        AddHandler picTextAdventureBorder.Click, AddressOf SelectTextAdventure
        AddHandler lstTemplate.SelectedIndexChanged, AddressOf SelectTextAdventure
        AddHandler picGamebook.Click, AddressOf SelectGamebook
        AddHandler picGamebookBorder.Click, AddressOf SelectGamebook
    End Sub

    Private Sub SelectTextAdventure(sender As Object, e As EventArgs)
        optTextAdventure.Checked = True
    End Sub

    Private Sub SelectGamebook(sender As Object, e As EventArgs)
        optGamebook.Checked = True
    End Sub

    Private Sub SetFilename()
        If m_manualFilename Then Return
        Dim myDocsFolder As String = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim gamesFolder As String = System.IO.Path.Combine(myDocsFolder, "Quest Games")
        Dim filename As String = System.IO.Path.Combine(gamesFolder, GenerateFilename(txtGameName.Text))
        txtFilename.Text = filename
    End Sub

    Private Function GenerateFilename(gameName As String) As String
        Dim result As String = EditorController.GenerateSafeFilename(gameName)
        If result.Length = 0 Then Return String.Empty
        Return System.IO.Path.Combine(result, result + ".aslx")
    End Function

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        If System.IO.File.Exists(txtFilename.Text) Then
            Dim result = MsgBox(String.Format("The file {0} already exists.{1}Would you like to overwrite it?", txtFilename.Text, Environment.NewLine + Environment.NewLine),
                                MsgBoxStyle.Exclamation Or MsgBoxStyle.YesNoCancel)
            If result = MsgBoxResult.Yes Then
                ' do nothing, file will be overwritten
            ElseIf result = MsgBoxResult.No Then
                ' browse for new file, and if user cancels Browse dialog then return
                If Not BrowseForFile() Then Return
            Else
                Return
            End If
        End If
        Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(k_regPath)
        key.SetValue(k_regName, lstTemplate.Text)
        Me.Hide()
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Cancelled = True
        Me.Hide()
    End Sub

    Public Sub SetAvailableTemplates(templates As Dictionary(Of String, TemplateData))
        m_templates = templates

        For Each item In From template In m_templates.Values Where template.Type = EditorStyle.TextAdventure
            lstTemplate.Items.Add(item.TemplateName)
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
        BrowseForFile()
    End Sub

    Private Function BrowseForFile() As Boolean
        ctlSaveDialog.FileName = txtFilename.Text
        If ctlSaveDialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtFilename.Text = ctlSaveDialog.FileName
            m_manualFilename = True
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub NewGameWindow_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            Cancelled = True
        End If
    End Sub

    Public Function GetSelectedTemplate() As TemplateData
        If optTextAdventure.Checked Then
            Return m_templates(lstTemplate.Text)
        Else
            Return m_templates.Values.First(Function(template) template.Type = EditorStyle.GameBook)
        End If
    End Function

    Private Sub NewGameWindow_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        txtGameName.Focus()
    End Sub
End Class