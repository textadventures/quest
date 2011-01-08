Public Class Menu
    Private m_cancelled As Boolean
    Private m_selected As Boolean

    Public WriteOnly Property Options() As IDictionary(Of String, String)
        Set(ByVal value As IDictionary(Of String, String))
            For Each key As String In value.Keys
                lstOptions.Items.Add(key, value(key), "")
            Next
            lstOptions.Items(0).Selected = True
        End Set
    End Property

    Public WriteOnly Property Caption() As String
        Set(ByVal value As String)
            lblCaption.Text = value
        End Set
    End Property

    Public WriteOnly Property AllowCancel() As Boolean
        Set(ByVal value As Boolean)
            cmdCancel.Visible = value
            Me.ControlBox = value
        End Set
    End Property

    Public ReadOnly Property SelectedItem() As String
        Get
            If m_cancelled Then Return Nothing
            If lstOptions.SelectedItems.Count = 0 Then Return Nothing
            Return lstOptions.SelectedItems(0).Name
        End Get
    End Property

    Private Sub cmdSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelect.Click
        ItemSelected()
    End Sub

    Private Sub ItemSelected()
        m_selected = True
        Me.Close()
    End Sub

    Private Sub Menu_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        m_cancelled = Not m_selected
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        m_cancelled = True
        Me.Close()
    End Sub

    Private Sub lstOptions_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstOptions.DoubleClick
        ItemSelected()
    End Sub

    Private Sub lstOptions_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstOptions.Resize
        lstOptions.Columns(0).Width = lstOptions.Width
    End Sub
End Class