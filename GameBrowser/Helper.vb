Public Class Helper
    Friend Shared Sub OutputStars(target As System.Windows.Documents.Run, value As Integer)
        If IsXP() Then
            ' XP doesn't have the Unicode star in the standard font, so use Wingdings
            target.FontFamily = New System.Windows.Media.FontFamily("Wingdings")
            target.Text = New String("«"c, CInt(value))
        Else
            target.Text = New String("★"c, CInt(value))
        End If
    End Sub

    Private Shared Function IsXP() As Boolean
        Return System.Environment.OSVersion.Platform = PlatformID.Win32NT AndAlso System.Environment.OSVersion.Version.Major < 6
    End Function
End Class
