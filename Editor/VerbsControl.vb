<ControlType("verbs")> _
Public Class VerbsControl
    Inherits AttributesControl

    Public Sub New()
        ctlSplitContainerMain.Panel1Collapsed = True
        lblAttributesTitle.Text = "Verbs"
        lblAttributesTitle.Width = 60
    End Sub

    Protected Overrides Function CanDisplayAttribute(attribute As String) As Boolean
        Return Controller.IsVerbAttribute(attribute)
    End Function
End Class
