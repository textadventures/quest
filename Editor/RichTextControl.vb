Imports mshtml

<ControlType("richtext")> _
Public Class RichTextControl
    Implements IElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_attribute As String
    Private m_attributeName As String
    Private m_data As IEditorData
    Private m_document As IHTMLDocument2

    Private Shared s_htmlToXml As New Dictionary(Of String, String) From {
        {"strong", "b"},
        {"em", "i"},
        {"b", "b"},
        {"i", "i"},
        {"u", "u"},
        {"br", "br"}
    }

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetUpBrowser()
    End Sub

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return GetValue()
        End Get
        Set(value As Object)
            Dim stringValue As String = TryCast(value, String)
            If stringValue IsNot Nothing Then
                HTML = ConvertXMLtoHTML(stringValue)
            Else
                HTML = String.Empty
            End If
            m_oldValue = stringValue
        End Set
    End Property

    Private Function GetValue() As String
        Return ConvertHTMLtoXML(HTML)
    End Function

    Public ReadOnly Property IsDirty() As Boolean
        Get
            Return GetValue() <> m_oldValue
        End Get
    End Property

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        If IsDirty Then
            Dim description As String = String.Format("Set {0} to '{1}'", m_attributeName, Value)
            m_controller.StartTransaction(description)
            data.SetAttribute(AttributeName, Value)
            m_controller.EndTransaction()
            ' reset the dirty flag
            Value = Value
            Debug.Assert(Not IsDirty)
        End If
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        PopulateData(data)
    End Sub

    Protected Overridable Sub PopulateData(data As IEditorData)
        If m_data Is Nothing Then
            Value = String.Empty
        Else
            Value = data.GetAttribute(m_attribute)
        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If controlData IsNot Nothing Then
            m_attribute = controlData.Attribute
            m_attributeName = controlData.Caption
        Else
            m_attribute = Nothing
            m_attributeName = Nothing
        End If
    End Sub

    Public Sub Initialise(attributeName As String)
        m_attribute = attributeName
        m_attributeName = attributeName
    End Sub

    Public Overridable ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return GetType(String)
        End Get
    End Property

    Protected Property OldValue As String
        Get
            Return m_oldValue
        End Get
        Set(value As String)
            m_oldValue = value
        End Set
    End Property

    Private Sub ctlWebBrowser_Navigated(sender As Object, e As System.Windows.Forms.WebBrowserNavigatedEventArgs) Handles ctlWebBrowser.Navigated
        AddHandler ctlWebBrowser.Document.Body.KeyDown, AddressOf Document_KeyDown
    End Sub

    Private Sub ctlWebBrowser_GotFocus(sender As Object, e As System.EventArgs) Handles ctlWebBrowser.GotFocus
        If ctlWebBrowser.Document IsNot Nothing AndAlso ctlWebBrowser.Document.Body IsNot Nothing Then
            ctlWebBrowser.Document.Body.Focus()
        End If
    End Sub

    Private Sub ctlWebBrowser_LostFocus(sender As Object, e As System.EventArgs) Handles ctlWebBrowser.LostFocus
        Save(m_data)
    End Sub

    Private Sub SetUpBrowser()
        ctlWebBrowser.DocumentText = "<html><body></body></html>"
        m_document = DirectCast(ctlWebBrowser.Document.DomDocument, IHTMLDocument2)
        m_document.designMode = "On"
    End Sub

    Private Property HTML As String
        Get
            If ctlWebBrowser.Document IsNot Nothing AndAlso ctlWebBrowser.Document.Body IsNot Nothing Then
                Return ctlWebBrowser.Document.Body.InnerHtml
            Else
                Return String.Empty
            End If
        End Get
        Set(value As String)
            If ctlWebBrowser.Document IsNot Nothing Then
                ctlWebBrowser.Document.Body.InnerHtml = value
            End If
        End Set
    End Property

    Private Function ConvertXMLtoHTML(input As String) As String
        Return input
    End Function

    Private Function ConvertHTMLtoXML(input As String) As String

        If String.IsNullOrEmpty(input) Then Return String.Empty

        Dim pos As Integer = 0
        Dim finished As Boolean = False
        Dim result As String = String.Empty

        Do
            Dim nextTagStart As Integer = input.IndexOf("<"c, pos)
            If nextTagStart = -1 Then
                finished = True
                result += input.Substring(pos)
            Else
                result += input.Substring(pos, nextTagStart - pos)
                Dim nextTagEnd As Integer = input.IndexOf(">"c, nextTagStart)
                Dim thisTag As String = input.Substring(nextTagStart + 1, nextTagEnd - nextTagStart - 1).ToLower()
                Dim endTag As Boolean = False

                If thisTag.Substring(0, 1) = "/" Then
                    endTag = True
                    thisTag = thisTag.Substring(1)
                End If

                If thisTag = "p" Then
                    If result.Length > 0 Then
                        result += "<br/><br/>"
                    End If
                Else
                    If Not s_htmlToXml.ContainsKey(thisTag) Then
                        ' TO DO: We will want to just ignore unrecognised tags as you could paste any HTML into the box
                        Throw New Exception(String.Format("Unrecognised HTML tag: ", thisTag))
                    End If

                    result += "<"

                    If endTag Then
                        result += "/"
                    End If

                    result += s_htmlToXml(thisTag)

                    result += ">"
                End If

                pos = nextTagEnd + 1
            End If
        Loop Until finished

        Return result
    End Function

    Private Sub Document_KeyDown(sender As Object, e As System.Windows.Forms.HtmlElementEventArgs)
        If IsDirty Then
            RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, GetValue()))
        End If
    End Sub

    Private Sub Bold()
        ctlWebBrowser.Document.ExecCommand("Bold", False, Nothing)
    End Sub

    Private Sub Italic()
        ctlWebBrowser.Document.ExecCommand("Italic", False, Nothing)
    End Sub

    Private Sub Underline()
        ctlWebBrowser.Document.ExecCommand("Underline", False, Nothing)
    End Sub

    Private Sub butBold_Click(sender As System.Object, e As System.EventArgs) Handles butBold.Click
        Bold()
    End Sub

    Private Sub butItalic_Click(sender As System.Object, e As System.EventArgs) Handles butItalic.Click
        Italic()
    End Sub

    Private Sub butUnderline_Click(sender As System.Object, e As System.EventArgs) Handles butUnderline.Click
        Underline()
    End Sub
End Class