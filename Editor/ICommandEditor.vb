Public Interface ICommandEditor

    Sub SaveData()

    Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)

End Interface
