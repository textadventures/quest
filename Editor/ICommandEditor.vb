Public Interface ICommandEditor

    Sub SaveData()

    Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)
    Property Controller() As EditorController
    Sub UpdateField(ByVal attribute As String, ByVal newValue As Object, ByVal setFocus As Boolean)

End Interface
