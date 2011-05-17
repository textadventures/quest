<ControlType("file")> _
Public Class FileControl
    Implements IElementEditorControl

    Private m_controller As EditorController
    Private m_attribute As String
    Private m_data As IEditorData

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public ReadOnly Property AttributeName As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public ReadOnly Property Control As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Controller As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            ctlDropDown.Controller = value
        End Set
    End Property

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return GetType(String)
        End Get
    End Property

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_controller = controller
        ctlDropDown.Initialise(controller, controlData)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        ctlDropDown.Populate(data)
    End Sub

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        ctlDropDown.Save(data)
    End Sub

    Public Property Value As Object Implements IElementEditorControl.Value
        Get
            Return ctlDropDown.Value
        End Get
        Set(value As Object)
            ctlDropDown.Value = value
        End Set
    End Property
End Class
