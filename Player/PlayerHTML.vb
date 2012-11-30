Imports System.Xml
Imports Microsoft.Win32
Imports CefSharp.WinForms
Imports CefSharp

Public Class PlayerHTML

    Public Event Ready()
    Public Event CommandRequested(command As String)
    Public Event SendEvent(eventName As String, param As String)
    Public Event ShortcutKeyPressed(keys As System.Windows.Forms.Keys)
    Public Event EndWait()
    Public Event ExitFullScreen()

    ' TO DO: Both Blank.htm and grid.js should be loaded from embedded resource
    Private m_baseHtmlPath As String = My.Application.Info.DirectoryPath() & "\Blank.htm"
    Private m_gridJsPath As String = My.Application.Info.DirectoryPath() & "\grid.js"

    Private m_buffer As New List(Of Action)
    Private m_resetting As Boolean = False
    Private WithEvents ctlWebView As WebView
    Private m_schemeHandler As CefSchemeHandlerFactory
    Private m_resourceSchemeHandler As CefResourceSchemeHandlerFactory
    Private WithEvents m_interop As QuestCefInterop
    Private WithEvents m_keyHandler As CefKeyboardHandler
    Private m_browserInitialized As Boolean = False
    Private m_browserInitializationLock As New Object()

    Private Sub PlayerHTML_Load(sender As Object, e As EventArgs) Handles Me.Load
        ctlWebView = New WebView()
        ctlWebView.Dock = DockStyle.Fill
        Controls.Add(ctlWebView)
        ctlWebView.CreateControl()
        m_keyHandler = New CefKeyboardHandler()
        ctlWebView.KeyboardHandler = m_keyHandler

        m_schemeHandler = New CefSchemeHandlerFactory()
        m_resourceSchemeHandler = New CefResourceSchemeHandlerFactory()
        m_interop = New QuestCefInterop()

        CEF.Initialize(New Settings)
        CEF.RegisterScheme("quest", m_schemeHandler)
        CEF.RegisterScheme("res", m_resourceSchemeHandler)
        CEF.RegisterJsObject("questCefInterop", m_interop)
    End Sub

    Public Sub WriteText(text As String)
        If (text.Length > 0) Then
            InvokeScript("addText", text)
            StripTagsAndSendToJaws(text)
        End If
    End Sub

    Public Sub SetAlignment(align As String)
        If align.Length = 0 Then align = "left"
        InvokeScript("createNewDiv", align)
    End Sub

    Public Sub BindMenu(linkid As String, verbs As String, text As String, elementId As String)
        InvokeScript("bindMenu", linkid, verbs, text, elementId)
    End Sub

    Private Sub OnDocumentLoad()
        If m_resetting Then
            m_resetting = False
            Return
        End If
        RaiseEvent Ready()
    End Sub

    Private Sub UIEvent(cmd As String, args As String)
        Select Case cmd
            Case "RunCommand"
                RunCommand(args)
            Case "ASLEvent"
                RunASLEvent(args)
            Case "GoURL"
                GoURL(args)
            Case "EndWait"
                RaiseEvent EndWait()
            Case "ExitFullScreen"
                RaiseEvent ExitFullScreen()
        End Select
    End Sub

    Private Sub RunASLEvent(data As String)
        Dim args As String() = data.Split({";"c}, 2)
        RaiseEvent SendEvent(args(0), args(1))
    End Sub

    Private Sub RunCommand(data As String)
        RaiseEvent CommandRequested(data)
    End Sub

    Private Sub GoURL(data As String)
        If data.StartsWith("http://") Or data.StartsWith("https://") Or data.StartsWith("mailto:") Then
            System.Diagnostics.Process.Start(data)
        End If
    End Sub

    Public Sub Clear()
        InvokeScript("clearScreen", "")
    End Sub

    Public Function GetURL(filename As String) As String
        Dim id = m_schemeHandler.AddImage(filename)
        Return "quest://local/" + id
    End Function

    Public Sub ShowPicture(filename As String)
        WriteText(String.Format("<img src=""{0}"" onload=""scrollToEnd();"" /><br />", GetURL(filename)))
    End Sub

    Public Sub Copy()
        ctlWebView.Copy()
    End Sub

    Public Sub SelectAll()
        InvokeScript("selectText", "divOutput")
        ClearBuffer()
    End Sub

    Public Sub InvokeScript(functionName As String, ParamArray args() As String)
        Dim quotedArgs = From arg In args
                         Select (If(arg Is Nothing, """""", """" + arg.Replace("\", "\\").Replace("""", "\""").Replace(Chr(13), "").Replace(Chr(10), "") + """"))
        Dim script As String = String.Format("{0}({1})", functionName, String.Join(",", quotedArgs))
        Debug.WriteLine(script)
        m_buffer.Add(Sub() ctlWebView.ExecuteScript(script))
    End Sub

    Public Sub SetBackground(colour As String)
        InvokeScript("setBackground", colour)
    End Sub

    Public Sub ClearBuffer()
        If Not Me.IsHandleCreated Then Return
        ' copy m_buffer to a new list, in case invoking scripts cause new scripts to be added
        ' to the buffer.
        Do
            Dim bufferCopy As List(Of Action) = New List(Of Action)(m_buffer)
            m_buffer.Clear()
            For Each script In bufferCopy
                script.Invoke()
            Next
        Loop Until Not m_buffer.Any()
    End Sub

    Private Const k_scriptsPlaceholder As String = "<!-- EXTERNAL_SCRIPTS_PLACEHOLDER -->"
    Private Const k_stylesheetsPlaceholder As String = "<!-- EXTERNAL_STYLESHEETS_PLACEHOLDER -->"
    Private Const k_htmlUIPlaceholder As String = "<!-- HTML_UI_PLACEHOLDER -->"
    Private Const k_gridJSPlaceholder As String = "// GRID_JS_PLACEHOLDER"

    Public Sub InitialiseHTMLUI(game As IASL)
        Dim scriptsHtml As String = String.Empty
        Dim stylesheetsHtml As String = String.Empty

        If game IsNot Nothing Then

            Dim scripts As IEnumerable(Of String) = game.GetExternalScripts()

            If scripts IsNot Nothing Then
                For Each script As String In scripts
                    Dim scriptURL = GetURL(script)
                    scriptsHtml += String.Format("<script type=""text/javascript"" src=""{0}""></script>", scriptURL)
                Next
            End If

            Dim stylesheets As IEnumerable(Of String) = game.GetExternalStylesheets()

            If stylesheets IsNot Nothing Then
                For Each stylesheet As String In stylesheets
                    stylesheetsHtml += String.Format("<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />", stylesheet)
                Next
            End If
        End If

        Dim htmlContent As String = System.IO.File.ReadAllText(m_baseHtmlPath)
        Dim gridJsContent As String = System.IO.File.ReadAllText(m_gridJsPath)

        htmlContent = htmlContent.Replace(k_scriptsPlaceholder, scriptsHtml)
        htmlContent = htmlContent.Replace(k_stylesheetsPlaceholder, stylesheetsHtml)
        htmlContent = htmlContent.Replace(k_htmlUIPlaceholder, PlayerHelper.GetUIHTML())
        htmlContent = htmlContent.Replace(k_gridJSPlaceholder, gridJsContent)

        m_resourceSchemeHandler.HTML = htmlContent
        WaitForBrowserInitialization()
        ctlWebView.Load("res://local/ui")
    End Sub

    Private Sub WaitForBrowserInitialization()
        If m_browserInitialized Then Return

        SyncLock m_browserInitializationLock
            System.Threading.Monitor.Wait(m_browserInitializationLock)
        End SyncLock
    End Sub

    Public Sub Finished()
        InvokeScript("gameFinished")
    End Sub

    Private Shared s_regexHtml As New System.Text.RegularExpressions.Regex("\<.+?\>")

    Private Sub StripTagsAndSendToJaws(text As String)
        text = s_regexHtml.Replace(text, "")
        text = text.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&")
        JawsApi.JawsApi.JFWSayString(text, False)
    End Sub

    Public Sub SetPanelContents(html As String)
        InvokeScript("setPanelContents", html)
    End Sub

    Public Sub UpdateLocation(location As String)
        InvokeScript("updateLocation", location)
    End Sub

    Private Shared s_elementMap As New Dictionary(Of String, String) From
    {
        {"Panes", "#gamePanes"},
        {"Location", "#location"},
        {"Command", "#txtCommandDiv"}
    }

    Public Sub DoShow(element As String)
        InvokeScript("uiShow", s_elementMap(element))
    End Sub

    Public Sub DoHide(element As String)
        InvokeScript("uiHide", s_elementMap(element))
    End Sub

    Public Sub UpdateCompass(exits As List(Of ListData))
        InvokeScript("updateCompass", String.Join("/", From e In exits Select e.Text))
    End Sub

    Public Sub UpdatePlacesObjectsList(list As List(Of ListData))
        InvokeScript("updateListEval", "placesobjects", PlayerHelper.ListDataParameter(list).GetParameter())
    End Sub

    Public Sub UpdateInventoryList(list As List(Of ListData))
        InvokeScript("updateListEval", "inventory", PlayerHelper.ListDataParameter(list).GetParameter())
    End Sub

    Public Sub SetCompassDirections(list As IEnumerable(Of String))
        InvokeScript("setCompassDirectionsEval", (New Utility.JSInterop.StringArrayParameter(list)).GetParameter())
    End Sub

    Public Sub SetInterfaceString(name As String, text As String)
        InvokeScript("setInterfaceString", name, text)
    End Sub

    Public Sub BeginWait()
        InvokeScript("beginWait")
    End Sub

    Public Sub WaitEnded()
        InvokeScript("waitEnded")
    End Sub

    Public Sub UpdateStatus(status As String)
        InvokeScript("updateStatus", status)
    End Sub

    Public Sub ShowExitFullScreenButton(show As Boolean)
        InvokeScript("showExitFullScreenButton", If(show, "true", "false"))
        ClearBuffer()
    End Sub

    Public Sub Reset()
        m_resetting = True
        ctlWebView.Load("about:blank")
    End Sub

    Private Sub ctlWebView_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles ctlWebView.PropertyChanged
        Select Case e.PropertyName
            Case "IsLoading"
                If Not ctlWebView.IsLoading Then
                    OnDocumentLoad()
                End If
            Case "IsBrowserInitialized"
                m_browserInitialized = True
                SyncLock m_browserInitializationLock
                    System.Threading.Monitor.Pulse(m_browserInitializationLock)
                End SyncLock
        End Select
    End Sub

    Private Sub m_interop_UIEventTriggered(command As String, parameter As String) Handles m_interop.UIEventTriggered
        UIEvent(command, parameter)
    End Sub

    Public Sub ShowDevTools()
        ctlWebView.ShowDevTools()
    End Sub

    Private Sub m_keyHandler_KeyPressed(code As Integer) Handles m_keyHandler.KeyPressed
        BeginInvoke(Sub() RaiseEvent ShortcutKeyPressed(CType(code, Keys)))
    End Sub
End Class
