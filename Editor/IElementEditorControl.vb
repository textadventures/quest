Public Class DataModifiedEventArgs
    Private m_oldValue As Object
    Private m_newValue As Object
    Private m_attribute As String

    Public Sub New(oldValue As Object, newValue As Object)
        m_oldValue = oldValue
        m_newValue = newValue
    End Sub

    Public ReadOnly Property OldValue() As Object
        Get
            Return m_oldValue
        End Get
    End Property

    Public ReadOnly Property NewValue() As Object
        Get
            Return m_newValue
        End Get
    End Property

    Public Property Attribute() As String
        Get
            Return m_attribute
        End Get
        Set(value As String)
            m_attribute = value
        End Set
    End Property
End Class

Public Interface IElementEditorControl

    Event Dirty(sender As Object, args As DataModifiedEventArgs)

    ReadOnly Property Control() As Control
    Property Value() As Object
    Property Controller() As EditorController
    Sub Initialise(controller As EditorController, controlData As IEditorControl)
    ReadOnly Property AttributeName() As String
    Sub Populate(data As IEditorData)
    Sub Save(data As IEditorData)

End Interface