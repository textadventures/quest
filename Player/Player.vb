Imports System.Xml
Imports System.Threading
Imports System.Globalization
Imports TextAdventures.Utility.Language.L

Public Class Player

    Implements IPlayerHelperUI

    Private m_htmlHelper As PlayerHelper
    Private WithEvents m_game As IASL
    Private WithEvents m_gameTimer As IASLTimer
    Private m_gameDebug As IASLDebug
    Private m_initialised As Boolean
    Private m_gameName As String
    Private WithEvents m_debugger As Debugger
    Private m_menu As TextAdventures.Quest.Controls.Menu = Nothing
    Private m_saveFile As String
    Private m_waiting As Boolean = False
    Private m_speech As New System.Speech.Synthesis.SpeechSynthesizer
    Private m_loopSound As Boolean = False
    Private m_waitingForSoundToFinish As Boolean = False
    Private m_destroyed As Boolean = False
    Private WithEvents m_mediaPlayer As New System.Windows.Media.MediaPlayer
    Private m_tickCount As Integer
    Private m_sendNextTickEventAfter As Integer
    Private m_pausing As Boolean
    Private WithEvents m_walkthroughRunner As WalkthroughRunner
    Private m_preLaunchAction As Action
    Private m_postLaunchAction As Action
    Private m_recordWalkthrough As String
    Private m_recordedWalkthrough As List(Of String)
    Private m_allowColourChange As Boolean = True
    Private m_allowFontChange As Boolean = True
    Private m_playSounds As Boolean = True
    Private m_useSapi As Boolean = False
    Private m_fromEditor As Boolean = False
    Private WithEvents m_log As Log
    Private m_gameLoadedSuccessfully As Boolean = False
    Private WithEvents ctlToolbar As Toolbar

    Public Event Quit()
    Public Event AddToRecent(filename As String, name As String)
    Public Event GameNameSet(name As String)
    Public Event ShortcutKeyPressed(keys As System.Windows.Forms.Keys)
    Public Event ExitFullScreen()
    Public Event RecordedWalkthrough(name As String, steps As List(Of String))

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlToolbar = New Toolbar
        Me.Controls.Add(ctlToolbar)
        ctlToolbar.Dock = DockStyle.Top
        ctlToolbar.Visible = False

        Reset()

    End Sub

    Public Sub SetMenu(menu As TextAdventures.Quest.Controls.Menu)
        m_menu = menu
        ResetMenu()

        menu.AddMenuClickHandler("debugger", AddressOf DebuggerMenuClick)
        menu.AddMenuClickHandler("log", AddressOf LogMenuClick)
        menu.AddMenuClickHandler("walkthrough", AddressOf RunWalkthrough)
        menu.AddMenuClickHandler("undo", AddressOf Undo)
        menu.AddMenuClickHandler("selectall", AddressOf SelectAll)
        menu.SetShortcut("selectall", Keys.Control Or Keys.A)
        menu.AddMenuClickHandler("copy", AddressOf Copy)
        menu.SetShortcut("copy", Keys.Control Or Keys.C)
        menu.AddMenuClickHandler("save", AddressOf Save)
        menu.AddMenuClickHandler("saveas", AddressOf SaveAs)
        menu.AddMenuClickHandler("stop", AddressOf StopGame)
        menu.AddMenuClickHandler("htmldevtools", AddressOf HTMLDevToolsClick)
    End Sub

    Private Sub ctlToolbar_ButtonClicked(sender As Object, args As Toolbar.ButtonClickedEventArgs) Handles ctlToolbar.ButtonClicked
        Select Case args.Button
            Case "stop"
                StopGame()
            Case "debugger"
                DebuggerMenuClick()
            Case "log"
                LogMenuClick()
            Case "htmldevtools"
                HTMLDevToolsClick()
        End Select
    End Sub

    Private Sub DebuggerMenuClick()
        ShowDebugger(Not m_menu.MenuChecked("debugger"))
    End Sub

    Private Sub LogMenuClick()
        ShowLog(Not m_menu.MenuChecked("log"))
    End Sub

    Private Sub HTMLDevToolsClick()
        ctlPlayerHtml.ShowDevTools()
    End Sub

    Public Sub Initialise(ByRef game As IASL, Optional fromEditor As Boolean = False, Optional editorSimpleMode As Boolean = False)
        m_fromEditor = fromEditor
        ctlToolbar.Visible = fromEditor
        If fromEditor Then
            ctlToolbar.SimpleMode = editorSimpleMode
        End If
        SetStatusText("")
        LocationUpdated("")
        m_menu.ClearWindowMenu()
        m_menu.MenuEnabled("copy") = True
        m_gameBackground = Nothing
        m_gameForeground = Nothing
        m_game = game
        m_gameDebug = TryCast(game, IASLDebug)
        m_gameTimer = TryCast(m_game, IASLTimer)
        ResetInterfaceStrings()
        m_htmlHelper = New PlayerHelper(m_game, Me)
        m_htmlHelper.UseGameColours = UseGameColours
        m_saveFile = Nothing

        Me.Cursor = Cursors.WaitCursor

        m_gameLoadedSuccessfully = m_game.Initialise(Me)

        Me.Cursor = Cursors.Default

        If m_gameLoadedSuccessfully Then
            AddToRecentList()
            m_menu.MenuEnabled("walkthrough") = m_gameDebug IsNot Nothing AndAlso m_gameDebug.Walkthroughs IsNot Nothing AndAlso m_gameDebug.Walkthroughs.Walkthroughs.Count > 0
            m_menu.MenuEnabled("debugger") = m_gameDebug IsNot Nothing AndAlso m_gameDebug.DebugEnabled

            ' Generate the new HTML and wait for Ready event
            ctlPlayerHtml.InitialiseHTMLUI(m_game)
        Else
            ctlPlayerHtml.InitialiseHTMLUI(Nothing)
        End If

    End Sub

    Private Sub BeginGame()
        m_initialised = True
        If m_preLaunchAction IsNot Nothing Then
            m_preLaunchAction.Invoke()
            m_preLaunchAction = Nothing
        End If
        m_game.Begin()
        ClearBuffer()
        ctlPlayerHtml.Focus()
        If m_postLaunchAction IsNot Nothing Then
            m_postLaunchAction.Invoke()
            m_postLaunchAction = Nothing
        End If
    End Sub

    Private Sub ClearBuffer()
        If Not Me.IsHandleCreated Then Return
        If Not m_htmlHelper Is Nothing Then WriteHTML(m_htmlHelper.ClearBuffer())
        BeginInvoke(Sub() ctlPlayerHtml.ClearBuffer())
    End Sub

    Public Sub Reset()
        CancelWalkthrough()
        If Not m_game Is Nothing Then m_game.Finish()
        m_initialised = False
        m_gameName = ""
        ShowDebugger(False)
        ShowLog(False)
        m_debugger = Nothing
        m_log = Nothing
        If Not m_menu Is Nothing Then
            ResetMenu()
        End If
    End Sub

    Private Sub ResetMenu()
        m_menu.MenuEnabled("walkthrough") = False
        m_menu.MenuEnabled("debugger") = False
    End Sub

    Private Sub CancelWalkthrough()
        If Not m_walkthroughRunner Is Nothing Then
            m_walkthroughRunner.Cancel()
            m_walkthroughRunner = Nothing
        End If
    End Sub

    Private Sub RunCommand(command As String, Optional metadata As IDictionary(Of String, String) = Nothing)

        If Not m_initialised Then Exit Sub

        If m_pausing Or m_waitingForSoundToFinish Then Return

        If m_waiting Then
            m_waiting = False
            Return
        End If

        If RecordWalkthrough IsNot Nothing Then
            m_recordedWalkthrough.Add(command)
        End If

        Try
            If m_gameTimer IsNot Nothing Then
                m_gameTimer.SendCommand(command, GetTickCountAndStopTimer(), metadata)
            Else
                m_game.SendCommand(command, metadata)
            End If
            ClearBuffer()
        Catch ex As Exception
            WriteLine(String.Format(T("EditorErrorRunningCommand") + " '{0}':<br>{1}", command, ex.Message))
        End Try

    End Sub

    Private Sub m_game_Finished() Handles m_game.Finished
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub() GameFinished())
    End Sub

    Private Sub GameFinished()
        If Not m_initialised Then Return
        CancelWalkthrough()
        m_initialised = False
        tmrTick.Enabled = False
        StopSound()
        If Me.IsHandleCreated Then
            BeginInvoke(Sub() ctlPlayerHtml.Finished())
        End If
        ClearBuffer()
        If RecordWalkthrough IsNot Nothing Then
            RaiseEvent RecordedWalkthrough(RecordWalkthrough, m_recordedWalkthrough)
        End If
        RecordWalkthrough = Nothing
        m_speech.SpeakAsyncCancelAll()
    End Sub

    Private Sub m_game_LogError(errorMessage As String) Handles m_game.LogError
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub()
                        WriteLine("<output><b>" + T("EditorSorryErrorOccurred") + "</b></output>")
                        WriteLine("<output>" + errorMessage + "</output>")
                        ClearBuffer()
                    End Sub)
    End Sub

    Private Sub AddToRecentList()
        If Not String.IsNullOrEmpty(m_game.SaveFilename) Then
            RaiseEvent AddToRecent(m_game.SaveFilename, m_gameName + " (Saved)")
            m_saveFile = m_game.SaveFilename
        ElseIf Not m_game.OriginalFilename Is Nothing Then
            RaiseEvent AddToRecent(m_game.OriginalFilename, m_gameName)
        ElseIf Not m_game.Filename Is Nothing Then
            RaiseEvent AddToRecent(m_game.Filename, m_gameName)
        End If
    End Sub

    Private Sub SetGameName(name As String)
        m_gameName = name
        AddToRecentList()
        RaiseEvent GameNameSet(name)
    End Sub

    Private Sub m_game_UpdateList(listType As TextAdventures.Quest.ListType, items As System.Collections.Generic.List(Of TextAdventures.Quest.ListData)) Handles m_game.UpdateList
        BeginInvoke(Sub() UpdateList(listType, items))
    End Sub

    Private Sub UpdateList(listType As TextAdventures.Quest.ListType, items As System.Collections.Generic.List(Of TextAdventures.Quest.ListData))
        ' Keep the IASL interface atomic, so we transmit lists of places separately to lists of objects.
        ' We can merge them when we receive them here, and then pass the merged list to the ElementList with some
        ' kind of flag so it knows what's a place and what's an object.

        Select Case listType
            Case TextAdventures.Quest.ListType.ObjectsList
                ctlPlayerHtml.UpdatePlacesObjectsList(items)
            Case TextAdventures.Quest.ListType.InventoryList
                ctlPlayerHtml.UpdateInventoryList(items)
            Case TextAdventures.Quest.ListType.ExitsList
                ctlPlayerHtml.UpdateCompass(items)
        End Select
    End Sub

    Public ReadOnly Property IsGameInProgress() As Boolean
        Get
            Return m_initialised
        End Get
    End Property

    Public Sub ShowDebugger(show As Boolean)
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

    Public Sub ShowLog(show As Boolean)
        If m_log Is Nothing Then
            m_log = New Log
        End If
        If show Then
            m_log.Show()
        Else
            m_log.Hide()
        End If
    End Sub

    Private Sub m_debugger_VisibleChanged(sender As Object, e As System.EventArgs) Handles m_debugger.VisibleChanged
        m_menu.MenuChecked("debugger") = m_debugger.Visible
        ctlToolbar.SetButtonChecked("debugger", m_debugger.Visible)
    End Sub

    Private Sub m_log_VisibleChanged(sender As Object, e As System.EventArgs) Handles m_log.VisibleChanged
        m_menu.MenuChecked("log") = m_log.Visible
        ctlToolbar.SetButtonChecked("log", m_log.Visible)
    End Sub

    Private Sub RunWalkthrough()
        ' Eventually we want to pop up a debugging panel on the right of the screen where we can select
        ' walkthroughs, step through etc.

        Dim walkThrough As String = ChooseWalkthrough()
        If walkThrough Is Nothing Then Return

        RunWalkthrough(walkThrough)
    End Sub

    Public Sub RunWalkthrough(name As String)
        If InitWalkthrough(name) Then
            StartWalkthrough()
        End If
    End Sub

    Public Function InitWalkthrough(name As String) As Boolean
        If m_walkthroughRunner IsNot Nothing Then
            Return False
        End If
        m_walkthroughRunner = New WalkthroughRunner(m_gameDebug, name)
        If m_walkthroughRunner.Steps = 0 Then
            m_walkthroughRunner = Nothing
            Return False
        End If
        Return True
    End Function

    Public Sub StartWalkthrough()
        Dim runnerThread As New Thread(Sub() WalkthroughRunner())
        runnerThread.Start()
    End Sub

    Private Sub WalkthroughRunner()
        If m_walkthroughRunner Is Nothing Then Exit Sub

        BeginInvoke(Sub() ctlPlayerHtml.SetAnimateScroll(False))

        Try
            m_walkthroughRunner.Run()

        Catch ex As Exception
            BeginInvoke(Sub() WriteLine(String.Format(T("EditorErrorWalkthroughHalted") + "<br>{0}", ex.Message)))

        Finally
            m_walkthroughRunner = Nothing
            BeginInvoke(Sub() ctlPlayerHtml.SetAnimateScroll(True))
        End Try
    End Sub

    Private Function ChooseWalkthrough() As String
        If m_gameDebug.Walkthroughs.Walkthroughs.Count = 0 Then
            Return Nothing
        End If

        If m_gameDebug.Walkthroughs.Walkthroughs.Count = 1 Then
            Return m_gameDebug.Walkthroughs.Walkthroughs.First.Key
        End If

        Dim menuForm As New Menu()
        Dim menuOptions As New Dictionary(Of String, String)

        For Each item In m_gameDebug.Walkthroughs.Walkthroughs
            menuOptions.Add(item.Key, item.Key)
        Next

        menuForm.Caption = T("EditorChooseAWalktroughToRun")
        menuForm.Options = menuOptions
        menuForm.AllowCancel = True
        menuForm.ShowDialog()

        Return menuForm.SelectedItem
    End Function

    Private m_gameBackground As String = Nothing

    Private Sub SetBackground(colour As String) Implements IPlayer.SetBackground
        m_gameBackground = colour
        If Not UseGameColours Then Return
        BeginInvoke(Sub() ctlPlayerHtml.SetBackground(colour))
    End Sub

    Private Sub RunScript(functionName As String, parameters As Object()) Implements IPlayer.RunScript
        ClearBuffer()
        BeginInvoke(Sub()
                        ctlPlayerHtml.InvokeScript(functionName, parameters)
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

    Private Sub Save()
        If Len(m_saveFile) = 0 Then
            SaveAs()
        Else
            BeginSave()
        End If
    End Sub

    Private Sub SaveAs()
        ctlSaveFile.DefaultExt = m_game.SaveExtension
        ctlSaveFile.Filter = T("EditorFilterSavedGames") + m_game.SaveExtension + T("EditorFilterAllFiles")
        ctlSaveFile.FileName = m_saveFile
        If ctlSaveFile.ShowDialog() = DialogResult.OK Then

            If Not ctlSaveFile.FileName.EndsWith("." + m_game.SaveExtension) Then
                Dim message As String = T("EditorInvalidSaveFileName") + " '." + m_game.SaveExtension.ToUpper() + "'."
                If m_fromEditor Then
                    message += Environment.NewLine + Environment.NewLine + T("EditorSaveItFromEditorScreen")
                End If
                MsgBox(message, MsgBoxStyle.Exclamation)
                Return
            End If

            m_saveFile = ctlSaveFile.FileName
            BeginSave()
        End If
    End Sub

    Private Sub BeginSave()
        BeginInvoke(Sub()
                        ctlPlayerHtml.InvokeScript("doSave")
                        ctlPlayerHtml.ClearBuffer()
                    End Sub)
    End Sub

    Private Sub FinishSave(html As String)
        Try
            m_game.Save(m_saveFile, html)
            RaiseEvent AddToRecent(m_saveFile, m_gameName + " (Saved)")
            WriteLine("")
            WriteLine("Saved: " + m_saveFile)
            ClearBuffer()
        Catch ex As Exception
            MsgBox(T("EditorUnableSaveFileError") + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Sub ShowMenu(menuData As MenuData) Implements IPlayer.ShowMenu
        If m_walkthroughRunner IsNot Nothing Then
            m_walkthroughRunner.ShowMenu(menuData)
        Else
            BeginInvoke(Sub()
                            ClearBuffer()
                            Dim menuForm As Menu
                            menuForm = New Menu()

                            menuForm.Caption = menuData.Caption
                            menuForm.Options = menuData.Options
                            menuForm.AllowCancel = menuData.AllowCancel
                            menuForm.ShowDialog()

                            If RecordWalkthrough IsNot Nothing Then
                                m_recordedWalkthrough.Add("menu:" + menuForm.SelectedItem)
                            End If

                            Dim runnerThread As New System.Threading.Thread(New System.Threading.ParameterizedThreadStart(AddressOf SetMenuResponseInNewThread))
                            runnerThread.Start(menuForm.SelectedItem)
                        End Sub
            )
        End If
    End Sub

    Private Sub SetMenuResponseInNewThread(response As Object)
        m_game.SetMenuResponse(DirectCast(response, String))
        ClearBuffer()
    End Sub

    Public Sub DoWait() Implements IPlayer.DoWait
        If m_walkthroughRunner IsNot Nothing Then
            m_walkthroughRunner.BeginWait()
        Else
            BeginInvoke(Sub() BeginWait())
        End If
    End Sub

    Private Sub BeginWait()
        m_waiting = True
        ctlPlayerHtml.BeginWait()
        Do
            Threading.Thread.Sleep(100)
            Application.DoEvents()
        Loop Until Not m_waiting Or Not m_initialised Or Not Me.IsHandleCreated
        ctlPlayerHtml.WaitEnded()
        m_game.FinishWait()
        ClearBuffer()
    End Sub

    Public Sub ShowQuestion(caption As String) Implements IPlayer.ShowQuestion
        If m_walkthroughRunner IsNot Nothing Then
            m_walkthroughRunner.ShowQuestion(caption)
        Else
            BeginInvoke(Sub()
                            ClearBuffer()
                            ctlPlayerHtml.Focus()
                            Dim result As Boolean = (MsgBox(caption, MsgBoxStyle.Question Or MsgBoxStyle.YesNo, m_gameName) = MsgBoxResult.Yes)

                            If RecordWalkthrough IsNot Nothing Then
                                m_recordedWalkthrough.Add("answer:" + If(result, "yes", "no"))
                            End If

                            m_game.SetQuestionResponse(result)
                            ClearBuffer()
                        End Sub
            )
        End If
    End Sub

    Public Function KeyPressed() As Boolean
        If m_waiting Then
            m_waiting = False
            Return True
        End If

        Return False
    End Function

    Private Sub SetPanesVisible(data As String) Implements IPlayer.SetPanesVisible
        BeginInvoke(Sub()
                        Select Case data
                            Case "on"
                                ctlPlayerHtml.InvokeScript("panesVisibleEval", "true")
                            Case "off"
                                ctlPlayerHtml.InvokeScript("panesVisibleEval", "false")
                            Case "disabled"
                                ctlPlayerHtml.InvokeScript("panesVisibleEval", "false")
                        End Select
                    End Sub)
    End Sub

    Private Sub ShowPicture(filename As String) Implements IPlayer.ShowPicture
        ClearBuffer()
        BeginInvoke(Sub() ctlPlayerHtml.ShowPicture(filename))
    End Sub

    Private Sub PlaySound(filename As String, synchronous As Boolean, looped As Boolean) Implements IPlayer.PlaySound
        If m_walkthroughRunner IsNot Nothing AndAlso synchronous Then
            synchronous = False
            m_walkthroughRunner.BeginWait()
        End If
        BeginInvoke(Sub()
                        If synchronous And looped Then
                            Throw New Exception(T("EditorCantPlaySound"))
                        End If

                        filename = m_game.GetResourcePath(filename)

                        If System.IO.File.Exists(filename) And PlaySounds Then
                            m_loopSound = looped

                            m_mediaPlayer.Open(New System.Uri(filename))
                            m_mediaPlayer.Play()
                        End If

                        m_waitingForSoundToFinish = synchronous

                        If Not PlaySounds Then
                            PlaybackFinished()
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
        PlaybackFinished()
    End Sub

    Private Sub m_mediaPlayer_MediaFailed(sender As Object, e As System.Windows.Media.ExceptionEventArgs) Handles m_mediaPlayer.MediaFailed
        PlaybackFinished()
    End Sub

    Private Sub PlaybackFinished()
        If m_waitingForSoundToFinish Then
            m_waitingForSoundToFinish = False
            m_game.FinishWait()
            ClearBuffer()
        End If
        If m_loopSound Then
            m_mediaPlayer.Position = New TimeSpan(0)
            m_mediaPlayer.Play()
        End If
    End Sub

    Private Sub Speak(text As String) Implements IPlayer.Speak
        text = StripTags(text)
        BeginInvoke(Sub()
                        If UseSAPI Then
                            m_speech.SpeakAsync(text)
                        Else
                            JawsApi.JawsApi.JFWSayString(text, False)
                        End If
                    End Sub)
    End Sub

    Public Sub SetWindowMenu(menuData As MenuData) Implements IPlayer.SetWindowMenu
        BeginInvoke(Sub()
                        m_menu.CreateWindowMenu(menuData.Caption, menuData.Options, AddressOf WindowMenuClicked)
                    End Sub
        )
    End Sub

    Private Sub WindowMenuClicked(command As String)
        RunCommand(command)
    End Sub

    Public Function GetNewGameFile(originalFilename As String, extensions As String) As String Implements IPlayer.GetNewGameFile
        If MsgBox(String.Format(T("EditorGameFileNotExistLikeFind"), originalFilename, vbCrLf + vbCrLf), MsgBoxStyle.Question Or MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then
            ctlOpenFile.Filter = T("EditorFilterGameFiles") + extensions
            ctlOpenFile.ShowDialog()
            Return ctlOpenFile.FileName
        Else
            Return ""
        End If
    End Function

    Public Sub WindowClosing()
        If Not m_game Is Nothing Then
            CancelWalkthrough()
            m_game.Finish()
        End If
    End Sub

    Private Sub tmrTick_Tick(sender As System.Object, e As System.EventArgs) Handles tmrTick.Tick
        If Not m_gameTimer Is Nothing Then
            m_tickCount += 1
            If m_sendNextTickEventAfter > 0 AndAlso m_tickCount >= m_sendNextTickEventAfter Then
                m_gameTimer.Tick(GetTickCountAndStopTimer())
            End If
        End If
    End Sub

    Private Function GetTickCountAndStopTimer() As Integer
        tmrTick.Enabled = False
        Return m_tickCount
    End Function

    Private Sub Player_HandleDestroyed(sender As Object, e As System.EventArgs) Handles Me.HandleDestroyed
        m_destroyed = True
    End Sub

    Public Sub WriteHTML(html As String) Implements IPlayer.WriteHTML
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub() ctlPlayerHtml.WriteText(html))
    End Sub

    Public Sub LocationUpdated(location As String) Implements IPlayer.LocationUpdated
        BeginInvoke(Sub() ctlPlayerHtml.UpdateLocation(location))
    End Sub

    Public Sub UpdateGameName(name As String) Implements IPlayer.UpdateGameName
        BeginInvoke(Sub() SetGameName(name))
    End Sub

    Public Sub ClearScreen() Implements IPlayer.ClearScreen
        ClearBuffer()
        BeginInvoke(Sub() ctlPlayerHtml.Clear())
    End Sub

    Public Sub SetStatusText(text As String) Implements IPlayer.SetStatusText
        BeginInvoke(Sub() ctlPlayerHtml.UpdateStatus(text))
    End Sub

    Public Sub DoQuit() Implements IPlayer.Quit
        BeginInvoke(Sub()
                        GameFinished()
                        RaiseEvent Quit()
                    End Sub)
    End Sub

    Private m_gameForeground As String = Nothing

    Public Sub SetForeground(colour As String) Implements IPlayer.SetForeground
        m_gameForeground = colour
        If Not UseGameColours Then Return
        m_htmlHelper.SetForeground(colour)
    End Sub

    Public Sub SetFont(name As String) Implements IPlayer.SetFont
        m_htmlHelper.SetFont(name)
    End Sub

    Public Sub SetFontSize(size As String) Implements IPlayer.SetFontSize
        m_htmlHelper.SetFontSize(size)
    End Sub

    Public Sub RequestSave(html As String) Implements IPlayer.RequestSave
        BeginInvoke(Sub() Save())
    End Sub

    Private m_gameLinkForeground As String = Nothing

    Public Sub SetLinkForeground(colour As String) Implements IPlayer.SetLinkForeground
        m_gameLinkForeground = colour
        If Not UseGameColours Then Return
        m_htmlHelper.SetLinkForeground(colour)
    End Sub

    Private Sub WriteLine(text As String)
        m_htmlHelper.AppendText(text)
        ClearBuffer()
    End Sub

    Private Sub ctlPlayerHtml_CommandRequested(command As String) Handles ctlPlayerHtml.CommandRequested
        Dim data = PlayerHelper.GetCommandData(command)
        RunCommand(data.Command, data.Metadata)
    End Sub

    Private Sub ctlPlayerHtml_EndWait() Handles ctlPlayerHtml.EndWait
        m_waiting = False
    End Sub

    Private Sub ctlPlayerHtml_ExitFullScreen() Handles ctlPlayerHtml.ExitFullScreen
        ctlPlayerHtml.ShowExitFullScreenButton(False)
        RaiseEvent ExitFullScreen()
    End Sub

    Private Sub ctlPlayerHtml_Ready() Handles ctlPlayerHtml.Ready
        ' We need this DocumentCompleted event to have finished before trying to output text to the webbrowser control.

        ResetPlayerOverrideColours()
        ResetPlayerOverrideFont()

        If m_gameLoadedSuccessfully Then
            BeginInvoke(Sub()
                            BeginGame()
                        End Sub)
        Else
            WriteLine("<b>" + T("EditorFailedToLoadGame") + "</b>")
            If (m_game.Errors.Count > 0) Then
                WriteLine(T("EditorFollowingErrorsOccurred"))
                For Each loadError As String In m_game.Errors
                    WriteLine(loadError.Replace(Chr(10), "<br/>"))
                Next
            End If
            GameFinished()
        End If
    End Sub

    Private Sub ctlPlayerHtml_Save(html As String) Handles ctlPlayerHtml.Save
        FinishSave(html)
    End Sub

    Private Sub ctlPlayerHtml_SendEvent(eventName As String, param As String) Handles ctlPlayerHtml.SendEvent
        If RecordWalkthrough IsNot Nothing Then
            m_recordedWalkthrough.Add("event:" + eventName + ";" + param)
        End If
        m_game.SendEvent(eventName, param)
        ClearBuffer()
    End Sub

    Private Sub ctlPlayerHtml_Speak(text As String) Handles ctlPlayerHtml.Speak
        Speak(text)
    End Sub

    Public Sub BindMenu(linkid As String, verbs As String, text As String, elementId As String) Implements IPlayerHelperUI.BindMenu
        BeginInvoke(Sub() ctlPlayerHtml.BindMenu(linkid, verbs, text, elementId))
    End Sub

    Public Sub OutputText(text As String) Implements IPlayerHelperUI.OutputText
        BeginInvoke(Sub() ctlPlayerHtml.WriteText(text))
    End Sub

    Public Sub SetAlignment(alignment As String) Implements IPlayerHelperUI.SetAlignment
        ClearBuffer()
        BeginInvoke(Sub() ctlPlayerHtml.SetAlignment(alignment))
    End Sub

    Public Sub DoHide(element As String) Implements IPlayer.Hide
        BeginInvoke(Sub() ctlPlayerHtml.DoHide(element))
    End Sub

    Public Sub DoShow(element As String) Implements IPlayer.Show
        BeginInvoke(Sub() ctlPlayerHtml.DoShow(element))
    End Sub

    Private Sub SetPanesVisible(visible As Boolean)
        SetPanesVisible(If(visible, "on", "disabled"))
    End Sub

    Private Sub SetLocationVisible(visible As Boolean)
        If visible Then DoShow("Location") Else DoHide("Location")
    End Sub

    Private Sub SetCommandVisible(visible As Boolean)
        If visible Then DoShow("Command") Else DoHide("Command")
    End Sub

    Private Sub StopGame()
        DoQuit()
    End Sub

    Private Sub ctlPlayerHtml_ShortcutKeyPressed(keys As System.Windows.Forms.Keys) Handles ctlPlayerHtml.ShortcutKeyPressed
        If keys = Windows.Forms.Keys.Escape Then
            EscPressed()
        End If
        RaiseEvent ShortcutKeyPressed(keys)
    End Sub

    Public Sub SetCompassDirections(dirs As IEnumerable(Of String)) Implements IPlayer.SetCompassDirections
        BeginInvoke(Sub() ctlPlayerHtml.SetCompassDirections(dirs))
    End Sub

    Private Sub ResetInterfaceStrings()
        SetInterfaceString("InventoryLabel", "Inventory")
        SetInterfaceString("StatusLabel", "Status")
        SetInterfaceString("PlacesObjectsLabel", "Places and Objects")
        SetInterfaceString("CompassLabel", "Compass")
        SetInterfaceString("InButtonLabel", "in")
        SetInterfaceString("OutButtonLabel", "out")
        SetInterfaceString("EmptyListLabel", "(empty)")
        SetInterfaceString("NothingSelectedLabel", "(nothing selected)")
        SetInterfaceString("TypeHereLabel", "Type here...")
        SetInterfaceString("ContinueLabel", "Continue...")
    End Sub

    Public Sub SetInterfaceString(name As String, text As String) Implements IPlayer.SetInterfaceString
        BeginInvoke(Sub() ctlPlayerHtml.SetInterfaceString(name, text))
    End Sub

    Public Function GetURL(file As String) As String Implements IPlayer.GetURL
        Return ctlPlayerHtml.GetURL(file)
    End Function

    Private Sub m_gameTimer_RequestNextTimerTick(nextTick As Integer) Handles m_gameTimer.RequestNextTimerTick
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub()
                        m_sendNextTickEventAfter = nextTick
                        m_tickCount = 0
                        tmrTick.Enabled = True
                    End Sub)
        ClearBuffer()
    End Sub

    Public Sub ShowExitFullScreenButton()
        ctlPlayerHtml.ShowExitFullScreenButton(True)
    End Sub

    Public Sub DoPause(ms As Integer) Implements IPlayer.DoPause
        If m_walkthroughRunner IsNot Nothing Then
            m_walkthroughRunner.BeginPause()
        Else
            BeginInvoke(Sub()
                            m_pausing = True
                            tmrPause.Interval = ms
                            tmrPause.Enabled = True
                        End Sub)
        End If
    End Sub

    Private Sub tmrPause_Tick(sender As Object, e As System.EventArgs) Handles tmrPause.Tick
        tmrPause.Enabled = False
        m_pausing = False
        m_game.FinishPause()
        ClearBuffer()
    End Sub

    Private Sub m_walkthroughRunner_MarkScrollPosition() Handles m_walkthroughRunner.MarkScrollPosition
        ctlPlayerHtml.MarkScrollPosition()
        ClearBuffer()
    End Sub

    Private Sub m_walkthroughRunner_Output(text As String) Handles m_walkthroughRunner.Output
        WriteLine(text)
    End Sub

    Public Property PreLaunchAction As Action
        Get
            Return m_preLaunchAction
        End Get
        Set(value As Action)
            m_preLaunchAction = value
        End Set
    End Property

    Public Property PostLaunchAction As Action
        Get
            Return m_postLaunchAction
        End Get
        Set(value As Action)
            m_postLaunchAction = value
        End Set
    End Property

    Public Property RecordWalkthrough As String
        Get
            Return m_recordWalkthrough
        End Get
        Set(value As String)
            m_recordWalkthrough = value
            If value IsNot Nothing Then
                m_recordedWalkthrough = New List(Of String)
            End If
        End Set
    End Property

    Public Property UseGameColours As Boolean
        Get
            Return m_allowColourChange
        End Get
        Set(value As Boolean)
            Dim changed As Boolean = m_allowColourChange <> value
            m_allowColourChange = value
            If m_htmlHelper IsNot Nothing Then m_htmlHelper.UseGameColours = value
            If changed Then ResetPlayerOverrideColours()
        End Set
    End Property

    Public Property UseGameFont As Boolean
        Get
            Return m_allowFontChange
        End Get
        Set(value As Boolean)
            Dim changed As Boolean = m_allowFontChange <> value
            m_allowFontChange = value
            If m_htmlHelper IsNot Nothing Then m_htmlHelper.UseGameFont = value
            If changed Then ResetPlayerOverrideFont()
        End Set
    End Property

    Public Property UseSAPI As Boolean
        Get
            Return m_useSapi
        End Get
        Set(value As Boolean)
            m_useSapi = value
            If Not value Then
                m_speech.SpeakAsyncCancelAll()
            End If
        End Set
    End Property

    Private m_playerOverrideBackground As Color
    Private m_playerOverrideForeground As Color
    Private m_playerOverrideLink As Color

    Private m_playerOverrideBackgroundHtml As String
    Private m_playerOverrideForegroundHtml As String
    Private m_playerOverrideLinkHtml As String

    Public Sub SetPlayerOverrideColours(background As Color, foreground As Color, link As Color)
        m_playerOverrideBackground = background
        m_playerOverrideForeground = foreground
        m_playerOverrideLink = link

        m_playerOverrideBackgroundHtml = ColorTranslator.ToHtml(background)
        m_playerOverrideForegroundHtml = ColorTranslator.ToHtml(foreground)
        m_playerOverrideLinkHtml = ColorTranslator.ToHtml(link)

        If m_htmlHelper IsNot Nothing Then
            m_htmlHelper.PlayerOverrideForeground = m_playerOverrideForegroundHtml
        End If
        If Not UseGameColours Then
            BeginInvoke(Sub() ctlPlayerHtml.SetBackground(m_playerOverrideBackgroundHtml))
            If m_htmlHelper IsNot Nothing Then
                m_htmlHelper.SetForeground(m_playerOverrideForegroundHtml)
                m_htmlHelper.SetLinkForeground(m_playerOverrideLinkHtml)
            End If
            ClearBuffer()
        End If
    End Sub

    Private Sub ResetPlayerOverrideColours()
        If Not UseGameColours Then
            SetPlayerOverrideColours(m_playerOverrideBackground, m_playerOverrideForeground, m_playerOverrideLink)
        Else
            If m_gameBackground IsNot Nothing Then SetBackground(m_gameBackground)
            If m_gameForeground IsNot Nothing Then SetForeground(m_gameForeground)
            If m_gameLinkForeground IsNot Nothing Then SetLinkForeground(m_gameLinkForeground)
            ClearBuffer()
        End If
    End Sub

    Private m_playerOverrideFontFamily As String
    Private m_playerOverrideFontSize As Single
    Private m_playerOverrideFontStyle As FontStyle

    Public Sub SetPlayerOverrideFont(fontFamily As String, fontSize As Single, fontStyle As FontStyle)
        m_playerOverrideFontFamily = fontFamily
        m_playerOverrideFontSize = fontSize
        m_playerOverrideFontStyle = fontStyle
        If m_htmlHelper IsNot Nothing Then
            m_htmlHelper.PlayerOverrideFontFamily = fontFamily
            m_htmlHelper.PlayerOverrideFontSize = fontSize
            ' TO DO: Font style is currently ignored
        End If
    End Sub

    Private Sub ResetPlayerOverrideFont()
        If m_htmlHelper IsNot Nothing Then
            m_htmlHelper.UseGameFont = UseGameFont
        End If
        If Not UseGameFont Then
            SetPlayerOverrideFont(m_playerOverrideFontFamily, m_playerOverrideFontSize, m_playerOverrideFontStyle)
        End If
    End Sub

    Public Property PlaySounds As Boolean
        Get
            Return m_playSounds
        End Get
        Set(value As Boolean)
            m_playSounds = value
        End Set
    End Property

    Public Sub SetPanelContents(html As String) Implements IPlayer.SetPanelContents
        BeginInvoke(Sub() ctlPlayerHtml.SetPanelContents(html))
    End Sub

    Public Sub EscPressed()
        If m_fromEditor And m_initialised Then
            DoQuit()
        End If
    End Sub

    Public Sub Log(text As String) Implements IPlayer.Log
        BeginInvoke(Sub()
                        If m_log Is Nothing Then
                            m_log = New Log
                        End If
                        If m_log.txtLog.TextLength > 0 Then
                            m_log.txtLog.AppendText(Environment.NewLine)
                        End If
                        m_log.txtLog.AppendText(DateTime.Now.ToString() + " " + text)
                        m_log.txtLog.Select(m_log.txtLog.Text.Length, 0)
                        m_log.txtLog.ScrollToCaret()
                    End Sub)

    End Sub

    Public Sub ResetAfterGameFinished()
        m_initialised = False
        ctlPlayerHtml.Reset()
    End Sub

    Public Function GetUIOption(ByVal [option] As UIOption) As String Implements IPlayer.GetUIOption
        Select Case [option]
            Case UIOption.UseGameColours
                Return If(UseGameColours, "true", "false")
            Case UIOption.UseGameFont
                Return If(UseGameFont, "true", "false")
            Case UIOption.OverrideForeground
                Return m_playerOverrideForegroundHtml
            Case UIOption.OverrideLinkForeground
                Return m_playerOverrideLinkHtml
            Case UIOption.OverrideFontName
                Return m_htmlHelper.PlayerOverrideFontFamily
            Case UIOption.OverrideFontSize
                Return m_htmlHelper.PlayerOverrideFontSize.ToString(CultureInfo.InvariantCulture)
            Case Else
                Throw New NotImplementedException()
        End Select
    End Function

    Private Shared s_regexHtml As New System.Text.RegularExpressions.Regex("\<.+?\>")

    Friend Shared Function StripTags(text As String) As String
        text = s_regexHtml.Replace(text, "")
        text = text.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&")
        Return text
    End Function

End Class
