Imports System.Xml
Imports System.Threading

Public Class Player

    Implements IPlayer

    Private m_htmlHelper As PlayerHelper
    Private m_panesVisible As Boolean
    Private WithEvents m_game As IASL
    Private WithEvents m_gameTimer As IASLTimer
    Private m_initialised As Boolean
    Private m_browserReady As Boolean
    Private m_gameReady As Boolean
    Private m_history As New List(Of String)
    Private m_historyPoint As Integer
    Private m_gameName As String
    Private WithEvents m_debugger As Debugger
    Private m_stage As Integer
    Private m_loaded As Boolean = False
    Private m_splitHelpers As New List(Of AxeSoftware.Utility.SplitterHelper)
    Private m_menu As AxeSoftware.Quest.Controls.Menu = Nothing
    Private m_saveFile As String
    Private m_waiting As Boolean = False
    Private m_speech As New System.Speech.Synthesis.SpeechSynthesizer
    Private m_loopSound As Boolean = False
    Private m_soundPlaying As Boolean = False
    Private m_destroyed As Boolean = False
    Private WithEvents m_mediaPlayer As New System.Windows.Media.MediaPlayer

    Public Event Quit()
    Public Event AddToRecent(ByVal filename As String, ByVal name As String)
    Public Event GameNameSet(ByVal name As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        PanesVisible = True
        Reset()

        lstPlacesObjects.IgnoreNames = New List(Of String)(ctlCompass.CompassDirections)

        m_splitHelpers.Add(New AxeSoftware.Utility.SplitterHelper(splitMain, "Quest", "MainSplitter"))
        m_splitHelpers.Add(New AxeSoftware.Utility.SplitterHelper(splitPane, "Quest", "PaneSplitter"))

    End Sub

    Public Sub SetMenu(ByVal menu As AxeSoftware.Quest.Controls.Menu)
        m_menu = menu

        menu.AddMenuClickHandler("debugger", AddressOf DebuggerMenuClick)
        menu.AddMenuClickHandler("walkthrough", AddressOf RunWalkthrough)
        menu.AddMenuClickHandler("undo", AddressOf Undo)
        menu.AddMenuClickHandler("selectall", AddressOf SelectAll)
        menu.AddMenuClickHandler("copy", AddressOf Copy)
        menu.AddMenuClickHandler("save", AddressOf Save)
        menu.AddMenuClickHandler("saveas", AddressOf SaveAs)
    End Sub

    Private Sub DebuggerMenuClick()
        ShowDebugger(Not m_menu.MenuChecked("debugger"))
    End Sub

    Public Sub Initialise(ByRef game As IASL)
        Try
            m_menu.MenuEnabled("debugger") = TypeOf game Is IASLDebug
            m_browserReady = False
            m_stage = 0
            ctlPlayerHtml.Setup()
            m_game = game
            m_gameTimer = TryCast(m_game, IASLTimer)
            m_gameReady = True
            txtCommand.Text = ""
            SetEnabledState(True)
            ' we don't finish initialising the game until the webbrowser's DocumentCompleted fires
            ' - it's not in a ready state before then.
            TryInitialise()
        Catch ex As Exception
            WriteLine("Failed to launch the game: " & ex.Message)
            m_game.Finish()
            m_game = Nothing
            m_gameReady = False
            GameFinished()
        End Try
    End Sub

    Public Sub Reset()
        If Not m_game Is Nothing Then m_game.Finish()
        m_initialised = False
        m_gameReady = False
        m_gameName = ""
        lstInventory.Clear()
        lstPlacesObjects.Clear()
    End Sub

    Public Property PanesVisible() As Boolean
        Get
            Return m_panesVisible
        End Get
        Set(ByVal value As Boolean)
            m_panesVisible = value
            splitMain.Panel2Collapsed = Not m_panesVisible
            If m_panesVisible Then
                cmdPanes.Text = ">"
            Else
                cmdPanes.Text = "<"
            End If
        End Set
    End Property

    Private Sub txtCommand_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCommand.KeyDown
        Select Case e.KeyCode
            Case Keys.Up
                SetHistoryPoint(-1)
                e.Handled = True
            Case Keys.Down
                SetHistoryPoint(1)
                e.Handled = True
            Case Keys.Escape
                txtCommand.Text = ""
                SetHistoryPoint(0)
                e.SuppressKeyPress = True
                e.Handled = True
            Case Keys.Enter
                e.SuppressKeyPress = True
                e.Handled = True
                ' Use a timer to trigger the call to EnterText, otherwise if we handle an event
                ' which shows the Menu form, we get a "ding" as this sub hasn't returned yet.
                tmrTimer.Tag = Nothing
                tmrTimer.Enabled = True
        End Select
    End Sub

    Private Sub SetHistoryPoint(ByVal offset As Integer)
        If offset = 0 Then
            m_historyPoint = m_history.Count
        Else
            If m_history.Count > 0 Then
                m_historyPoint = m_historyPoint + offset

                If m_historyPoint < 0 Then m_historyPoint = m_history.Count - 1
                If m_historyPoint >= m_history.Count Then m_historyPoint = 0

                txtCommand.Text = m_history.Item(m_historyPoint)
                txtCommand.SelectionStart = txtCommand.Text.Length
            End If
        End If
    End Sub

    Private Sub cmdGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdGo.Click
        If m_waiting Then
            m_waiting = False
            Exit Sub
        End If
        EnterText()
    End Sub

    Private Sub cmdPanes_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPanes.Click
        PanesVisible = Not PanesVisible
    End Sub

    Private Sub EnterText()
        If txtCommand.Text.Length > 0 Then
            m_history.Add(txtCommand.Text)
            SetHistoryPoint(0)
            RunCommand(txtCommand.Text)
        End If
    End Sub

    Private Sub RunCommand(ByVal command As String)

        If Not m_initialised Then Exit Sub

        txtCommand.Text = ""

        Try
            m_game.SendCommand(command)
        Catch ex As Exception
            WriteLine(String.Format("Error running command '{0}':<br>{1}", command, ex.Message))
        End Try

        txtCommand.Focus()

    End Sub

    Private Sub ctlCompass_RunCommand(ByVal command As String) Handles ctlCompass.RunCommand
        RunCommand(command)
    End Sub

    Private Delegate Sub FinishedDelegate()

    Private Sub m_game_Finished() Handles m_game.Finished
        BeginInvoke(New FinishedDelegate(AddressOf GameFinished))
    End Sub

    Private Sub GameFinished()
        m_initialised = False
        tmrTick.Enabled = False
        SetEnabledState(False)
        StopSound()
    End Sub

    Private Delegate Sub PrintDelegate(text As String)

    Private Sub m_game_LogError(errorMessage As String) Handles m_game.LogError
        BeginInvoke(Sub() PrintText("<output>[Sorry, an error occurred]</output>"))
    End Sub

    Private Sub m_game_PrintText(ByVal text As String) Handles m_game.PrintText
        BeginInvoke(New PrintDelegate(AddressOf PrintText), text)
    End Sub

    Private Sub TryInitialise()
        Dim success As Boolean

        If (Not m_initialised) And m_gameReady And m_browserReady Then

            If m_stage = 1 Then

                success = m_game.Initialise(Me)

                If Not success Then
                    WriteLine("<b>Failed to load game.</b>")
                    If (m_game.Errors.Count > 0) Then
                        WriteLine("The following errors occurred:")
                        For Each loadError As String In m_game.Errors
                            WriteLine(loadError)
                        Next
                    End If
                    GameFinished()
                Else
                    AddToRecentList()
                End If

                ShowDebugger(False)
                m_debugger = Nothing

                If success Then
                    TryInitialiseStage2()
                End If

            ElseIf m_stage = 2 Then
                TryInitialiseStage2()
            Else
                Throw New Exception("Undefined startup stage")
            End If
        End If
    End Sub

    Private Sub TryInitialiseStage2()
        ctlPlayerHtml.AddASLEventElement()
        m_menu.MenuEnabled("walkthrough") = Not m_game.Walkthrough Is Nothing
        m_initialised = True
        tmrTick.Enabled = True
        m_game.Begin()
        txtCommand.Focus()
    End Sub

    Private Sub AddToRecentList()
        If Not String.IsNullOrEmpty(m_game.SaveFilename) Then
            RaiseEvent AddToRecent(m_game.SaveFilename, m_gameName + " (Saved)")
            m_saveFile = m_game.SaveFilename
        ElseIf Not m_game.Filename Is Nothing Then
            RaiseEvent AddToRecent(m_game.Filename, m_gameName)
        End If
    End Sub

    Private Sub tmrTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrTimer.Tick
        tmrTimer.Enabled = False
        EnterText()
    End Sub

    Private Delegate Sub RequestRaisedDelegate(ByVal request As Quest.Request, ByVal data As String)

    Private Sub SetGameName(name As String)
        m_gameName = name
        AddToRecentList()
        RaiseEvent GameNameSet(name)
    End Sub

    Private Delegate Sub UpdateListDelegate(ByVal listType As AxeSoftware.Quest.ListType, ByVal items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData))

    Private Sub m_game_UpdateList(ByVal listType As AxeSoftware.Quest.ListType, ByVal items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData)) Handles m_game.UpdateList
        BeginInvoke(New UpdateListDelegate(AddressOf UpdateList), listType, items)
    End Sub

    Private Sub UpdateList(ByVal listType As AxeSoftware.Quest.ListType, ByVal items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData))
        ' Keep the IASL interface atomic, so we transmit lists of places separately to lists of objects.
        ' We can merge them when we receive them here, and then pass the merged list to the ElementList with some
        ' kind of flag so it knows what's a place and what's an object.

        Select Case listType
            Case AxeSoftware.Quest.ListType.ObjectsList
                lstPlacesObjects.Items = items
            Case AxeSoftware.Quest.ListType.InventoryList
                lstInventory.Items = items
            Case AxeSoftware.Quest.ListType.ExitsList
                ctlCompass.SetAvailableExits(items)
        End Select
    End Sub

    Private Sub lstPlacesObjects_SendCommand(ByVal command As String) Handles lstPlacesObjects.SendCommand
        RunCommand(command)
    End Sub

    Private Sub lstInventory_SendCommand(ByVal command As String) Handles lstInventory.SendCommand
        RunCommand(command)
    End Sub

    Public ReadOnly Property IsGameInProgress() As Boolean
        Get
            Return m_initialised
        End Get
    End Property

    Public Sub ShowDebugger(ByVal show As Boolean)
        If show Then
            If m_debugger Is Nothing Then
                m_debugger = New Debugger
                m_debugger.Game = DirectCast(m_game, IASLDebug)
            End If
            m_debugger.Show()
            m_debugger.LoadSplitterPositions()
        Else
            If Not m_debugger Is Nothing Then
                m_debugger.Hide()
            End If
        End If
    End Sub

    Private Sub m_debugger_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles m_debugger.VisibleChanged
        m_menu.MenuChecked("debugger") = m_debugger.Visible
    End Sub

    Public Sub RunWalkthrough()
        ' Eventually we want to pop up a debugging panel on the right of the screen where we can select
        ' walkthroughs, step through etc.

        For Each cmd As String In m_game.Walkthrough.Steps
            RunCommand(cmd)
        Next
    End Sub

    Private Sub InvokeScript(ByVal functionName As String, ByVal ParamArray args() As String)
        ctlPlayerHtml.InvokeScript(functionName, args)
    End Sub

    Private Sub SetBackground(ByVal colour As String) Implements IPlayer.SetBackground
        BeginInvoke(Sub()
                        InvokeScript("SetBackground", colour)
                    End Sub)
    End Sub

    Private Sub RunScript(ByVal data As String) Implements IPlayer.RunScript
        BeginInvoke(Sub()
                        Dim functionName As String = ""
                        Dim dataList As List(Of String)
                        Dim args As New List(Of String)

                        dataList = New List(Of String)(data.Split(CChar(";")))

                        For Each value As String In dataList
                            If functionName.Length = 0 Then
                                functionName = value
                            Else
                                args.Add(value.Trim())
                            End If
                        Next

                        InvokeScript(functionName, args.ToArray())
                    End Sub)
    End Sub

    Private Sub Undo()
        RunCommand("undo")
    End Sub

    Private Sub Copy()
        ctlPlayerHtml.Copy()
    End Sub

    Private Sub SelectAll()
        ctlPlayerHtml.SelectAll()
    End Sub

    Private Sub SetEnabledState(ByVal enabled As Boolean)
        txtCommand.Enabled = enabled
        ctlCompass.Enabled = enabled
        cmdGo.Enabled = enabled
        lstInventory.Enabled = enabled
        lstPlacesObjects.Enabled = enabled
    End Sub

    Public Sub RestoreSplitterPositions()
        For Each splitHelper As AxeSoftware.Utility.SplitterHelper In m_splitHelpers
            splitHelper.LoadSplitterPositions()
        Next
    End Sub

    Private Sub Save()
        If Len(m_saveFile) = 0 Then
            SaveAs()
        Else
            Save(m_saveFile)
        End If
    End Sub

    Private Sub SaveAs()
        ctlSaveFile.DefaultExt = m_game.SaveExtension
        ctlSaveFile.Filter = "Saved Games|*." + m_game.SaveExtension + "|All files|*.*"
        ctlSaveFile.FileName = m_saveFile
        If ctlSaveFile.ShowDialog() = DialogResult.OK Then
            m_saveFile = ctlSaveFile.FileName
            Save(m_saveFile)
        End If
    End Sub

    Private Sub Save(ByVal filename As String)
        Try
            m_game.Save(filename)
            RaiseEvent AddToRecent(filename, m_gameName + " (Saved)")
            WriteLine("")
            WriteLine("Saved: " + filename)
        Catch ex As Exception
            MsgBox("Unable to save the file due to the following error:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Sub ShowMenu(ByVal menuData As MenuData) Implements IPlayer.ShowMenu
        BeginInvoke(Sub()
                        Dim menuForm As Menu
                        menuForm = New Menu()

                        menuForm.Caption = menuData.Caption
                        menuForm.Options = menuData.Options
                        menuForm.AllowCancel = menuData.AllowCancel
                        menuForm.ShowDialog()

                        Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf SetMenuResponseInNewThread))
                        runnerThread.Start(menuForm.SelectedItem)
                    End Sub
        )
    End Sub

    Private Sub SetMenuResponseInNewThread(response As Object)
        m_game.SetMenuResponse(DirectCast(response, String))
    End Sub

    ' TO DO: everything implementing an IPlayer interface will need BeginInvoke

    Public Sub DoWait() Implements IPlayer.DoWait
        BeginInvoke(New Action(AddressOf BeginWait))
    End Sub

    Private Sub BeginWait()
        m_waiting = True
        Do
            Threading.Thread.Sleep(100)
            Application.DoEvents()
        Loop Until Not m_waiting Or Not m_initialised
        m_game.FinishWait()
    End Sub

    Public Sub ShowQuestion(ByVal caption As String) Implements IPlayer.ShowQuestion
        BeginInvoke(Sub()
                        Dim result As Boolean = (MsgBox(caption, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, m_gameName) = MsgBoxResult.Yes)
                        m_game.SetQuestionResponse(result)
                    End Sub
        )
    End Sub

    Public Function KeyPressed() As Boolean
        If m_waiting Then
            m_waiting = False
            Return True
        End If

        Return False
    End Function

    Private Sub wbOutput_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.PreviewKeyDownEventArgs)
        KeyPressed()
    End Sub

    Private Sub SetPanesVisible(ByVal data As String) Implements IPlayer.SetPanesVisible
        BeginInvoke(Sub()
                        Select Case data
                            Case "on"
                                PanesVisible = True
                                cmdPanes.Visible = True
                            Case "off"
                                PanesVisible = False
                                cmdPanes.Visible = True
                            Case "disabled"
                                PanesVisible = False
                                cmdPanes.Visible = False
                        End Select

                        If cmdPanes.Visible Then
                            lblBanner.Width = cmdPanes.Left - 1
                        Else
                            lblBanner.Width = ctlPlayerHtml.Width
                        End If
                    End Sub)
    End Sub

    Private Sub ShowPicture(ByVal filename As String) Implements IPlayer.ShowPicture
        BeginInvoke(Sub()
                        ctlPlayerHtml.ShowPicture(filename)
                    End Sub)
    End Sub

    Private Sub PlaySound(ByVal filename As String, ByVal synchronous As Boolean, ByVal looped As Boolean) Implements IPlayer.PlaySound
        BeginInvoke(Sub()
                        If synchronous And looped Then
                            Throw New Exception("Can't play sound that is both synchronous and looped")
                        End If
                        m_loopSound = looped
                        m_soundPlaying = True

                        m_mediaPlayer.Open(New System.Uri(filename))
                        m_mediaPlayer.Play()

                        If synchronous Then
                            Do
                                Threading.Thread.Sleep(10)
                                Application.DoEvents()
                            Loop Until Not m_soundPlaying
                            m_game.FinishWait()
                        End If
                    End Sub
        )
    End Sub

    Private Sub StopSound() Implements IPlayer.StopSound
        If m_destroyed Then Exit Sub
        BeginInvoke(Sub()
                        m_loopSound = False
                        m_mediaPlayer.Stop()
                    End Sub
        )
    End Sub

    Private Sub m_mediaPlayer_MediaEnded(sender As Object, e As System.EventArgs) Handles m_mediaPlayer.MediaEnded
        m_soundPlaying = False
        If m_loopSound Then
            m_mediaPlayer.Position = New TimeSpan(0)
            m_mediaPlayer.Play()
        End If
    End Sub

    Private Sub Speak(ByVal text As String) Implements IPlayer.Speak
        BeginInvoke(Sub()
                        m_speech.Speak(text)
                    End Sub)
    End Sub

    Public Sub SetWindowMenu(ByVal menuData As MenuData) Implements IPlayer.SetWindowMenu
        BeginInvoke(Sub()
                        m_menu.CreateWindowMenu(menuData.Caption, menuData.Options, AddressOf WindowMenuClicked)
                    End Sub
        )
    End Sub

    Private Sub WindowMenuClicked(ByVal command As String)
        RunCommand(command)
    End Sub

    Public Function GetNewGameFile(ByVal originalFilename As String, ByVal extensions As String) As String Implements IPlayer.GetNewGameFile
        If MsgBox(String.Format("The game file {0} does not exist.{1}Would you like to find the file yourself?", originalFilename, vbCrLf + vbCrLf), MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            ctlOpenFile.Filter = "Game files|" + extensions
            ctlOpenFile.ShowDialog()
            Return ctlOpenFile.FileName
        Else
            Return ""
        End If
    End Function

    Public Sub WindowClosing()
        If Not m_game Is Nothing Then
            m_game.Finish()
        End If
    End Sub

    Private Sub tmrTick_Tick(sender As System.Object, e As System.EventArgs) Handles tmrTick.Tick
        If Not m_gameTimer Is Nothing Then
            m_gameTimer.Tick()
        End If
    End Sub

    Private Sub Player_HandleDestroyed(sender As Object, e As System.EventArgs) Handles Me.HandleDestroyed
        m_destroyed = True
    End Sub

    Public Sub WriteHTML(html As String) Implements IPlayer.WriteHTML
        Throw New NotImplementedException()
    End Sub

    Public Sub LocationUpdated(location As String) Implements IPlayer.LocationUpdated
        BeginInvoke(Sub()
                        lblBanner.Text = location
                    End Sub)
    End Sub

    Public Sub UpdateGameName(name As String) Implements IPlayer.UpdateGameName
        BeginInvoke(Sub()
                        SetGameName(name)
                    End Sub)
    End Sub

    Public Sub ClearScreen() Implements IPlayer.ClearScreen
        BeginInvoke(Sub()
                        ctlPlayerHtml.Clear()
                    End Sub)
    End Sub

    Public Sub SetStatusText(text As String) Implements IPlayer.SetStatusText
        BeginInvoke(Sub()
                        lstInventory.Status = text
                    End Sub)
    End Sub

    Public Sub DoQuit() Implements IPlayer.Quit
        BeginInvoke(Sub()
                        RaiseEvent Quit()
                    End Sub)
    End Sub

    Public Sub SetForeground(colour As String) Implements IPlayer.SetForeground
        ctlPlayerHtml.Foreground = colour
    End Sub

    Public Sub SetFont(name As String) Implements IPlayer.SetFont
        ctlPlayerHtml.FontName = name
    End Sub

    Public Sub SetFontSize(size As String) Implements IPlayer.SetFontSize
        ctlPlayerHtml.FontSize = CInt(size)
    End Sub

    Public Sub RequestLoad() Implements IPlayer.RequestLoad
        ' TO DO: Raise event
    End Sub

    Public Sub RequestRestart() Implements IPlayer.RequestRestart
        ' TO DO: Raise event
    End Sub

    Public Sub RequestSave() Implements IPlayer.RequestSave
        BeginInvoke(Sub()
                        Save()
                    End Sub)
    End Sub

    Public Sub SetLinkForeground(colour As String) Implements IPlayer.SetLinkForeground
        ctlPlayerHtml.LinkForeground = colour
    End Sub

    Private Sub WriteLine(text As String)
        ctlPlayerHtml.WriteLine(text)
    End Sub

    Private Sub PrintText(text As String)
        ctlPlayerHtml.PrintText(text)
    End Sub

    Private Sub ctlPlayerHtml_CommandLinkClicked(command As String) Handles ctlPlayerHtml.CommandLinkClicked
        RunCommand(command)
    End Sub

    Private Sub ctlPlayerHtml_Ready() Handles ctlPlayerHtml.Ready
        m_browserReady = True
        m_stage = m_stage + 1

        ' We need this DocumentCompleted event to have finished before trying to output text to the webbrowser control.
        'Dim runnerThread As New Thread(New ThreadStart(AddressOf TryInitialise))
        'runnerThread.Start()

        BeginInvoke(Sub()
                        TryInitialise()
                    End Sub)
    End Sub

    Private Sub ctlPlayerHtml_SendEvent(eventName As String, param As String) Handles ctlPlayerHtml.SendEvent
        m_game.SendEvent(eventName, param)
    End Sub
End Class
