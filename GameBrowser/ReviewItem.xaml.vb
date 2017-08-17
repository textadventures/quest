Imports TextAdventures.Utility.Language.L
Public Class ReviewItem
    Public Sub Populate(isReview As Boolean, userName As String, text As String, rating As Integer)
        reviewOrComment.Text = If(isReview, T("EditorReview"), T("EditorComment"))
        user.Text = userName
        reviewText.Text = System.Net.WebUtility.HtmlDecode(text)
        If rating > 0 Then
            ratingBlock.Visibility = Windows.Visibility.Visible
            Helper.OutputStars(stars, rating)
            ratingValue.Text = String.Format("({0} " + T("EditorStars") + ")", rating)
        Else
            ratingBlock.Visibility = Windows.Visibility.Collapsed
        End If
    End Sub
End Class
