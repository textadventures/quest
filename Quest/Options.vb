Imports AxeSoftware.Utility

Public Enum OptionNames
    UseGameColours
    ForegroundColour
    BackgroundColour
    LinkColour
End Enum

Public Class Options
    Private Shared m_instance As Options
    Public Shared ReadOnly Property Instance As Options
        Get
            If m_instance Is Nothing Then
                m_instance = New Options
            End If
            Return m_instance
        End Get
    End Property

    Private defaults As New Dictionary(Of OptionNames, String) From {
        {OptionNames.UseGameColours, True.ToString()},
        {OptionNames.ForegroundColour, Color.Black.ToArgb().ToString()},
        {OptionNames.BackgroundColour, Color.White.ToArgb().ToString()},
        {OptionNames.LinkColour, Color.Blue.ToArgb().ToString()}
    }

    Private Function GetOptionKey(optionName As OptionNames) As String
        Return optionName.ToString()
    End Function

    Private Function GetValue(optionName As OptionNames) As String
        Return Registry.GetSetting("Quest", "Settings", GetOptionKey(optionName), defaults(optionName)).ToString()
    End Function

    Private Sub SetValue(optionName As OptionNames, value As String)
        Registry.SaveSetting("Quest", "Settings", GetOptionKey(optionName), value)
    End Sub

    Public Function GetColourValue(optionName As OptionNames) As Color
        Return Color.FromArgb(Integer.Parse(GetValue(optionName)))
    End Function

    Public Sub SetColourValue(optionName As OptionNames, colour As Color)
        SetValue(optionName, colour.ToArgb().ToString())
    End Sub

    Public Function GetStringValue(optionName As OptionNames) As String
        Return GetValue(optionName)
    End Function

    Public Sub SetStringValue(optionName As OptionNames, value As String)
        SetValue(optionName, value)
    End Sub

    Public Function GetBooleanValue(optionName As OptionNames) As Boolean
        Return Boolean.Parse(GetValue(optionName))
    End Function

    Public Sub SetBooleanValue(optionName As OptionNames, value As Boolean)
        SetValue(optionName, value.ToString())
    End Sub

End Class
