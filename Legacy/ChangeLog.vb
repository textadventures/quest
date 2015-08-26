Option Strict On
Option Explicit On
Option Infer On

Imports System.Collections.Generic

Friend Class ChangeLog

    ' NOTE: We only actually use the Object change log at the moment, as that is the only
    ' one that has properties and actions.

    Public Enum AppliesTo
        [Object]
        Room
    End Enum

    Public AppliesToType As AppliesTo
    Public Changes As New Dictionary(Of String, String)

    ' appliesTo = room or object name
    ' element = the thing that's changed, e.g. an action or property name
    ' changeData = the actual change info
    Public Sub AddItem(ByRef appliesTo As String, ByRef element As String, ByRef changeData As String)
        ' the first four characters of the changeData will be "prop" or "acti", so we add this to the
        ' key so that actions and properties don't collide.

        Dim key = appliesTo & "#" & Left(changeData, 4) & "~" & element
        If Changes.ContainsKey(key) Then
            Changes.Remove(key)
        End If

        Changes.Add(key, changeData)
    End Sub
End Class