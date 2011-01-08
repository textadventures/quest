Option Strict Off
Option Explicit On

Imports System.Collections.Generic

Friend Class ChangeLog

    ' NOTE: We only actually use the Object change log at the moment, as that is the only
    ' one that has properties and actions.

    Public Enum eAppliesToType
        atObject
        atRoom
    End Enum

    Private m_lAppliesToType As eAppliesToType
    Private m_colChanges As New Dictionary(Of String, String)

    Public Property AppliesToType() As eAppliesToType
        Get
            AppliesToType = m_lAppliesToType
        End Get
        Set(ByVal Value As eAppliesToType)
            m_lAppliesToType = Value
        End Set
    End Property

    Public ReadOnly Property Changes() As Dictionary(Of String, String)
        Get
            Changes = m_colChanges
        End Get
    End Property

    ' sAppliesTo = room or object name
    ' sElement = the thing that's changed, e.g. an action or property name
    ' sChangeData = the actual change info
    Public Sub AddItem(ByRef sAppliesTo As String, ByRef sElement As String, ByRef sChangeData As String)
        Dim sKey As String

        ' the first four characters of the sChangeData will be "prop" or "acti", so we add this to the
        ' key so that actions and properties don't collide.

        sKey = sAppliesTo & "#" & Left(sChangeData, 4) & "~" & sElement
        If m_colChanges.ContainsKey(sKey) Then
            m_colChanges.Remove(sKey)
        End If

        m_colChanges.add(sKey, sChangeData)

        Exit Sub

    End Sub
End Class