Imports System.Xml

Public Class PlayerHTML

    Private m_currentElement As System.Windows.Forms.HtmlElement = Nothing
    Private m_links As New Dictionary(Of String, HyperlinkType)
    Private m_style As String
    Private m_linkStyle As String
    Private m_foreground As String  ' base foreground colour
    Private m_foregroundOverride As String  ' foreground colour overridden by a <color> tag
    Private m_linkForeground As String
    Private m_bold As Boolean
    Private m_italic As Boolean
    Private m_underline As Boolean
    Private m_fontName As String = "Arial"
    Private m_fontSize As Integer = 9
    Private m_fontSizeOverride As Integer = 0

    Public Event Ready()
    Public Event CommandLinkClicked(command As String)
    Public Event SendEvent(eventName As String, param As String)

    Public Sub Setup()
        wbOutput.Navigate(My.Application.Info.DirectoryPath() & "\Blank.htm")
    End Sub

    Private Function CurrentElement() As HtmlElement
        If m_currentElement = Nothing Then
            Return wbOutput.Document.Body
        End If
        Return m_currentElement
    End Function

    Private Sub WriteText(ByVal text As String)
        ' If text starts with a space then IE won't print it.
        ' TO DO: This needs to be replaced with a regex of some kind
        If Microsoft.VisualBasic.Left(text, 1) = " " Then text = "&nbsp;" + Mid(text, 2)

        Dim newText As HtmlElement = wbOutput.Document.CreateElement("span")
        newText.Style = GetCurrentStyle(False)
        newText.InnerHtml = text
        CurrentElement.AppendChild(newText)
        newText.ScrollIntoView(True)
    End Sub

    Private Sub SetAlignment(ByVal align As String)
        If align.Length = 0 Or align = "left" Then
            m_currentElement = Nothing
        Else
            Dim newDiv As HtmlElement = wbOutput.Document.CreateElement("div")
            newDiv.Style = "text-align:" + align
            wbOutput.Document.Body.AppendChild(newDiv)
            m_currentElement = newDiv
        End If
    End Sub

    Public Sub WriteLine(ByVal text As String)
        WriteText(text + "<br />")
    End Sub

    Public Sub PrintText(text As String)
        Dim currentTag As String = ""
        Dim currentTagValue As String = ""
        Dim settings As XmlReaderSettings = New XmlReaderSettings()
        settings.IgnoreWhitespace = False
        Dim reader As XmlReader = XmlReader.Create(New System.IO.StringReader(text), settings)
        Dim newLine As Boolean = True
        Dim alignmentSet As Boolean = False
        Dim textWritten As Boolean = False

        While reader.Read()
            Select Case reader.NodeType
                Case XmlNodeType.Element
                    Select Case reader.Name
                        Case "output"
                            If reader.GetAttribute("nobr") = "true" Then
                                newLine = False
                            End If
                        Case "object"
                            currentTag = "object"
                        Case "exit"
                            currentTag = "exit"
                        Case "br"
                            WriteText("<br />")
                            textWritten = True
                        Case "b"
                            Bold = True
                        Case "i"
                            Italic = True
                        Case "u"
                            Underline = True
                        Case "color"
                            ForegroundOverride = reader.GetAttribute("color")
                        Case "font"
                            FontSizeOverride = CInt(reader.GetAttribute("size"))
                        Case "align"
                            SetAlignment(reader.GetAttribute("align"))
                        Case Else
                            Throw New Exception(String.Format("Unrecognised element '{0}'", reader.Name))
                    End Select
                Case XmlNodeType.Text
                    If Len(currentTag) = 0 Then
                        WriteText(reader.Value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"))
                        textWritten = True
                    Else
                        currentTagValue = reader.Value
                    End If
                Case XmlNodeType.Whitespace
                    WriteText(reader.Value.Replace(" ", "&nbsp;"))
                    textWritten = True
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
                        Case "color"
                            ForegroundOverride = ""
                        Case "font"
                            FontSizeOverride = 0
                        Case "align"
                            SetAlignment("")
                            alignmentSet = True
                            textWritten = False
                        Case Else
                            Throw New Exception(String.Format("Unrecognised element '{0}'", reader.Name))
                    End Select
            End Select

        End While

        If newLine Then
            ' If we have just set the text alignment but then print no more text afterwards,
            ' there's no need to submit an extra <br> tag as subsequent text will be in a
            ' brand new <div> element.

            If Not (alignmentSet And Not textWritten) Then
                WriteText("<br />")
            End If
        End If
    End Sub

    Private Sub AddLink(ByVal linkText As String, ByVal linkType As HyperlinkType)
        Dim id As String = GetUniqueID()
        Dim newLink As HtmlElement = wbOutput.Document.CreateElement("a")
        m_links.Add(id, linkType)
        newLink.Id = id
        newLink.InnerHtml = linkText
        newLink.SetAttribute("href", "javascript:")
        newLink.Style = GetCurrentStyle(True)
        CurrentElement.AppendChild(newLink)
        AddHandler newLink.Click, AddressOf InLineLinkClicked
    End Sub

    Private Sub wbOutput_DocumentCompleted(ByVal sender As Object, ByVal e As WebBrowserDocumentCompletedEventArgs) Handles wbOutput.DocumentCompleted
        RaiseEvent Ready()
    End Sub

    ' TO DO: This shouldn't need to be public...

    ' This is how we support JavaScript calling ASL functions (in WebFunctions.js - ASLEvent function).
    ' We have a hidden _ASLEvent div, and simply handle the click event on it here. In WebFunctions.js
    ' we set the InnerText of our div with the data we want to pass in, and trigger the click event using
    ' jQuery. This may be slightly clunky but it works, and it's better than using window.external in our
    ' JavaScript as that won't work under Mono.
    Public Sub AddASLEventElement()
        Dim newLink As HtmlElement = wbOutput.Document.CreateElement("div")
        newLink.Id = "_ASLEvent"
        newLink.Style = "display:none"
        CurrentElement.AppendChild(newLink)
        AddHandler newLink.Click, AddressOf HiddenCmdLinkClicked
    End Sub

    Private Sub HiddenCmdLinkClicked(ByVal sender As Object, ByVal e As EventArgs)
        Dim element As HtmlElement = DirectCast(sender, HtmlElement)
        Dim data As String = element.InnerText
        Dim args As String() = data.Split({";"c}, 2)
        RaiseEvent SendEvent(args(0), args(1))
    End Sub

    Public Sub Clear()
        m_currentElement = Nothing
        wbOutput.Document.Body.InnerHtml = ""
    End Sub

    Private Function GetCurrentStyle(ByVal link As Boolean) As String
        If Len(m_style) = 0 Then
            Dim fontSize As Integer = m_fontSize
            If m_fontSizeOverride > 0 Then fontSize = m_fontSizeOverride

            m_style = "font-family:" + m_fontName _
                + ";font-size:" + CStr(fontSize) + "pt"

            If Bold Then
                m_style += ";font-weight:bold"
            End If

            If Italic Then
                m_style += ";font-style:italic"
            End If

            ' Omit text-decoration from link style by setting it here
            m_linkStyle = m_style

            If Underline Then
                m_style += ";text-decoration:underline"
            End If

            If Len(m_foregroundOverride) > 0 Then
                m_style += ";color:" + m_foregroundOverride
            ElseIf Len(m_foreground) > 0 Then
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

    Public Property FontSizeOverride() As Integer
        Get
            Return m_fontSizeOverride
        End Get
        Set(ByVal value As Integer)
            m_fontSizeOverride = value
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

    Public Property ForegroundOverride() As String
        Get
            Return m_foregroundOverride
        End Get
        Set(ByVal value As String)
            m_foregroundOverride = value
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

    Private Function GetUniqueID() As String
        Static id As Integer
        id += 1
        Return "k" + id.ToString()
    End Function

    Public Sub ShowPicture(filename As String)
        Dim newImage As HtmlElement = wbOutput.Document.CreateElement("img")
        newImage.SetAttribute("src", filename)
        CurrentElement.AppendChild(newImage)
        newImage.ScrollIntoView(True)
    End Sub

    Public Sub Copy()
        wbOutput.Document.ExecCommand("Copy", True, Nothing)
    End Sub

    Public Sub SelectAll()
        wbOutput.Document.ExecCommand("SelectAll", True, Nothing)
    End Sub

    Public Sub InvokeScript(ByVal functionName As String, ByVal ParamArray args() As String)
        wbOutput.Document.InvokeScript(functionName, args)
    End Sub

    Private Sub InLineLinkClicked(ByVal sender As Object, ByVal e As EventArgs)

        Dim link As HtmlElement = wbOutput.Document.ActiveElement
        Dim cmd As String = ""

        ' TO DO: show menu instead
        Select Case m_links(link.Id)
            Case HyperlinkType.ObjectLink
                cmd = "look at " + link.InnerText
            Case HyperlinkType.ExitLink
                cmd = "go " + link.InnerText
        End Select

        ' Run asynchronously as otherwise we're running a command in the middle
        ' of a link click event, which means we don't scroll the window properly

        BeginInvoke(Sub()
                        RaiseEvent CommandLinkClicked(cmd)
                    End Sub)
    End Sub

End Class
