<ControlType("verbs")> _
Public Class VerbsControl
    Inherits AttributesControl

    Private Shared s_allowedTypes As New Dictionary(Of String, String) From {
        {"string", "Print a message"},
        {"script", "Run a script"}
    }

    Public Sub New()
        ctlSplitContainerMain.Panel1Collapsed = True
        lblAttributesTitle.Text = "Verbs"
        lblAttributesTitle.Width = 60
    End Sub

    Protected Overrides Function CanDisplayAttribute(attribute As String, value As Object) As Boolean
        If Not Controller.IsVerbAttribute(attribute) Then Return False
        Return GetType(String).IsAssignableFrom(value.GetType()) OrElse GetType(IEditableScripts).IsAssignableFrom(value.GetType())
    End Function

    Protected Overrides ReadOnly Property AllowedTypes As System.Collections.Generic.Dictionary(Of String, String)
        Get
            Return s_allowedTypes
        End Get
    End Property

    Protected Overrides Sub Add()
        ' TO DO: This fetches all verbs in the game, but verbs can be defined in rooms, so we should
        ' filter out any out-of-scope verbs.

        Dim result As PopupEditors.EditStringResult = PopupEditors.EditString(
            "Please enter a name for the new verb",
            String.Empty,
            Controller.GetVerbProperties())

        If result.Cancelled Then Return

        If Not lstAttributes.Items.ContainsKey(result.Result) Then
            Controller.StartTransaction(String.Format("Add '{0}' verb", result.Result))

            If Not Controller.IsVerbAttribute(result.Result) Then
                Dim newVerbId As String = Controller.CreateNewVerb(Nothing, False)
                Dim verbData As IEditorData = Controller.GetEditorData(newVerbId)
                verbData.SetAttribute("property", result.Result)
                verbData.SetAttribute("pattern", result.Result)
                verbData.SetAttribute("defaulttext", "You can't " + result.Result + " that.")
            End If

            Data.SetAttribute(result.Result, String.Empty)
            Controller.EndTransaction()
        End If

        lstAttributes.Items(result.Result).Selected = True
        lstAttributes.SelectedItems(0).EnsureVisible()
    End Sub
End Class
