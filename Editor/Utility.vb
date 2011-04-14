Public Class Utility
    Public Shared Function FormatAsOneLine(input As String) As String
        Return input.Replace(Environment.NewLine, " / ")
    End Function
End Class
