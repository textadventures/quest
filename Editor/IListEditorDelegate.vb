Public Interface IListEditorDelegate
    Sub DoAdd()
    Sub DoEdit(key As String, index As Integer)
    Sub DoRemove(keys() As String)
End Interface
