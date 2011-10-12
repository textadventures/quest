Imports AxeSoftware.Utility

Public Enum OptionNames
    UseGameColours
    ForegroundColour
    BackgroundColour
    LinkColour
    UseGameFont
    FontFamily
    FontSize
    FontStyle
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
        {OptionNames.LinkColour, Color.Blue.ToArgb().ToString()},
        {OptionNames.UseGameFont, True.ToString()},
        {OptionNames.FontFamily, "Arial"},
        {OptionNames.FontSize, "9"},
        {OptionNames.FontStyle, "0"}
    }

    Public Event OptionChanged(optionName As OptionNames)

    Private Function GetOptionKey(optionName As OptionNames) As String
        Return optionName.ToString()
    End Function

    Private Function GetValue(optionName As OptionNames) As String
        Return Registry.GetSetting("Quest", "Settings", GetOptionKey(optionName), defaults(optionName)).ToString()
    End Function

    Private Sub SetValue(optionName As OptionNames, value As String)
        Dim oldValue As String = GetValue(optionName)
        Registry.SaveSetting("Quest", "Settings", GetOptionKey(optionName), value)
        If oldValue <> value Then
            RaiseEvent OptionChanged(optionName)
        End If
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

    Public Function GetSingleValue(optionName As OptionNames) As Single
        Return Single.Parse(GetValue(optionName))
    End Function

    Public Sub SetSingleValue(optionName As OptionNames, value As Single)
        SetValue(optionName, value.ToString())
    End Sub

    Public Function GetIntValue(optionName As OptionNames) As Integer
        Return Integer.Parse(GetValue(optionName))
    End Function

    Public Sub SetIntValue(optionName As OptionNames, value As Integer)
        SetValue(optionName, value.ToString())
    End Sub

End Class
