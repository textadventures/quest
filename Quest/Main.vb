Imports AxeSoftware.Utility

Public Class Main

    Private m_currentFile As String
    Private m_loaded As Boolean = False
    Private Const k_regPath As String = "Software\Quest"
    Private Delegate Sub MenuHandler()

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlPlayer.Visible = False
        InitialiseMenuHandlers()

        Dim helper As New AxeSoftware.Utility.WindowHelper(Me, "Quest", "Main")
    End Sub

    Private Sub InitialiseMenuHandlers()
        ctlMenu.AddMenuClickHandler("open", AddressOf Browse)
        ctlMenu.AddMenuClickHandler("about", AddressOf AboutMenuClick)
        ctlMenu.AddMenuClickHandler("exit", AddressOf ExitMenuClick)
        ctlMenu.AddMenuClickHandler("openedit", AddressOf OpenEditMenuClick)
        ctlMenu.AddMenuClickHandler("restart", AddressOf RestartMenuClick)
    End Sub

    Private Sub ctlPlayer_AddToRecent(ByVal filename As String, ByVal name As String) Handles ctlPlayer.AddToRecent
        ctlLauncher.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayer_Quit() Handles ctlPlayer.Quit
        Me.Close()
    End Sub

    Private Sub ctlLauncher_BrowseForGame() Handles ctlLauncher.BrowseForGame
        Browse()
    End Sub

    Private Sub ctlLauncher_EditGame(ByVal filename As String) Handles ctlLauncher.EditGame
        LaunchEdit(filename)
    End Sub

    Private Sub ctlLauncher_LaunchGame(ByVal filename As String) Handles ctlLauncher.LaunchGame
        Launch(filename)
    End Sub

    Private Sub Browse()
        Dim startFolder As String = DirectCast(Registry.GetSetting("Quest", "Settings", "StartFolder", _
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), String)

        dlgOpenFile.InitialDirectory = startFolder
        dlgOpenFile.Multiselect = False
        dlgOpenFile.Filter = "Quest Games (*.aslx, *.asl, *.cas, *.qsg)|*.aslx;*.asl;*.cas;*.qsg|All files|*.*"
        dlgOpenFile.FileName = ""
        dlgOpenFile.ShowDialog()
        If dlgOpenFile.FileName.Length > 0 Then
            Registry.SaveSetting("Quest", "Settings", "StartFolder", System.IO.Path.GetDirectoryName(dlgOpenFile.FileName))

            Launch(dlgOpenFile.FileName)
        End If
    End Sub

    Private Sub Launch(ByRef filename As String)
        Dim game As AxeSoftware.Quest.IASL = Nothing

        Try
            m_currentFile = filename
            ctlPlayer.Reset()

            game = GameLauncher.GetGame(filename, String.Empty)

            If game Is Nothing Then
                MsgBox("Unrecognised file type. This game cannot be loaded in Quest.", MsgBoxStyle.Critical)
            Else
                Me.SuspendLayout()
                ctlMenu.Mode = Quest.Controls.Menu.MenuMode.Player
                ctlLauncher.Visible = False
                ctlEditor.Visible = False
                ctlPlayer.Visible = True
                ctlPlayer.SetMenu(ctlMenu)
                Me.ResumeLayout()
                ctlPlayer.RestoreSplitterPositions()
                Application.DoEvents()
                ctlPlayer.Initialise(game)
                ctlPlayer.Focus()
            End If

        Catch ex As Exception
            MsgBox("Error launching game: " & ex.Message)
        End Try

    End Sub

    Private Sub LaunchEdit(ByRef filename As String)
        Dim game As AxeSoftware.Quest.IASL = Nothing
        Dim ext As String

        Try
            ext = System.IO.Path.GetExtension(filename)

            Select Case ext
                Case ".aslx"
                    Me.SuspendLayout()
                    ctlMenu.Mode = Quest.Controls.Menu.MenuMode.Editor
                    ctlLauncher.Visible = False
                    ctlPlayer.Visible = False
                    ctlEditor.Visible = True
                    ctlEditor.SetMenu(ctlMenu)
                    Me.ResumeLayout()
                    'ctlPlayer.RestoreSplitterPositions()
                    Application.DoEvents()
                    ctlEditor.Initialise(filename)
                    ctlEditor.Focus()
                Case Else
                    MsgBox(String.Format("Unrecognised file type '{0}'", ext))
            End Select

        Catch ex As Exception
            MsgBox("Error launching game: " & ex.Message)
        End Try

    End Sub

    Private Sub AboutMenuClick()
        Dim frmAbout As New About
        frmAbout.ShowDialog()
    End Sub

    Private Sub ctlPlayer_GameNameSet(ByVal name As String) Handles ctlPlayer.GameNameSet
        Dim caption As String
        caption = "Quest"
        If Not String.IsNullOrEmpty(name) Then caption += " - " + name
        Me.Text = caption
    End Sub

    Private Sub ExitMenuClick()
        Me.Close()
    End Sub

    Private Sub RestartMenuClick()
        Launch(m_currentFile)
    End Sub

    Private Sub OpenEditMenuClick()
        BrowseEdit()
    End Sub

    Private Sub BrowseEdit()
        Dim startFolder As String = DirectCast(Registry.GetSetting("Quest", "Settings", "StartFolder", _
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)), String)

        dlgOpenFile.InitialDirectory = startFolder
        dlgOpenFile.Multiselect = False
        dlgOpenFile.Filter = "Quest Games (*.aslx)|*.aslx|All files|*.*"
        dlgOpenFile.FileName = ""
        dlgOpenFile.ShowDialog()
        If dlgOpenFile.FileName.Length > 0 Then
            Registry.SaveSetting("Quest", "Settings", "StartFolder", System.IO.Path.GetDirectoryName(dlgOpenFile.FileName))

            LaunchEdit(dlgOpenFile.FileName)
        End If

    End Sub

    Private Sub ctlEditor_AddToRecent(ByVal filename As String, ByVal name As String) Handles ctlEditor.AddToRecent
        ctlLauncher.AddToEditorRecent(filename, name)
    End Sub

    Private Sub Main_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        ctlPlayer.WindowClosing()
    End Sub

    Private Sub Main_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        e.Handled = ctlPlayer.KeyPressed()
    End Sub
End Class
