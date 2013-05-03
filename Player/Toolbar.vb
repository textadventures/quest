Public Class Toolbar
    Private m_simpleMode As Boolean
    Private m_buttons As New Dictionary(Of String, ToolStripButton)

    Public Class ButtonClickedEventArgs
        Inherits EventArgs

        Public Property Button As String
    End Class

    Public Event ButtonClicked(sender As Object, args As ButtonClickedEventArgs)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Height = ctlToolStrip.Height
        SimpleMode = False

        For Each item As ToolStripItem In ctlToolStrip.Items
            Dim button As ToolStripButton = TryCast(item, ToolStripButton)
            If button IsNot Nothing Then
                m_buttons.Add(DirectCast(button.Tag, String), button)
                AddHandler button.Click, AddressOf HandleClick
            End If
        Next
    End Sub

    Private Sub HandleClick(sender As Object, e As System.EventArgs)
        Dim button As ToolStripItem = DirectCast(sender, ToolStripItem)

        If Not button.Tag Is Nothing Then
            RaiseEvent ButtonClicked(Me, New ButtonClickedEventArgs With {.Button = DirectCast(button.Tag, String)})
        End If
    End Sub

    Public Property SimpleMode As Boolean
        Get
            Return m_simpleMode
        End Get
        Set(value As Boolean)
            If (m_simpleMode <> value) Then
                m_simpleMode = value
                butDebugger.Visible = Not m_simpleMode
                butLog.Visible = Not m_simpleMode
                butHTML.Visible = Not m_simpleMode
            End If
        End Set
    End Property

    Public Sub SetButtonChecked(button As String, checked As Boolean)
        m_buttons(button).Checked = checked
    End Sub

End Class
