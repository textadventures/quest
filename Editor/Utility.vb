Public Class Utility
    Public Structure EditStringResult
        Public Cancelled As Boolean
        Public Result As String
    End Structure

    Private Shared s_validationMessages As New Dictionary(Of ValidationMessage, String) From {
        {ValidationMessage.OK, "No error"},
        {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"}
    }

    Public Shared Function EditString(prompt As String, defaultResult As String) As EditStringResult
        Dim result As New EditStringResult
        Dim inputResult = InputBox(prompt + ":", DefaultResponse:=defaultResult)
        result.Cancelled = (inputResult.Length = 0)
        result.Result = inputResult
        Return result
    End Function

    Public Shared Function GetError(validationMessage As ValidationMessage, item As String) As String
        Return String.Format(s_validationMessages(validationMessage), item)
    End Function
End Class
