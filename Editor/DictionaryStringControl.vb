<ControlType("stringdictionary")> _
Public Class DictionaryStringControl
    Implements IElementEditorControl

    Private m_attributeName As String
    Private m_controller As EditorController
    Private WithEvents m_list As IEditableDictionary(Of String)

    Public ReadOnly Property AttributeName As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attributeName
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
        End Set
    End Property

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_controller = controller
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate

    End Sub

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save

    End Sub

    Public Property Value As Object Implements IElementEditorControl.Value
        Get
            Return Nothing
        End Get
        Set(value As Object)

        End Set
    End Property

End Class
