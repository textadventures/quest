Public Class Utility
    Public Shared Function FormatAsOneLine(input As String) As String
        If input Is Nothing Then Return Nothing
        Return input.Replace(Environment.NewLine, " / ")
    End Function
End Class
