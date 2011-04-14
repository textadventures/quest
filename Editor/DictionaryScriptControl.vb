<ControlType("scriptdictionary")> _
Public Class DictionaryScriptControl
    Implements IElementEditorControl
    Implements IListEditorDelegate

    Private m_attributeName As String
    Private m_elementName As String
    Private m_controller As EditorController
    Private WithEvents m_list As IEditableDictionary(Of IEditableScripts)
    Private m_data As IEditorData
    Private m_controlData As IEditorControl

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
        ctlListEditor.Style = ListEditor.ColumnStyle.TwoColumns
        ctlListEditor.SetHeader(1, "Key")
        ctlListEditor.SetHeader(2, "Script")
        ctlListEditor.UpdateList(Nothing)
        m_controller = controller
        m_attributeName = controlData.Attribute
        m_controlData = controlData
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
            m_list = DirectCast(value, IEditableDictionary(Of IEditableScripts))
            ctlListEditor.UpdateList(If(m_list Is Nothing, Nothing, m_list.DisplayItems))
        End Set
    End Property

    Public Sub DoAdd() Implements IListEditorDelegate.DoAdd
        Dim addKey = PopupEditors.EditString(m_controlData.GetString("keyprompt"), String.Empty)
        If addKey.Cancelled Then Return
        If Not ValidateInput(addKey.Result) Then Return


        ' create new editablescripts from the controller
        ' listen to updates on it - update list value with new displaystring


        'Dim addValue = Utility.EditString(m_controlData.GetString("valueprompt"), String.Empty)
        'If addValue.Cancelled Then Return

        'If m_list Is Nothing Then
        '    Value = m_controller.CreateNewEditableStringDictionary(m_elementName, m_attributeName, addKey.Result, addValue.Result)
        'Else
        '    m_list.Add(addKey.Result, addValue.Result)
        'End If
    End Sub

    Public Sub DoEdit(key As String, index As Integer) Implements IListEditorDelegate.DoEdit

        Dim script As IEditableScripts = m_list(key)
        PopupEditors.EditScript(m_controller, script, m_attributeName, m_elementName)

        If (m_list(key) IsNot script) Then
            ' script has been created so need to set the field value
        End If

        'Dim result = Utility.EditString(m_controlData.GetString("valueprompt"), m_list(key))
        'If result.Cancelled Then Return
        'If result.Result = m_list(key) Then Return

        'm_list.Update(key, result.Result)
    End Sub

    Public Sub DoRemove(keys() As String) Implements IListEditorDelegate.DoRemove
        m_list.Remove(keys)
    End Sub

    Private Function ValidateInput(input As String) As Boolean
        If m_list Is Nothing Then Return True
        Dim result = m_list.CanAdd(input)
        If result.Valid Then Return True

        MsgBox(PopupEditors.GetError(result.Message, input), MsgBoxStyle.Exclamation, "Unable to add item")
        Return False
    End Function

    Private Sub m_list_Added(sender As Object, e As EditableListUpdatedEventArgs(Of IEditableScripts)) Handles m_list.Added
        'ctlListEditor.AddListItem(New KeyValuePair(Of String, String)(e.UpdatedItem.Key, e.UpdatedItem.Value), e.Index)

        'If (e.Source = EditorUpdateSource.User) Then
        '    ctlListEditor.SetSelectedItem(e.UpdatedItem.Key)
        '    ctlListEditor.Focus()
        'End If
    End Sub

    Private Sub m_list_Removed(sender As Object, e As EditableListUpdatedEventArgs(Of IEditableScripts)) Handles m_list.Removed
        'ctlListEditor.RemoveListItem(e.UpdatedItem, e.Index)
    End Sub

    Private Sub m_list_Updated(sender As Object, e As EditableListUpdatedEventArgs(Of IEditableScripts)) Handles m_list.Updated
        Dim updatedIndex = e.Index
        Dim newDisplayString = e.UpdatedItem.Value.DisplayString
        ctlListEditor.UpdateValue(updatedIndex, newDisplayString)
    End Sub
End Class
