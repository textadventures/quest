Public Class ListEditor
    Private m_listItems As New Dictionary(Of String, ListViewItem)

    Private m_delegate As IListEditorDelegate

    Public Sub UpdateList(list As IEnumerable(Of KeyValuePair(Of String, String)))
        lstList.Clear()
        m_listItems.Clear()
        Dim mainColumn As New ColumnHeader
        lstList.Columns.Add(mainColumn)
        mainColumn.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize)
        lstList.HeaderStyle = ColumnHeaderStyle.None
        Dim index As Integer = 0

        If list IsNot Nothing Then
            For Each item In list
                AddListItem(item, index)
                index += 1
            Next
        End If

        SetButtonsEnabledStatus()
    End Sub

    Public Sub AddListItem(item As KeyValuePair(Of String, String), index As Integer)
        Dim newListViewItem As ListViewItem = lstList.Items.Insert(index, item.Value)
        m_listItems.Add(item.Key, newListViewItem)
    End Sub

    Public Sub RemoveListItem(item As IEditableListItem(Of String), index As Integer)
        lstList.Items.Remove(m_listItems(item.Key))
        m_listItems.Remove(item.Key)
    End Sub

    Private Sub ListControl_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        lstList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)
    End Sub

    Private Sub cmdAdd_Click(sender As System.Object, e As System.EventArgs) Handles cmdAdd.Click
        m_delegate.DoAdd()
    End Sub

    Private Sub cmdDelete_Click(sender As Object, e As System.EventArgs) Handles cmdDelete.Click
        m_delegate.DoRemove(GetSelectedItems().ToArray)
    End Sub

    Private Sub cmdEdit_Click(sender As System.Object, e As System.EventArgs) Handles cmdEdit.Click
        EditSelectedItem()
    End Sub

    Private Sub EditSelectedItem()
        m_delegate.DoEdit(lstList.SelectedItems(0).Text, lstList.SelectedItems(0).Index)
    End Sub

    Private Function GetSelectedItems() As List(Of String)
        Dim result As New List(Of String)
        For Each item As ListViewItem In lstList.SelectedItems
            result.Add(item.Text)
        Next
        Return result
    End Function

    Private Structure EditItemResult
        Public Cancelled As Boolean
        Public Result As String
    End Structure

    Private Sub lstList_DoubleClick(sender As Object, e As System.EventArgs) Handles lstList.DoubleClick
        If IsEditAllowed() Then EditSelectedItem()
    End Sub

    Private Sub lstList_ItemSelectionChanged(sender As Object, e As System.Windows.Forms.ListViewItemSelectionChangedEventArgs) Handles lstList.ItemSelectionChanged
        SetButtonsEnabledStatus()
    End Sub

    Private Sub lstList_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstList.SelectedIndexChanged
        SetButtonsEnabledStatus()
    End Sub

    Private Sub SetButtonsEnabledStatus()
        cmdEdit.Enabled = IsEditAllowed()
        cmdDelete.Enabled = IsDeleteAllowed()
    End Sub

    Private Function IsEditAllowed() As Boolean
        Return lstList.SelectedItems.Count = 1
    End Function

    Private Function IsDeleteAllowed() As Boolean
        Return lstList.SelectedItems.Count > 0
    End Function

    Public Property EditorDelegate As IListEditorDelegate
        Get
            Return m_delegate
        End Get
        Set(value As IListEditorDelegate)
            m_delegate = value
        End Set
    End Property

    Public Sub SetSelectedItem(key As String)
        lstList.SelectedItems.Clear()
        m_listItems(key).Selected = True
        m_listItems(key).Focused = True
        m_listItems(key).EnsureVisible()
        lstList.Focus()
    End Sub

End Class
