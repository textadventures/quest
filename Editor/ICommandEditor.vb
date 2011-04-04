Public Interface ICommandEditor

    Sub SaveData()

    Event Dirty(sender As Object, args As DataModifiedEventArgs)
    Property Controller() As EditorController
    Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean)

End Interface
