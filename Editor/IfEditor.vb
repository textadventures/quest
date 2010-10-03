Public Class IfEditor

    Implements ICommandEditor

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub SaveData() Implements ICommandEditor.SaveData

    End Sub
End Class
