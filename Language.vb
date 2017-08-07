Imports Microsoft.VisualBasic

'----------------------------------------------------------------------------------------------------
'Added by SoonGames
'Set the language
'----------------------------------------------------------------------------------------------------
Public Class Language

    Public LanguageIndex As Integer

    Public Function GetLanguageIndex() As Integer
        Return LanguageIndex
    End Function

    Public Sub SetLanguageIndex(AutoPropertyValue As Integer)
        LanguageIndex = AutoPropertyValue
    End Sub

    Public Sub SetLanguage()
        '
        'Use SystemLanguage
        '
        If (My.Settings.Language = "") Then
            My.Settings.Language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName
        End If
        Select Case (My.Settings.Language)
            Case "en"
                SetLanguageIndex(0)
            Case "de"
                SetLanguageIndex(1)
            Case Else
                SetLanguageIndex(0)
        End Select
        '
        Try
            Dim Culture As New System.Globalization.CultureInfo(My.Settings.Language)
            Threading.Thread.CurrentThread.CurrentUICulture = Culture
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        '
    End Sub

End Class
'----------------------------------------------------------------------------------------------------
