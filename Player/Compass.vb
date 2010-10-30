Imports AxeSoftware.Quest

Public Class Compass

    Private Const numButtons As Integer = 11
    Private Const buttonSize As Integer = 28
    Private Const buttonSpacing As Integer = 2

    ' Wingdings arrows
    Private Const buttonCaptions As String = "ãáäß àåâætu"
    Private buttonCommands() As String = {"northwest", "north", "northeast", _
                                          "west", "out", "east", _
                                          "southwest", "south", "southeast", _
                                          "up", "down"}

    Private buttons As Dictionary(Of String, System.Windows.Forms.Button) = New Dictionary(Of String, System.Windows.Forms.Button)

    Public Event RunCommand(ByVal command As String)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        For i As Integer = 0 To numButtons - 1
            Dim button As System.Windows.Forms.Button
            button = New System.Windows.Forms.Button
            With button
                .Height = buttonSize
                .Width = buttonSize
                .Left = (i Mod 3) * (buttonSize + buttonSpacing)
                .Top = (i \ 3) * (buttonSize + buttonSpacing)
                If i >= 9 Then
                    .Left = 3 * (buttonSize + buttonSpacing)
                    ' up and down buttons are displaced half a button height downwards
                    .Top = CInt(((i - 9) + 0.5) * (buttonSize + buttonSpacing))
                End If

                If i <> 4 Then
                    If i < 9 Then
                        .Font = New Font("Wingdings", 9)
                    Else
                        .Font = New Font("Marlett", 10)
                    End If
                    .Text = Mid(buttonCaptions, i + 1, 1)
                Else
                    .Font = New Font("Tahoma", 7)
                    .Text = "out"
                End If

                .Tag = i
            End With
            AddHandler button.Click, AddressOf ButtonClicked
            pnlContainer.Controls.Add(button)
            buttons.Add(buttonCommands(i), button)
        Next

        pnlContainer.Width = (buttonSize * 4) + (buttonSpacing * 3)
        pnlContainer.Height = (buttonSize * 3) + (buttonSpacing * 2)
    End Sub

    Private Sub Compass_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        pnlContainer.Left = CInt((Me.Width - pnlContainer.Width) / 2)
    End Sub

    Private Sub ButtonClicked(ByVal sender As Object, ByVal e As System.EventArgs)
        RaiseEvent RunCommand(buttonCommands(CInt(DirectCast(sender, Button).Tag)))
    End Sub

    Public Sub SetAvailableExits(ByVal exits As List(Of ListData))
        Dim enabledList As New List(Of String)

        For Each direction As ListData In exits
            If buttons.ContainsKey(direction.Text) Then
                buttons(direction.Text).Enabled = True
                enabledList.Add(direction.Text)
            End If
        Next

        For Each key As String In buttons.Keys
            If Not enabledList.Contains(key) Then buttons(key).Enabled = False
        Next
    End Sub

    Friend ReadOnly Property CompassDirections() As String()
        Get
            Return buttonCommands
        End Get
    End Property
End Class
