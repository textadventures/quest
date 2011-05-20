Imports AxeSoftware.Utility

Public Class Main

    Private m_currentFile As String
    Private m_loaded As Boolean = False
    Private m_playingEditorGame As Boolean = False
    Private Const k_regPath As String = "Software\Quest"
    Private Delegate Sub MenuHandler()

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlLauncher.QuestVersion = My.Application.Info.Version
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
        ctlMenu.AddMenuClickHandler("createnew", AddressOf CreateNewMenuClick)
    End Sub

    Private Sub ctlPlayer_AddToRecent(filename As String, name As String) Handles ctlPlayer.AddToRecent
        ctlLauncher.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayer_Quit() Handles ctlPlayer.Quit
        If m_playingEditorGame Then
            ctlPlayer.Visible = False
            ctlMenu.Mode = Quest.Controls.Menu.MenuMode.Editor
            ctlEditor.Visible = True
            m_playingEditorGame = False
            ctlEditor.SetWindowTitle()
        Else
            ctlMenu.Mode = Quest.Controls.Menu.MenuMode.GameBrowser
            ctlLauncher.RefreshLists()
            ctlPlayer.Visible = False
            ctlLauncher.Visible = True
            SetWindowTitle()
        End If
    End Sub

    Private Sub ctlLauncher_BrowseForGame() Handles ctlLauncher.BrowseForGame
        Browse()
    End Sub

    Private Sub ctlLauncher_EditGame(filename As String) Handles ctlLauncher.EditGame
        LaunchEdit(filename)
    End Sub

    Private Sub ctlLauncher_LaunchGame(filename As String) Handles ctlLauncher.LaunchGame
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

    Private Sub Launch(filename As String)
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

    Private Sub LaunchEdit(filename As String)
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

    Private Sub ctlPlayer_GameNameSet(name As String) Handles ctlPlayer.GameNameSet
        SetWindowTitle(name)
    End Sub

    Private Sub SetWindowTitle(Optional gameName As String = "")
        Dim caption As String
        caption = "Quest"
        If Not String.IsNullOrEmpty(gameName) Then caption += " - " + gameName
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

    Private Sub CreateNewMenuClick()
        Dim newFile = ctlEditor.CreateNewGame()

        If String.IsNullOrEmpty(newFile) Then Return

        LaunchEdit(newFile)
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

    Private Sub ctlEditor_AddToRecent(filename As String, name As String) Handles ctlEditor.AddToRecent
        ctlLauncher.AddToEditorRecent(filename, name)
    End Sub

    Private Sub Main_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        ctlPlayer.WindowClosing()
        If Not ctlEditor.CloseEditor(False) Then
            e.Cancel = True
        End If
    End Sub

    Private Sub Main_KeyPress(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        e.Handled = ctlPlayer.KeyPressed()
    End Sub

    Private Sub ctlEditor_Close() Handles ctlEditor.Close
        ctlMenu.Mode = Quest.Controls.Menu.MenuMode.GameBrowser
        ctlLauncher.RefreshLists()
        ctlEditor.Visible = False
        ctlLauncher.Visible = True
        SetWindowTitle()
    End Sub

    Private Sub ctlEditor_Loaded(name As String) Handles ctlEditor.Loaded
        SetWindowTitle(name)
    End Sub

    Private Sub ctlEditor_Play(filename As String) Handles ctlEditor.Play
        m_playingEditorGame = True
        Launch(filename)
    End Sub

    Private Sub Main_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        ctlLauncher.MainWindowShown()
    End Sub
End Class
