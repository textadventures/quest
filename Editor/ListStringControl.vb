<ControlType("list")> _
Public Class ListStringControl
    Implements IElementEditorControl
    Implements IListEditorDelegate

    Private m_attributeName As String
    Private m_elementName As String
    Private m_controller As EditorController
    Private m_data As IEditorData
    Private m_controlData As IEditorControl

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

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        ctlListEditor.EditorDelegate = Me
        m_controlData = controlData
        m_attributeName = controlData.Attribute
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        If data IsNot Nothing Then
            Value = data.GetAttribute(m_attributeName)
            m_elementName = data.Name
        Else
            Value = Nothing
            m_elementName = Nothing
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
            ctlListEditor.UpdateList(If(m_list Is Nothing, Nothing, m_list.DisplayItems))
        End Set
    End Property

    Private Structure EditItemResult
        Public Cancelled As Boolean
        Public Result As String
    End Structure

    Private Function EditItem(input As String) As EditItemResult
        Dim result As New EditItemResult
        Dim inputResult = InputBox(m_controlData.GetString("editprompt"), DefaultResponse:=input)
        result.Cancelled = (inputResult.Length = 0)
        result.Result = inputResult
        Return result
    End Function

    Private Sub m_list_Added(sender As Object, e As EditableListUpdatedEventArgs(Of String)) Handles m_list.Added
        ctlListEditor.AddListItem(New KeyValuePair(Of String, String)(e.UpdatedItem.Key, e.UpdatedItem.Value), e.Index)

        If (e.Source = EditorUpdateSource.User) Then
            ctlListEditor.SetSelectedItem(e.UpdatedItem.Key)
            ctlListEditor.Focus()
        End If
    End Sub

    Private Sub m_list_Removed(sender As Object, e As EditableListUpdatedEventArgs(Of String)) Handles m_list.Removed
        ctlListEditor.RemoveListItem(e.UpdatedItem, e.Index)
    End Sub

    Public Sub DoAdd() Implements IListEditorDelegate.DoAdd
        Dim result = EditItem(String.Empty)
        If result.Cancelled Then Return

        If m_list Is Nothing Then
            Value = m_controller.CreateNewEditableList(m_elementName, m_attributeName, result.Result)
        Else
            m_list.Add(result.Result)
        End If
    End Sub

    Public Sub DoEdit(key As String, index As Integer) Implements IListEditorDelegate.DoEdit
        Dim result = EditItem(key)
        If result.Cancelled Then Return

        m_list.Update(index, result.Result)
    End Sub

    Public Sub DoRemove(keys() As String) Implements IListEditorDelegate.DoRemove
        m_list.Remove(keys)
    End Sub
End Class
