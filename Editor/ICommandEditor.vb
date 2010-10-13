Public Interface ICommandEditor

    Sub SaveData()

    Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)
    Property Controller() As EditorController
    'Sub Populate(ByVal data As IEditorData)

End Interface
