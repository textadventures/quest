<ControlType("verbs")> _
Public Class VerbsControl
    Inherits AttributesControl

    Private Shared s_allowedTypes As New Dictionary(Of String, String) From {
        {"string", "String"},
        {"script", "Script"}
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
End Class
