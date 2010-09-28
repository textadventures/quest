Imports AxeSoftware.Quest

Public Class DebuggerPane

    Private WithEvents m_game As IASLDebug
    Private m_filter As String

    Public Property Game()
        Get
            Return m_game
        End Get
        Set(ByVal value)
            m_game = value
            UpdateObjectList()
        End Set
    End Property

    Public Sub UpdateObjectList()
        lstObjects.Items.Clear()
        For Each obj As String In m_game.GetObjects(m_filter)
            lstObjects.Items.Add(obj)
        Next
    End Sub

    Private Sub lstObjects_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstObjects.SelectedIndexChanged
        UpdateAttributes()
    End Sub

    Private Sub UpdateAttributes()
        lstAttributes.Items.Clear()
        If lstObjects.SelectedItems.Count = 1 Then
            Dim data As DebugData
            data = m_game.GetDebugData(lstObjects.SelectedItems(0).Text)
            For Each attr As String In data.Data.Keys
                Dim lvItem As ListViewItem
                lvItem = lstAttributes.Items.Add(attr)
                lvItem.SubItems.Add(data.Data(attr).Value)
                If data.Data(attr).IsInherited Then
                    lvItem.ForeColor = Color.DarkGray
                End If
            Next
        End If
    End Sub

    Private Sub m_game_ObjectsUpdated() Handles m_game.ObjectsUpdated
        UpdateObjectList()
    End Sub

    Friend Property Filter()
        Get
            Return m_filter
        End Get
        Set(ByVal value)
            m_filter = value
        End Set
    End Property

End Class
