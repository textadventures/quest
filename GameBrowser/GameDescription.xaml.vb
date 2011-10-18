Public Class GameDescription
    Public Event Close()

    'newItem.description.Text = System.Net.WebUtility.HtmlDecode(data.Description)

    Private Sub cmdClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles cmdClose.Click
        RaiseEvent Close()
    End Sub
End Class
