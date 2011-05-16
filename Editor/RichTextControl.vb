Imports mshtml

<ControlType("richtext")> _
Public Class RichTextControl
    Implements IElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_attribute As String
    Private m_attributeName As String
    Private m_data As IEditorData
    Private m_nullable As Boolean
    Private m_document As IHTMLDocument2

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
            SetValue(value)
        End Set
    End Property

    Protected Overridable Function GetValue() As Object
        'If (m_nullable AndAlso txtTextBox.Text.Length = 0) Then Return Nothing
        'Return txtTextBox.Text
        Return String.Empty
    End Function

    Protected Overridable Sub SetValue(value As Object)
        Dim stringValue As String = TryCast(value, String)
        If stringValue IsNot Nothing Then
            '    txtTextBox.Text = stringValue
        Else
            '    txtTextBox.Text = String.Empty
        End If
        m_oldValue = stringValue
    End Sub

    'Private Sub TextBoxControl_Leave(sender As Object, e As System.EventArgs) Handles txtTextBox.Leave
    '    Save(m_data)
    'End Sub

    'Private Sub TextBoxControl_TextChanged(sender As Object, e As System.EventArgs) Handles txtTextBox.TextChanged
    '    If IsDirty Then
    '        RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, txtTextBox.Text))
    '    End If
    'End Sub

    Public ReadOnly Property IsDirty() As Boolean
        Get
            'Return txtTextBox.Text <> m_oldValue
            Return False
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
        SaveData(data)
    End Sub

    Protected Overridable Sub SaveData(data As IEditorData)
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
            m_nullable = controlData.GetBool("nullable")
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

    Private Sub ctlWebBrowser_DocumentCompleted(sender As Object, e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles ctlWebBrowser.DocumentCompleted
        ctlWebBrowser.Document.Write(ctlWebBrowser.DocumentText)
        m_document.designMode = "On"
    End Sub

    Private Sub ctlWebBrowser_GotFocus(sender As Object, e As System.EventArgs) Handles ctlWebBrowser.GotFocus
        If ctlWebBrowser.Document IsNot Nothing AndAlso ctlWebBrowser.Document.Body IsNot Nothing Then
            ctlWebBrowser.Document.Body.Focus()
        End If
    End Sub

    Private Sub SetUpBrowser()
        ctlWebBrowser.DocumentText = "<html><body></body></html>"
        m_document = DirectCast(ctlWebBrowser.Document.DomDocument, IHTMLDocument2)
        m_document.designMode = "On"
    End Sub

End Class