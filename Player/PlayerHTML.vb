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

    Private m_baseHtmlPath As String = My.Application.Info.DirectoryPath() & "\Blank.htm"
    Private m_navigationAllowed As Boolean = True
    Private m_buffer As New List(Of Action)
    Private m_resetting As Boolean = False
    Private ctlWebView As WebView
    Private m_schemeHandler As CefSchemeHandlerFactory

    Private Sub PlayerHTML_Load(sender As Object, e As EventArgs) Handles Me.Load
        ctlWebView = New WebView()
        ctlWebView.Dock = DockStyle.Fill
        Controls.Add(ctlWebView)
        ctlWebView.CreateControl()

        m_schemeHandler = New CefSchemeHandlerFactory()

        CEF.Initialize(New Settings)
        CEF.RegisterScheme("quest", m_schemeHandler)
        CEF.RegisterScheme("res", New CefResourceSchemeHandlerFactory())
    End Sub

    Public Sub Setup()
        m_navigationAllowed = True
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

    'Private Sub wbOutput_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs)
    '    If m_resetting Then
    '        m_resetting = False
    '        Return
    '    End If
    '    wbOutput.ScriptErrorsSuppressed = False
    '    AddHandler wbOutput.Document.Window.Error, AddressOf wbOutput_Error
    '    AddUIEventElement()
    '    DeleteTempFile()
    '    RaiseEvent Ready()
    'End Sub

    'Private Sub wbOutput_Error(sender As Object, e As HtmlElementErrorEventArgs)
    '    Dim displayError As Boolean = True
    '    If m_internetExplorerVersion < 9 AndAlso e.Description.Contains("HTMLCanvasElement") Then
    '        displayError = False
    '    End If
    '    If displayError Then
    '        WriteText(String.Format("JavaScript error at line {0}: {1}", e.LineNumber, e.Description))
    '    End If
    '    e.Handled = True
    'End Sub

    '' This is how we support JavaScript calling ASL functions (ASLEvent function) and other code in desktop Player.
    '' We have a hidden _UIEvent div, and simply handle the click event on it here. In desktopplayer.js
    '' we set the InnerText of our div with the data we want to pass in, and trigger the click event using
    '' jQuery. This may be slightly clunky but it works, and it's better than using window.external in our
    '' JavaScript as that won't work under Mono.
    'Private Sub AddUIEventElement()
    '    Dim newLink As HtmlElement = wbOutput.Document.CreateElement("div")
    '    newLink.Id = "_UIEvent"
    '    newLink.Style = "display:none"
    '    wbOutput.Document.Body.AppendChild(newLink)
    '    AddHandler newLink.Click, AddressOf HiddenCmdLinkClicked
    'End Sub

    Private Sub HiddenCmdLinkClicked(sender As Object, e As EventArgs)
        Dim element As HtmlElement = DirectCast(sender, HtmlElement)
        Dim data() As String = element.InnerText.Split({" "c}, 2)
        Dim cmd As String = data(0)
        Dim args As String = data(1)

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

    Public Sub ShowPicture(filename As String)
        WriteText(String.Format("<img src=""{0}"" onload=""scrollToEnd();"" /><br />", filename))
    End Sub

    Public Sub Copy()
        'wbOutput.Document.ExecCommand("Copy", True, Nothing)
    End Sub

    Public Sub SelectAll()
        'wbOutput.Document.ExecCommand("SelectAll", True, Nothing)
    End Sub

    Public Sub InvokeScript(functionName As String, ParamArray args() As String)
        'm_buffer.Add(Sub() wbOutput.Document.InvokeScript(functionName, args))
    End Sub

    Public Sub SetBackground(colour As String)
        InvokeScript("SetBackground", colour)
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
    Private Const k_htmlUIPlaceholder As String = "<!-- HTML_UI_PLACEHOLDER -->"

    Public Sub InitialiseHTMLUI(scripts As IEnumerable(Of String))
        ' Construct an HTML page based on the default Blank.htm, but with additional <script> tags
        ' for each external Javascript file this game wants to use.

        Dim scriptsHtml As String = String.Empty
        If scripts IsNot Nothing Then
            For Each script As String In scripts
                scriptsHtml += String.Format("<script type=""text/javascript"" src=""{0}""></script>", script)
            Next
        End If

        Dim htmlContent As String = System.IO.File.ReadAllText(m_baseHtmlPath)

        ' Now we can insert the custom <script> elements
        htmlContent = htmlContent.Replace(k_scriptsPlaceholder, scriptsHtml)

        htmlContent = htmlContent.Replace(k_htmlUIPlaceholder, PlayerHelper.GetUIHTML())

        m_schemeHandler.HTML = htmlContent
        ctlWebView.Load("quest://local")

        'ctlWebView.ShowDevTools()
    End Sub

    Public Sub Finished()
        InvokeScript("gameFinished")
    End Sub

    Private Sub wbOutput_Navigating(sender As Object, e As System.Windows.Forms.WebBrowserNavigatingEventArgs)
        If Not m_navigationAllowed Then
            e.Cancel = True
        End If
    End Sub

    Public Sub DisableNavigation()
        m_navigationAllowed = False
    End Sub

    'Private Sub wbOutput_PreviewKeyDown(sender As Object, e As System.Windows.Forms.PreviewKeyDownEventArgs)
    '    ' With WebBrowserShortcutsEnabled = False, *all* shortcut keys are suppressed, not just webbrowser ones.
    '    ' So to enable Quest menu shortcut keys to work, we handle them here.
    '    RaiseEvent ShortcutKeyPressed(e.KeyData)
    'End Sub

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
        m_navigationAllowed = True
        m_resetting = True
        'wbOutput.Navigate("about:blank")
    End Sub

End Class
