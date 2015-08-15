Option Strict Off
Option Explicit On

Imports System.Collections.Generic

Friend Class ChangeLog

    ' NOTE: We only actually use the Object change log at the moment, as that is the only
    ' one that has properties and actions.

    Public Enum AppliesTo
        [Object]
        Room
    End Enum

    Private _appliesToType As AppliesTo
    Private _changes As New Dictionary(Of String, String)

    Public Property AppliesToType() As AppliesTo
        Get
            Return _appliesToType
        End Get
        Set(Value As AppliesTo)
            _appliesToType = Value
        End Set
    End Property

    Public ReadOnly Property Changes() As Dictionary(Of String, String)
        Get
            Return _changes
        End Get
    End Property

    ' appliesTo = room or object name
    ' element = the thing that's changed, e.g. an action or property name
    ' changeData = the actual change info
    Public Sub AddItem(ByRef appliesTo As String, ByRef element As String, ByRef changeData As String)
        ' the first four characters of the changeData will be "prop" or "acti", so we add this to the
        ' key so that actions and properties don't collide.

        Dim key = appliesTo & "#" & Left(changeData, 4) & "~" & element
        If _changes.ContainsKey(key) Then
            _changes.Remove(key)
        End If

        _changes.Add(key, changeData)
    End Sub
End Class