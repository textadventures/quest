<ControlType("title")> _
Public Class TitleControl
    Implements ISelfCaptionedElementEditorControl

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
            Return Nothing
        End Get
        Set(value As EditorController)
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return Nothing
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If controlData IsNot Nothing Then
        Else
        End If
    End Sub

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return Nothing
        End Get
    End Property

    Public Sub SetCaption(caption As String) Implements ISelfCaptionedElementEditorControl.SetCaption
        lblTitle.Text = caption
    End Sub
End Class
