<Microsoft.VisualBasic.ComClass()> Public Class WebScripting
    Friend Event WebEvent(ByVal name As String, ByVal param As String)

    Public Sub Trigger(ByVal name As String, ByVal param As String)
        RaiseEvent WebEvent(name, param)
    End Sub

End Class
