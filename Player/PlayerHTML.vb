Imports System.Xml

Public Class PlayerHTML

    Public Event Ready()
    Public Event CommandRequested(command As String)
    Public Event SendEvent(eventName As String, param As String)

    Private m_baseHtmlPath As String = My.Application.Info.DirectoryPath() & "\Blank.htm"
    Private m_deleteFile As String = Nothing
    Private m_navigationAllowed As Boolean = True

    Public Sub Setup()
        m_navigationAllowed = True
        wbOutput.Navigate(m_baseHtmlPath)
    End Sub

    Public Sub WriteText(text As String)
        InvokeScript("addText", text)
    End Sub

    Public Sub SetAlignment(align As String)
        If align.Length = 0 Then align = "left"
        InvokeScript("createNewDiv", align)
    End Sub

    Public Sub BindMenu(linkid As String, verbs As String, text As String)
        InvokeScript("bindMenu", linkid, verbs, text)
    End Sub

    Public Sub WriteLine(text As String)
        WriteText(text + "<br />")
    End Sub

    Private Sub wbOutput_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles wbOutput.DocumentCompleted
        AddUIEventElement()
        DeleteTempFile()
        RaiseEvent Ready()
    End Sub

    ' This is how we support JavaScript calling ASL functions (ASLEvent function) and other code in desktop Player.
    ' We have a hidden _UIEvent div, and simply handle the click event on it here. In desktopplayer.js
    ' we set the InnerText of our div with the data we want to pass in, and trigger the click event using
    ' jQuery. This may be slightly clunky but it works, and it's better than using window.external in our
    ' JavaScript as that won't work under Mono.
    Private Sub AddUIEventElement()
        Dim newLink As HtmlElement = wbOutput.Document.CreateElement("div")
        newLink.Id = "_UIEvent"
        newLink.Style = "display:none"
        wbOutput.Document.Body.AppendChild(newLink)
        AddHandler newLink.Click, AddressOf HiddenCmdLinkClicked
    End Sub

    Private Sub DeleteTempFile()
        If m_deleteFile Is Nothing Then Exit Sub
        System.IO.File.Delete(m_deleteFile)
        m_deleteFile = Nothing
    End Sub

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
        End Select

    End Sub

    Private Sub RunASLEvent(data As String)
        Dim args As String() = data.Split({";"c}, 2)
        RaiseEvent SendEvent(args(0), args(1))
    End Sub

    Private Sub RunCommand(data As String)
        RaiseEvent CommandRequested(data)
    End Sub

    Public Sub Clear()
        InvokeScript("clearScreen", "")
    End Sub

    Public Sub ShowPicture(filename As String)
        WriteText(String.Format("<img src=""{0}"" onload=""scrollToEnd();"" /><br />", filename))
    End Sub

    Public Sub Copy()
        wbOutput.Document.ExecCommand("Copy", True, Nothing)
    End Sub

    Public Sub SelectAll()
        wbOutput.Document.ExecCommand("SelectAll", True, Nothing)
    End Sub

    Public Sub InvokeScript(functionName As String, ParamArray args() As String)
        wbOutput.Document.InvokeScript(functionName, args)
    End Sub

    Public Sub SetBackground(colour As String)
        InvokeScript("SetBackground", colour)
    End Sub

    Private Const k_scriptsPlaceholder As String = "<!-- EXTERNAL_SCRIPTS_PLACEHOLDER -->"

    Public Sub InitialiseScripts(scripts As IEnumerable(Of String))
        ' Construct an HTML page based on the default Blank.htm, but with additional <script> tags
        ' for each external Javascript file this game wants to use.

        Dim htmlPath As String = System.IO.Path.GetTempFileName

        Dim scriptsHtml As String = String.Empty
        For Each script As String In scripts
            scriptsHtml += String.Format("<script type=""text/javascript"" src=""{0}""></script>", script)
        Next

        Dim htmlContent As String = System.IO.File.ReadAllText(m_baseHtmlPath)

        ' The <script> src for the default scripts in Blank.htm need to be remapped so they are picked up
        ' from the Quest app path, not the temp folder. The same applies for stylesheet hrefs.
        InsertFolderName(htmlContent, "src")
        InsertFolderName(htmlContent, "href")

        ' Now we can insert the custom <script> elements
        htmlContent = htmlContent.Replace(k_scriptsPlaceholder, scriptsHtml)

        ' Write our customised html file to the temp folder
        System.IO.File.WriteAllText(htmlPath, htmlContent)

        wbOutput.Navigate(htmlPath)

        m_deleteFile = htmlPath
    End Sub

    Private Sub InsertFolderName(ByRef content As String, attribute As String)
        content = content.Replace(attribute + "=""", attribute + "=""" + My.Application.Info.DirectoryPath() + "\")
    End Sub

    Public Sub Finished()
        InvokeScript("gameFinished")
    End Sub

    Private Sub wbOutput_Navigating(sender As Object, e As System.Windows.Forms.WebBrowserNavigatingEventArgs) Handles wbOutput.Navigating
        If Not m_navigationAllowed Then
            e.Cancel = True
        End If
    End Sub

    Public Sub DisableNavigation()
        m_navigationAllowed = False
    End Sub
End Class
