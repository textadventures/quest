Public Class BrowseFilter
    Public Event CategoryChanged(category As String)

    Public Sub Populate(cats As String())
        lstCategories.Items.AddRange(cats)
        If lstCategories.Items.Count > 0 Then
            lstCategories.SelectedIndex = 0
        End If
    End Sub

    Private Sub lstCategories_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstCategories.SelectedIndexChanged
        RaiseEvent CategoryChanged(lstCategories.Text)
    End Sub
End Class
