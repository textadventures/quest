Imports AxeSoftware.Quest
Imports System.Text.RegularExpressions
Imports System.Xml

Public Class Player

    Private m_panesVisible As Boolean
    Private WithEvents m_game As IASL
    Private m_initialised As Boolean
    Private m_browserReady As Boolean
    Private m_gameReady As Boolean
    Private m_links As New Dictionary(Of String, HyperlinkType)
    Private m_history As New List(Of String)
    Private m_historyPoint As Integer
    Private m_gameName As String
    Private WithEvents m_debugger As Debugger
    Private m_fontName As String = "Arial"
    Private m_fontSize As Integer = 9
    Private m_style As String
    Private m_linkStyle As String
    Private m_foreground As String
    Private m_linkForeground As String
    Private m_bold As Boolean
    Private m_italic As Boolean
    Private m_underline As Boolean
    Private WithEvents m_webScripting As New WebScripting
    Private m_stage As Integer
    Private m_loaded As Boolean = False
    Private m_splitHelpers As New List(Of AxeSoftware.Utility.SplitterHelper)
    Private m_menu As AxeSoftware.Quest.Controls.Menu = Nothing
    Private m_saveFile As String

    Public Event Quit()
    Public Event AddToRecent(ByVal filename As String, ByVal name As String)
    Public Event SetGameName(ByVal name As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        PanesVisible = True
        Reset()

        lstPlacesObjects.IgnoreNames = New List(Of String)(ctlCompass.CompassDirections)

        wbOutput.ObjectForScripting = m_webScripting

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
            m_browserReady = False
            m_stage = 0
            wbOutput.Navigate(My.Application.Info.DirectoryPath() & "\Blank.htm")
            m_game = game
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

    Public Property PanesVisible()
        Get
            Return m_panesVisible
        End Get
        Set(ByVal value)
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

        Try
            m_game.SendCommand(command)
        Catch ex As Exception
            WriteLine(String.Format("Error running command '{0}':<br>{1}", command, ex.Message))
        End Try

        txtCommand.Text = ""
        txtCommand.Focus()

    End Sub

    Private Sub ctlCompass_RunCommand(ByVal command As String) Handles ctlCompass.RunCommand
        RunCommand(command)
    End Sub

    Private Function GetUniqueID() As String
        Static id As Integer
        id += 1
        Return "k" + id.ToString()
    End Function

    Private Sub WriteText(ByVal text As String)
        ' If text starts with a space then IE won't print it.
        ' TO DO: This needs to be replaced with a regex of some kind
        If Microsoft.VisualBasic.Left(text, 1) = " " Then text = "&nbsp;" + Mid(text, 2)

        Dim newText As HtmlElement = wbOutput.Document.CreateElement("span")
        newText.Style = GetCurrentStyle(False)
        newText.InnerHtml = text
        wbOutput.Document.Body.AppendChild(newText)
        newText.ScrollIntoView(True)
    End Sub

    Private Sub WriteLine(ByVal text As String)
        WriteText(text + "<br />")
    End Sub

    Private Sub m_game_Finished() Handles m_game.Finished
        GameFinished()
    End Sub

    Private Sub GameFinished()
        m_initialised = False
        SetEnabledState(False)
    End Sub

    Private Sub m_game_PrintText(ByVal text As String) Handles m_game.PrintText

        Dim currentTag As String = ""
        Dim currentTagValue As String = ""
        Dim settings As XmlReaderSettings = New XmlReaderSettings()
        settings.IgnoreWhitespace = False
        Dim reader As XmlReader = XmlReader.Create(New System.IO.StringReader(text), settings)

        While reader.Read()
            Select Case reader.NodeType
                Case XmlNodeType.Element
                    Select Case reader.Name
                        Case "output"
                            ' do nothing
                        Case "object"
                            currentTag = "object"
                        Case "exit"
                            currentTag = "exit"
                        Case "br"
                            WriteText("<br />")
                        Case "b"
                            Bold = True
                        Case "i"
                            Italic = True
                        Case "u"
                            Underline = True
                        Case Else
                            Throw New Exception(String.Format("Unrecognised element '{0}'", reader.Name))
                    End Select
                Case XmlNodeType.Text
                    If Len(currentTag) = 0 Then
                        WriteText(reader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"))
                    Else
                        currentTagValue = reader.Value
                    End If
                Case XmlNodeType.Whitespace
                    WriteText(reader.Value.Replace(" ", "&nbsp;"))
                Case XmlNodeType.EndElement
                    Select Case reader.Name
                        Case "output"
                            ' do nothing
                        Case "object"
                            currentTag = ""
                            AddLink(currentTagValue, HyperlinkType.ObjectLink)
                        Case "exit"
                            currentTag = ""
                            AddLink(currentTagValue, HyperlinkType.ExitLink)
                        Case "b"
                            Bold = False
                        Case "i"
                            Italic = False
                        Case "u"
                            Underline = False
                        Case Else
                            Throw New Exception(String.Format("Unrecognised element '{0}'", reader.Name))
                    End Select
            End Select

        End While

        WriteText("<br />")
    End Sub

    Private Sub AddLink(ByVal linkText As String, ByVal linkType As HyperlinkType)
        Dim id As String = GetUniqueID()
        Dim newLink As HtmlElement = wbOutput.Document.CreateElement("a")
        m_links.Add(id, linkType)
        newLink.Id = id
        newLink.InnerHtml = linkText
        newLink.SetAttribute("href", "javascript:")
        newLink.Style = GetCurrentStyle(True)
        wbOutput.Document.Body.AppendChild(newLink)
        AddHandler newLink.Click, AddressOf InLineLinkClicked
    End Sub

    Private Sub wbOutput_DocumentCompleted(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles wbOutput.DocumentCompleted
        m_browserReady = True
        m_stage = m_stage + 1
        TryInitialise()
    End Sub

    Private Sub TryInitialise()
        Dim success As Boolean

        If (Not m_initialised) And m_gameReady And m_browserReady Then

            If m_stage = 1 Then

                success = m_game.Initialise()

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
                    If Not String.IsNullOrEmpty(m_game.SaveFilename) Then
                        RaiseEvent AddToRecent(m_game.SaveFilename, m_gameName + " (Saved)")
                    ElseIf Not m_game.Filename Is Nothing Then
                        RaiseEvent AddToRecent(m_game.Filename, m_gameName)
                    End If
                End If

                ShowDebugger(False)
                m_debugger = Nothing

                If success Then
                    Dim URL As String = m_game.GetInterface()
                    If Not URL Is Nothing Then
                        wbOutput.Navigate(m_game.GetInterface())
                    Else
                        m_game.Begin()
                    End If
                End If

            ElseIf m_stage = 2 Then
                m_initialised = True
                m_game.Begin()
                txtCommand.Focus()
            Else
                Throw New Exception("Undefined startup stage")
            End If
        End If
    End Sub

    Private Sub InLineLinkClicked(ByVal sender As Object, ByVal e As EventArgs)

        Dim link As HtmlElement = wbOutput.Document.ActiveElement

        ' TO DO: show menu instead
        Select Case m_links(link.Id)
            Case HyperlinkType.ObjectLink
                tmrTimer.Tag = "look at " + link.InnerText
            Case HyperlinkType.ExitLink
                tmrTimer.Tag = "go " + link.InnerText
        End Select

        tmrTimer.Enabled = True

        ' We use a timer to run the command as otherwise we're running a command in the middle
        ' of a link click event, which means we don't scroll the window properly

    End Sub

    Private Sub tmrTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrTimer.Tick
        tmrTimer.Enabled = False
        If Not tmrTimer.Tag Is Nothing Then
            RunCommand(tmrTimer.Tag)
        Else
            EnterText()
        End If
    End Sub

    Private Sub m_game_RequestRaised(ByVal request As AxeSoftware.Quest.Request, ByVal data As String) Handles m_game.RequestRaised
        Select Case request
            Case AxeSoftware.Quest.Request.Quit
                RaiseEvent Quit()
            Case AxeSoftware.Quest.Request.Load
            Case AxeSoftware.Quest.Request.Save
            Case AxeSoftware.Quest.Request.UpdateLocation
                lblBanner.Text = data
            Case AxeSoftware.Quest.Request.GameName
                m_gameName = data
                RaiseEvent SetGameName(data)
            Case AxeSoftware.Quest.Request.FontName
                FontName = data
            Case AxeSoftware.Quest.Request.FontSize
                FontSize = CInt(data)
            Case AxeSoftware.Quest.Request.Background
                SetBackground(data)
            Case AxeSoftware.Quest.Request.Foreground
                Foreground = data
            Case AxeSoftware.Quest.Request.LinkForeground
                LinkForeground = data
            Case AxeSoftware.Quest.Request.RunScript
                RunScript(data)
            Case Else
                Throw New Exception("Unhandled request")
        End Select
    End Sub

    Private Function m_game_ShowMenu(ByVal menuData As AxeSoftware.Quest.MenuData) As String Handles m_game.ShowMenu
        Dim menuForm As Menu
        menuForm = New Menu()

        menuForm.Caption = menuData.Caption
        menuForm.Options = menuData.Options
        menuForm.AllowCancel = menuData.AllowCancel
        menuForm.ShowDialog()

        Return menuForm.SelectedItem
    End Function

    Private Sub m_game_UpdateList(ByVal listType As AxeSoftware.Quest.ListType, ByVal items As System.Collections.Generic.List(Of AxeSoftware.Quest.ListData)) Handles m_game.UpdateList

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
                m_debugger.Game = m_game
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

    Private Function GetCurrentStyle(ByVal link As Boolean) As String
        If Len(m_style) = 0 Then
            m_style = "font-family:" + m_fontName _
                + ";font-size:" + CStr(m_fontSize) + "pt"

            If Bold Then
                m_style += ";font-weight:bold"
            End If

            If Italic Then
                m_style += ";font-style:italic"
            End If

            ' Omit text-decoration from link style by setting it here
            m_linkStyle = m_style

            If Underline Then
                m_style = m_style + ";text-decoration:underline"
            End If

            If Len(m_foreground) > 0 Then
                m_style += ";color:" + m_foreground
            End If
            If Len(m_linkForeground) > 0 Then
                m_linkStyle += ";color:" + m_linkForeground
            End If
            Return m_style
        End If

        If Not link Then
            Return m_style
        Else
            Return m_linkStyle
        End If
    End Function

    Public Property FontName() As String
        Get
            Return m_fontName
        End Get
        Set(ByVal value As String)
            m_fontName = value
            m_style = ""
        End Set
    End Property

    Public Property FontSize() As Integer
        Get
            Return m_fontSize
        End Get
        Set(ByVal value As Integer)
            m_fontSize = value
            m_style = ""
        End Set
    End Property

    Public Property Foreground() As String
        Get
            Return m_foreground
        End Get
        Set(ByVal value As String)
            m_foreground = value
            m_style = ""
        End Set
    End Property

    Public Property LinkForeground() As String
        Get
            Return m_linkForeground
        End Get
        Set(ByVal value As String)
            m_linkForeground = value
            m_style = ""
        End Set
    End Property

    Public Property Bold() As Boolean
        Get
            Return m_bold
        End Get
        Set(ByVal value As Boolean)
            m_bold = value
            m_style = ""
        End Set
    End Property

    Public Property Italic() As Boolean
        Get
            Return m_italic
        End Get
        Set(ByVal value As Boolean)
            m_italic = value
            m_style = ""
        End Set
    End Property

    Public Property Underline() As Boolean
        Get
            Return m_underline
        End Get
        Set(ByVal value As Boolean)
            m_underline = value
            m_style = ""
        End Set
    End Property

    Private Sub InvokeScript(ByVal functionName As String, ByVal ParamArray args() As String)
        wbOutput.Document.InvokeScript(functionName, args)
    End Sub

    Private Sub SetBackground(ByVal colour As String)
        InvokeScript("SetBackground", colour)
    End Sub

    Private Sub RunScript(ByVal data As String)
        Dim functionName As String = ""
        Dim dataList As List(Of String)
        Dim args As New List(Of String)

        dataList = New List(Of String)(data.Split(";"))

        For Each value As String In dataList
            If functionName.Length = 0 Then
                functionName = value
            Else
                args.Add(value.Trim())
            End If
        Next

        InvokeScript(functionName, args.ToArray())
    End Sub

    Private Sub Undo()
        RunCommand("undo")
    End Sub

    Private Sub Copy()
        wbOutput.Document.ExecCommand("Copy", True, Nothing)
    End Sub

    Private Sub SelectAll()
        wbOutput.Document.ExecCommand("SelectAll", True, Nothing)
    End Sub

    Private Sub m_webScripting_WebEvent(ByVal name As String, ByVal param As String) Handles m_webScripting.WebEvent
        m_game.SendEvent(name, param)
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
        ctlSaveFile.DefaultExt = "aslx"
        ctlSaveFile.Filter = "Quest Games|*.aslx|All files|*.*"
        ctlSaveFile.FileName = m_saveFile
        If ctlSaveFile.ShowDialog() = DialogResult.OK Then
            m_saveFile = ctlSaveFile.FileName
            Save(m_saveFile)
        End If
    End Sub

    Private Sub Save(ByVal filename As String)
        Try
            System.IO.File.WriteAllText(filename, m_game.Save())
            RaiseEvent AddToRecent(filename, m_gameName + " (Saved)")
            WriteLine("")
            WriteLine("Saved: " + filename)
        Catch ex As Exception
            MsgBox("Unable to save the file due to the following error:" + Environment.NewLine + Environment.NewLine + ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

End Class
