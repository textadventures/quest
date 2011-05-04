Public Class NewGameWindow

    Public Cancelled As Boolean
    Private m_templates As Dictionary(Of String, String)

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        Me.Hide()
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        Cancelled = True
        Me.Hide()
    End Sub

    Public Sub SetAvailableTemplates(templates As Dictionary(Of String, String))
        m_templates = templates

        For Each item In m_templates.Keys
            lstTemplate.Items.Add(item)
        Next

        If lstTemplate.Items.Count > 0 Then
            lstTemplate.SelectedIndex = 0
        End If
    End Sub

End Class