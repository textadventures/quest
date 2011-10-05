Imports AxeSoftware.Utility

Public Enum OptionNames
    UseGameColours
    ForegroundColour
    BackgroundColour
    LinkColour
End Enum

Public Class Options
    Private Shared defaults As New Dictionary(Of OptionNames, String) From {
        {OptionNames.UseGameColours, True.ToString()},
        {OptionNames.ForegroundColour, Color.Black.ToArgb().ToString()},
        {OptionNames.BackgroundColour, Color.White.ToArgb().ToString()},
        {OptionNames.LinkColour, Color.Blue.ToArgb().ToString()}
    }

    Private Shared Function GetOptionKey(optionName As OptionNames) As String
        Return optionName.ToString()
    End Function

    Private Shared Function GetValue(optionName As OptionNames) As String
        Return Registry.GetSetting("Quest", "Settings", GetOptionKey(optionName), defaults(optionName)).ToString()
    End Function

    Private Shared Sub SetValue(optionName As OptionNames, value As String)
        Registry.SaveSetting("Quest", "Settings", GetOptionKey(optionName), value)
    End Sub

    Public Shared Function GetColourValue(optionName As OptionNames) As Color
        Return Color.FromArgb(Integer.Parse(GetValue(optionName)))
    End Function

    Public Shared Sub SetColourValue(optionName As OptionNames, colour As Color)
        SetValue(optionName, colour.ToArgb().ToString())
    End Sub

    Public Shared Function GetStringValue(optionName As OptionNames) As String
        Return GetValue(optionName)
    End Function

    Public Shared Sub SetStringValue(optionName As OptionNames, value As String)
        SetValue(optionName, value)
    End Sub

    Public Shared Function GetBooleanValue(optionName As OptionNames) As Boolean
        Return Boolean.Parse(GetValue(optionName))
    End Function

    Public Shared Sub SetBooleanValue(optionName As OptionNames, value As Boolean)
        SetValue(optionName, value.ToString())
    End Sub

End Class
