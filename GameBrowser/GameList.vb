Imports Microsoft.Win32

Friend Class GameList
    Private m_launchCaption As String

    Public Event Launch(ByVal filename As String)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        ' This gets around a bug where the appearance of the vertical scrollbar causes contained controls not to resize
        Dim p As Padding = ctlTableLayout.Padding
        ctlTableLayout.Padding = New Padding(p.Left, p.Top, SystemInformation.VerticalScrollBarWidth + p.Right, p.Bottom)
    End Sub

    Public Property LaunchCaption() As String
        Get
            Return m_launchCaption
        End Get
        Set(ByVal value As String)
            m_launchCaption = value
        End Set
    End Property

    Public Sub CreateListElements(ByVal list As List(Of GameListItemData))
        Dim newItem As GameListItem

        Dim count As Integer = 0

        For Each recent As GameListItemData In list
            count += 1

            newItem = New GameListItem
            newItem.Dock = DockStyle.Fill
            newItem.Width = Me.Width
            newItem.GameName = recent.GameName
            newItem.GameInfo = System.IO.Path.GetFileName(recent.Filename)
            newItem.Filename = recent.Filename
            newItem.LaunchCaption = m_launchCaption
            AddHandler newItem.Launch, AddressOf LaunchHandler

            ctlTableLayout.Controls.Add(newItem)
        Next
    End Sub

    Private Sub LaunchHandler(ByVal filename As String)
        RaiseEvent Launch(filename)
    End Sub
End Class

