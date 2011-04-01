Imports System.Xml

Public Class PlayerHTML

    Public Event Ready()
    Public Event CommandRequested(command As String)
    Public Event SendEvent(eventName As String, param As String)

    Public Sub Setup()
        wbOutput.Navigate(My.Application.Info.DirectoryPath() & "\Blank.htm")
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

End Class
