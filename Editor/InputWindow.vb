Public Class InputWindow

    Private m_activeControl As Control

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ShowDropdown(False)
        m_activeControl = txtInput
    End Sub

    Private Sub cmdOK_Click(sender As System.Object, e As System.EventArgs) Handles cmdOK.Click
        Me.Hide()
    End Sub

    Private Sub cmdCancel_Click(sender As System.Object, e As System.EventArgs) Handles cmdCancel.Click
        m_activeControl.Text = ""
        Me.Hide()
    End Sub

    Private Sub ShowDropdown(visible As Boolean)
        If visible Then
            Me.Height = 162
        Else
            Me.Height = 127
        End If

        lblDropdownCaption.Visible = visible
        lstDropdown.Visible = visible
    End Sub

    Public Sub SetDropdown(caption As String, items As IEnumerable(Of String), defaultSelection As String)
        lblDropdownCaption.Text = caption + ":"
        For Each item In items
            lstDropdown.Items.Add(item)
        Next
        lstDropdown.Text = defaultSelection
        ShowDropdown(True)
    End Sub

    Public Sub SetAutoComplete(items As IEnumerable(Of String))
        txtInput.Visible = False
        lstInputAutoComplete.Visible = True
        m_activeControl = lstInputAutoComplete
        For Each item In items
            lstInputAutoComplete.Items.Add(item)
        Next
    End Sub

    Public ReadOnly Property ActiveInputControl As Control
        Get
            Return m_activeControl
        End Get
    End Property

End Class