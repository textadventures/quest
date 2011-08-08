Imports AxeSoftware.Quest

Public Class ElementList

    Public Event SendCommand(command As String)
    Private m_ignoreNames As New List(Of String)
    Private m_verbs() As List(Of String)
    Private m_emptyListLabel As String = "(empty)"
    Private m_nothingSelectedLabel As String = "(nothing selected)"

    Public Property Title() As String
        Get
            Return lblTitle.Text
        End Get
        Set(value As String)
            lblTitle.Text = value
        End Set
    End Property

    Public Property EmptyListLabel As String
        Get
            Return m_emptyListLabel
        End Get
        Set(value As String)
            m_emptyListLabel = value
        End Set
    End Property

    Public Property NothingSelectedLabel As String
        Get
            Return m_nothingSelectedLabel
        End Get
        Set(value As String)
            m_nothingSelectedLabel = value
        End Set
    End Property

    Private WriteOnly Property ToolbarButtons() As String()
        Set(value As String())
            tlbToolbar.Items.Clear()

            If value Is Nothing Then
                Dim btn As New System.Windows.Forms.ToolStripButton
                If lstList.Items.Count = 0 Then
                    btn.Text = m_emptyListLabel
                Else
                    btn.Text = m_nothingSelectedLabel
                End If
                btn.Enabled = False
                tlbToolbar.Items.Add(btn)
                Exit Property
            End If

            For Each btnText As String In value
                Dim btn As New System.Windows.Forms.ToolStripButton
                btn.Text = btnText
                tlbToolbar.Items.Add(btn)
            Next
        End Set
    End Property

    Public WriteOnly Property Items() As List(Of ListData)
        Set(value As List(Of ListData))

            Dim oldSelection As String = ""

            If lstList.SelectedItems.Count = 1 Then
                oldSelection = lstList.SelectedItems(0).Text
            End If

            lstList.Clear()
            ReDim m_verbs(value.Count)
            Dim count As Integer = 0

            For Each listItem As ListData In value
                ' m_ignoreNames contains the built-in compass directions, so we don't
                ' add them to the "Places and Objects" list too.
                If Not m_ignoreNames.Contains(listItem.Text) Then
                    Dim listViewItem As ListViewItem = lstList.Items.Add(listItem.Text)
                    If listItem.Text = oldSelection Then listViewItem.Selected = True
                    m_verbs(count) = New List(Of String)(listItem.Verbs)
                    count += 1
                End If
            Next

            If lstList.SelectedItems.Count = 0 Then
                If lstList.Items.Count > 0 Then
                    lstList.Items(0).Selected = True
                End If
            End If

            RefreshToolbarButtons()

        End Set
    End Property

    Private Sub tlbToolbar_ItemClicked(sender As System.Object, e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles tlbToolbar.ItemClicked
        Dim command As String

        If lstList.SelectedItems.Count <> 1 Then Exit Sub
        command = LCase(e.ClickedItem.Text + " " + lstList.SelectedItems(0).Text)

        RaiseEvent SendCommand(command)
    End Sub

    Private Sub lstList_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstList.SelectedIndexChanged
        RefreshToolbarButtons()
    End Sub

    Private Sub RefreshToolbarButtons()
        If lstList.SelectedItems.Count = 0 OrElse lstList.SelectedItems(0).Index = -1 Then
            ToolbarButtons = Nothing
            Exit Sub
        End If

        Dim verbs As List(Of String) = m_verbs(lstList.SelectedItems(0).Index)

        If verbs Is Nothing Then
            ToolbarButtons = Nothing
        Else
            ToolbarButtons = verbs.ToArray()
        End If
    End Sub

    Friend WriteOnly Property IgnoreNames() As List(Of String)
        Set(value As List(Of String))
            m_ignoreNames = value
        End Set
    End Property

    Public Sub Clear()
        lstList.Clear()
        RefreshToolbarButtons()
    End Sub

    Public WriteOnly Property Status As String
        Set(value As String)
            ctlSplitContainer.Panel2Collapsed = (value.Length = 0)
            txtStatus.Text = value
        End Set
    End Property
End Class
