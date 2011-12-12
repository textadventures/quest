Public Class ReviewItem
    Public Sub Populate(isReview As Boolean, userName As String, text As String, rating As Integer)
        reviewOrComment.Text = If(isReview, "Review", "Comment")
        user.Text = userName
        reviewText.Text = System.Net.WebUtility.HtmlDecode(text)
        If rating > 0 Then
            ratingBlock.Visibility = Windows.Visibility.Visible
            stars.Text = New String("★"c, rating)
            ratingValue.Text = String.Format("({0} stars)", rating)
        Else
            ratingBlock.Visibility = Windows.Visibility.Collapsed
        End If
    End Sub
End Class
