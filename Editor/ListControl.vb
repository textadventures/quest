<ControlType("list")> _
Public Class ListControl
    Implements IElementEditorControl

    Private m_attributeName As String
    Private m_controller As EditorController
    Private m_data As IEditorData

    ' TO DO: Can we make this generic so we can edit lists of any type??
    Private WithEvents m_list As IEditableList(Of String)

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

    Public Sub Initialise(controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_attributeName = controlData.Attribute
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        If data IsNot Nothing Then
            Value = data.GetAttribute(m_attributeName)
        Else
            Value = Nothing
        End If
    End Sub

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save

    End Sub

    Public Property Value As Object Implements IElementEditorControl.Value
        Get
            Return m_list
        End Get
        Set(value As Object)
            m_list = DirectCast(value, IEditableList(Of String))
            UpdateList()
        End Set
    End Property

    Private Sub UpdateList()
        lstList.Clear()
        lstList.Columns.Add("Name")
        For Each item In m_list.Items
            lstList.Items.Add(item.Value.Value)
        Next
    End Sub
End Class
