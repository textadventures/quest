Imports TextAdventures.Quest.Controls.Menu

Friend Class MenuData

    Private m_modes As List(Of MenuMode)
    Private m_shortcut As System.Windows.Forms.Keys

    Public Sub New(ParamArray modes() As MenuMode)
        m_modes = New List(Of MenuMode)(modes)
    End Sub

    Public Function IsApplicable(mode As MenuMode) As Boolean
        Return m_modes.Contains(mode)
    End Function

    Public Property ShortcutKeys As System.Windows.Forms.Keys
        Get
            Return m_shortcut
        End Get
        Set(value As System.Windows.Forms.Keys)
            m_shortcut = value
        End Set
    End Property

End Class
