Imports System.Xml
Imports System.Threading

Public Class Player

    Implements IPlayerHelperUI

    Private m_htmlHelper As PlayerHelper
    Private m_panesVisible As Boolean
    Private WithEvents m_game As IASL
    Private WithEvents m_gameTimer As IASLTimer
    Private m_gameDebug As IASLDebug
    Private m_initialised As Boolean
    Private m_gameReady As Boolean
    Private m_gameName As String
    Private WithEvents m_debugger As Debugger
    Private m_loaded As Boolean = False
    Private m_menu As AxeSoftware.Quest.Controls.Menu = Nothing
    Private m_saveFile As String
    Private m_waiting As Boolean = False
    Private m_speech As New System.Speech.Synthesis.SpeechSynthesizer
    Private m_loopSound As Boolean = False
    Private m_waitingForSoundToFinish As Boolean = False
    Private m_soundPlaying As Boolean = False
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
    Private m_fromEditor As Boolean = False
    Private WithEvents m_log As Log

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
        PanesVisible = True
        Reset()

    End Sub

    Public Sub SetMenu(menu As AxeSoftware.Quest.Controls.Menu)
        m_menu = menu

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
    End Sub

    Private Sub DebuggerMenuClick()
        ShowDebugger(Not m_menu.MenuChecked("debugger"))
    End Sub

    Private Sub LogMenuClick()
        ShowLog(Not m_menu.MenuChecked("log"))
    End Sub

    Public Sub Initialise(ByRef game As IASL, Optional fromEditor As Boolean = False)
        m_fromEditor = fromEditor
        SetPanesVisible(True)
        SetCommandVisible(True)
        SetLocationVisible(True)
        SetStatusText("")
        LocationUpdated("")
        m_menu.ClearWindowMenu()
        m_menu.MenuEnabled("copy") = True
        m_gameBackground = Nothing
        m_gameForeground = Nothing
        m_game = game
        m_gameDebug = TryCast(game, IASLDebug)
        m_gameTimer = TryCast(m_game, IASLTimer)
        m_gameReady = True
        ResetInterfaceStrings()
        m_htmlHelper = New PlayerHelper(m_game, Me)
        m_htmlHelper.UseGameColours = UseGameColours
        ctlPlayerHtml.Setup()
 
        Me.Cursor = Cursors.WaitCursor

        Dim success As Boolean = m_game.Initialise(Me)

        Me.Cursor = Cursors.Default

        If success Then
            AddToRecentList()
            m_menu.MenuEnabled("walkthrough") = m_gameDebug IsNot Nothing AndAlso m_gameDebug.Walkthroughs IsNot Nothing AndAlso m_gameDebug.Walkthroughs.Walkthroughs.Count > 0
            m_menu.MenuEnabled("debugger") = m_gameDebug IsNot Nothing AndAlso m_gameDebug.DebugEnabled

            Dim scripts As IEnumerable(Of String) = m_game.GetExternalScripts

            ' Generate the new HTML and wait for Ready event
            ctlPlayerHtml.InitialiseHTMLUI(scripts)
        Else
            WriteLine("<b>Failed to load game.</b>")
            If (m_game.Errors.Count > 0) Then
                WriteLine("The following errors occurred:")
                For Each loadError As String In m_game.Errors
                    WriteLine(loadError.Replace(Chr(10), "<br/>"))
                Next
            End If
            GameFinished()
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
        ctlPlayerHtml.DisableNavigation()
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
        If Not m_game Is Nothing Then m_game.Finish()
        m_initialised = False
        m_gameReady = False
        m_gameName = ""
        ShowDebugger(False)
        ShowLog(False)
        m_debugger = Nothing
        m_log = Nothing
        If Not m_menu Is Nothing Then
            m_menu.MenuEnabled("walkthrough") = False
            m_menu.MenuEnabled("debugger") = False
        End If
    End Sub

    Public Property PanesVisible() As Boolean
        Get
            Return m_panesVisible
        End Get
        Set(value As Boolean)
            m_panesVisible = value
            ' TO DO: Call JS function
        End Set
    End Property

    ' TO DO: Call JS function
    'Private Sub cmdPanes_Click(sender As System.Object, e As System.EventArgs)
    '    PanesVisible = Not PanesVisible
    'End Sub

    Private Sub RunCommand(command As String)

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
                m_gameTimer.SendCommand(command, GetTickCountAndStopTimer())
            Else
                m_game.SendCommand(command)
            End If
            ClearBuffer()
        Catch ex As Exception
            WriteLine(String.Format("Error running command '{0}':<br>{1}", command, ex.Message))
        End Try

    End Sub

    Private Sub m_game_Finished() Handles m_game.Finished
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub() GameFinished())
    End Sub

    Private Sub GameFinished()
        If Not m_initialised Then Return
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
    End Sub

    Private Sub m_game_LogError(errorMessage As String) Handles m_game.LogError
        If Not Me.IsHandleCreated Then Return
        BeginInvoke(Sub()
                        WriteLine("<output><b>Sorry, an error occurred.</b></output>")
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

    Private Sub m_game_UpdateList(listType As AxeSoftware.Quest.ListType, items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData)) Handles m_game.UpdateList
        BeginInvoke(Sub() UpdateList(listType, items))
    End Sub

    Private Sub UpdateList(listType As AxeSoftware.Quest.ListType, items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData))
        ' Keep the IASL interface atomic, so we transmit lists of places separately to lists of objects.
        ' We can merge them when we receive them here, and then pass the merged list to the ElementList with some
        ' kind of flag so it knows what's a place and what's an object.

        Select Case listType
            Case AxeSoftware.Quest.ListType.ObjectsList
                ctlPlayerHtml.UpdatePlacesObjectsList(items)
            Case AxeSoftware.Quest.ListType.InventoryList
                ctlPlayerHtml.UpdateInventoryList(items)
            Case AxeSoftware.Quest.ListType.ExitsList
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
    End Sub

    Private Sub m_log_VisibleChanged(sender As Object, e As System.EventArgs) Handles m_log.VisibleChanged
        m_menu.MenuChecked("log") = m_log.Visible
    End Sub

    Private Sub RunWalkthrough()
        ' Eventually we want to pop up a debugging panel on the right of the screen where we can select
        ' walkthroughs, step through etc.

        Dim walkThrough As String = ChooseWalkthrough()
        If walkThrough Is Nothing Then Return

        RunWalkthrough(walkThrough)
    End Sub

    Public Sub RunWalkthrough(name As String)
        InitWalkthrough(name)
        StartWalkthrough()
    End Sub

    Public Sub InitWalkthrough(name As String)
        m_walkthroughRunner = New WalkthroughRunner(m_gameDebug, name)
        If m_walkthroughRunner.Steps = 0 Then
            m_walkthroughRunner = Nothing
        End If
    End Sub

    Public Sub StartWalkthrough()
        Dim runnerThread As New Thread(Sub() WalkthroughRunner())
        runnerThread.Start()
    End Sub

    Private Sub WalkthroughRunner()
        If m_walkthroughRunner Is Nothing Then Exit Sub

        Try
            m_walkthroughRunner.Run()

        Catch ex As Exception
            BeginInvoke(Sub() WriteLine(String.Format("Error - walkthrough halted:<br>{0}", ex.Message)))

        Finally
            m_walkthroughRunner = Nothing
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

        menuForm.Caption = "Please choose a walkthrough to run:"
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

    Private Sub RunScript(data As String) Implements IPlayer.RunScript
        ClearBuffer()
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

                        ctlPlayerHtml.InvokeScript(functionName, args.ToArray())
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

    Private Sub Save(filename As String)
        Try
            m_game.Save(filename)
            RaiseEvent AddToRecent(filename, m_gameName + " (Saved)")
            WriteLine("")
            WriteLine("Saved: " + filename)
            ClearBuffer()
        Catch ex As Exception
            MsgBox("Unable to save the file due to the following error:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
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

    Private Sub wbOutput_PreviewKeyDown(sender As Object, e As System.Windows.Forms.PreviewKeyDownEventArgs)
        KeyPressed()
    End Sub

    Private Sub SetPanesVisible(data As String) Implements IPlayer.SetPanesVisible
        BeginInvoke(Sub()
                        Select Case data
                            Case "on"
                                PanesVisible = True
                                ' TO DO: Call JS function
                                'cmdPanes.Visible = True
                            Case "off"
                                PanesVisible = False
                                ' TO DO: Call JS function
                                'cmdPanes.Visible = True
                            Case "disabled"
                                PanesVisible = False
                                ' TO DO: Call JS function
                                'cmdPanes.Visible = False
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
                            Throw New Exception("Can't play sound that is both synchronous and looped")
                        End If

                        If System.IO.File.Exists(filename) And PlaySounds Then
                            m_loopSound = looped
                            m_soundPlaying = True

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
        m_soundPlaying = False
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
        BeginInvoke(Sub() m_speech.Speak(text))
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

    Public Sub RequestLoad() Implements IPlayer.RequestLoad
        ' TO DO: Raise event
    End Sub

    Public Sub RequestRestart() Implements IPlayer.RequestRestart
        ' TO DO: Raise event
    End Sub

    Public Sub RequestSave() Implements IPlayer.RequestSave
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
        RunCommand(command)
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
        BeginInvoke(Sub()
                        BeginGame()
                    End Sub)
    End Sub

    Private Sub ctlPlayerHtml_SendEvent(eventName As String, param As String) Handles ctlPlayerHtml.SendEvent
        m_game.SendEvent(eventName, param)
        ClearBuffer()
    End Sub

    Public Sub BindMenu(linkid As String, verbs As String, text As String) Implements IPlayerHelperUI.BindMenu
        BeginInvoke(Sub() ctlPlayerHtml.BindMenu(linkid, verbs, text))
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
        m_waiting = False
        RaiseEvent ShortcutKeyPressed(keys)
    End Sub

    Public Sub SetCompassDirections(dirs As IEnumerable(Of String)) Implements IPlayer.SetCompassDirections
        ' TO DO: Call JS function to set compass direction names
        'ctlCompass.CompassDirections = New List(Of String)(dirs)
        'lstPlacesObjects.IgnoreNames = ctlCompass.CompassDirections
    End Sub

    Private Sub ResetInterfaceStrings()
        SetInterfaceString("InventoryLabel", "Inventory")
        SetInterfaceString("PlacesObjectsLabel", "Places and Objects")
        SetInterfaceString("CompassLabel", "Compass")
        SetInterfaceString("InButtonLabel", "in")
        SetInterfaceString("OutButtonLabel", "out")
        SetInterfaceString("EmptyListLabel", "(empty)")
        SetInterfaceString("NothingSelectedLabel", "(nothing selected)")
    End Sub

    Public Sub SetInterfaceString(name As String, text As String) Implements IPlayer.SetInterfaceString
        BeginInvoke(Sub()
                        Select Case name
                            Case "InventoryLabel"
                                ' TO DO: Call JS function
                                'lstInventory.Title = text
                            Case "PlacesObjectsLabel"
                                ' TO DO: Call JS function
                                'lstPlacesObjects.Title = text
                            Case "CompassLabel"
                                ' TO DO: Call JS function
                                'lblCompass.Text = text
                            Case "InButtonLabel"
                                ' TO DO: Call JS function
                                'ctlCompass.InLabel = text
                            Case "OutButtonLabel"
                                ' TO DO: Call JS function
                                'ctlCompass.OutLabel = text
                            Case "EmptyListLabel"
                                ' TO DO: Call JS function - or is this label is obsolete?
                                'lstInventory.EmptyListLabel = text
                                'lstPlacesObjects.EmptyListLabel = text
                            Case "NothingSelectedLabel"
                                ' TO DO: Call JS function - or is this label is obsolete?
                                'lstInventory.NothingSelectedLabel = text
                                'lstPlacesObjects.NothingSelectedLabel = text
                        End Select
                    End Sub)
    End Sub

    Public Function GetURL(file As String) As String Implements IPlayer.GetURL
        Return file
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

    Private m_playerOverrideBackground As Color
    Private m_playerOverrideForeground As Color
    Private m_playerOverrideLink As Color

    Public Sub SetPlayerOverrideColours(background As Color, foreground As Color, link As Color)
        m_playerOverrideBackground = background
        m_playerOverrideForeground = foreground
        m_playerOverrideLink = link
        If m_htmlHelper IsNot Nothing Then
            m_htmlHelper.PlayerOverrideForeground = ColorTranslator.ToHtml(foreground)
        End If
        If Not UseGameColours Then
            BeginInvoke(Sub() ctlPlayerHtml.SetBackground(ColorTranslator.ToHtml(background)))
            If m_htmlHelper IsNot Nothing Then
                m_htmlHelper.SetForeground(ColorTranslator.ToHtml(foreground))
                m_htmlHelper.SetLinkForeground(ColorTranslator.ToHtml(link))
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

    Public Property ScriptErrorsSuppressed As Boolean
        Get
            Return ctlPlayerHtml.ScriptErrorsSuppressed
        End Get
        Set(value As Boolean)
            ctlPlayerHtml.ScriptErrorsSuppressed = value
        End Set
    End Property

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
End Class
