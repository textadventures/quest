<ControlType("attributes")> _
Public Class AttributesControl
    Implements IElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return Nothing
        End Get
        Set(value As Object)
        End Set
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
            ' Hmm, we're going to be editing ALL attributes in here. I think this is only really read by ElementEditor
            ' in UpdateField.
            Return Nothing
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        If data Is Nothing Then

        Else

        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If controlData IsNot Nothing Then
            'ctlMultiControl.Initialise(controller, 
        Else
        End If

        ctlMultiControl.InitialiseForAttributesEditor()
    End Sub

End Class
