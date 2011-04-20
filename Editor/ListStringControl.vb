<ControlType("list")> _
Public Class ListStringControl
    Implements IElementEditorControl
    Implements IResizableElementEditorControl
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
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        ctlListEditor.EditorDelegate = Me
        m_controlData = controlData
        If controlData IsNot Nothing Then
            m_attributeName = controlData.Attribute
        Else
            m_attributeName = Nothing
        End If
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

    Private Sub m_list_Added(sender As Object, e As EditableListUpdatedEventArgs(Of String)) Handles m_list.Added
        ctlListEditor.AddListItem(New KeyValuePair(Of String, String)(e.UpdatedItem.Key, e.UpdatedItem.Value), e.Index)

        If (e.Source = EditorUpdateSource.User) Then
            ctlListEditor.SetSelectedItem(e.UpdatedItem.Key)
            ctlListEditor.Focus()
        End If

        RaiseEvent Dirty(Me, New DataModifiedEventArgs(Nothing, m_list))
    End Sub

    Private Sub m_list_Removed(sender As Object, e As EditableListUpdatedEventArgs(Of String)) Handles m_list.Removed
        ctlListEditor.RemoveListItem(e.UpdatedItem.Key)
        RaiseEvent Dirty(Me, New DataModifiedEventArgs(Nothing, m_list))
    End Sub

    Public Sub DoAdd() Implements IListEditorDelegate.DoAdd
        Dim result = PopupEditors.EditString(m_controlData.GetString("editprompt"), String.Empty)
        If result.Cancelled Then Return
        If Not ValidateInput(result.Result) Then Return

        If m_list Is Nothing Then
            Value = m_controller.CreateNewEditableList(m_elementName, m_attributeName, result.Result)
        Else
            PrepareForEditing()
            m_list.Add(result.Result)
        End If
    End Sub

    Public Sub DoEdit(key As String, index As Integer) Implements IListEditorDelegate.DoEdit
        Dim result = PopupEditors.EditString(m_controlData.GetString("editprompt"), key)
        If result.Cancelled Then Return
        If result.Result = key Then Return
        If Not ValidateInput(result.Result) Then Return

        PrepareForEditing()
        m_list.Update(index, result.Result)
    End Sub

    Public Sub DoRemove(keys() As String) Implements IListEditorDelegate.DoRemove
        PrepareForEditing()
        m_list.Remove(keys)
    End Sub

    Private Function ValidateInput(input As String) As Boolean
        If m_list Is Nothing Then Return True
        Dim result = m_list.CanAdd(input)
        If result.Valid Then Return True

        MsgBox(PopupEditors.GetError(result.Message, input), MsgBoxStyle.Exclamation, "Unable to add item")
        Return False
    End Function

    Private Sub ctlListEditor_ToolbarClicked() Handles ctlListEditor.ToolbarClicked
        RaiseEvent RequestParentElementEditorSave()
    End Sub

    Private Sub PrepareForEditing()
        ' If we're currently displaying a list which belongs to a type we inherit from,
        ' we must clone the list before we can edit it.

        If m_list Is Nothing Then Return

        If m_list.Locked Then
            Value = m_list.Clone(m_elementName, m_attributeName)
        End If
    End Sub
End Class
