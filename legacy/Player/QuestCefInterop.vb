Public Class QuestCefInterop
    Public Event UIEventTriggered(command As String, parameter As String)

    Public Sub UiEvent(command As String, parameter As String)
        RaiseEvent UIEventTriggered(command, parameter)
    End Sub
End Class
