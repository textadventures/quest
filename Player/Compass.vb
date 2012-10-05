Imports TextAdventures.Quest

Public Class Compass

    Private Const numButtons As Integer = 12
    Private Const buttonSize As Integer = 28
    Private Const buttonSpacing As Integer = 2

    ' Wingdings arrows
    Private Const buttonCaptions As String = "ãáäßàåâætu"

    Private defaultButtonCommands As New List(Of String)({
                                    "northwest", "north", "northeast",
                                    "west", "east",
                                    "southwest", "south", "southeast",
                                    "up", "down",
                                    "in", "out"
                                })

    Private buttonCommands As List(Of String) = defaultButtonCommands

    Private buttons As Dictionary(Of Integer, System.Windows.Forms.Button) = New Dictionary(Of Integer, System.Windows.Forms.Button)

    Public Event RunCommand(command As String)

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
                If i < 8 Then
                    Dim x As Integer = If(i <= 3, i, i + 1)
                    .Left = (x Mod 3) * (buttonSize + buttonSpacing)
                    .Top = (x \ 3) * (buttonSize + buttonSpacing)
                Else
                    .Left = (((i - 8) \ 2) + 3) * (buttonSize + buttonSpacing)
                    ' up/down/in/out buttons are displaced half a button height downwards
                    .Top = CInt((((i - 8) Mod 2) + 0.5) * (buttonSize + buttonSpacing))
                End If

                If i < 8 Then
                    .Font = New Font("Wingdings", 9)
                    .Text = Mid(buttonCaptions, i + 1, 1)
                ElseIf i < 10 Then
                    .Font = New Font("Marlett", 10)
                    .Text = Mid(buttonCaptions, i + 1, 1)
                Else
                    .Font = New Font("Tahoma", 7)
                    .Text = buttonCommands(i)
                End If

                .Tag = i
            End With
            AddHandler button.Click, AddressOf ButtonClicked
            pnlContainer.Controls.Add(button)
            buttons.Add(i, button)
        Next

        pnlContainer.Width = (buttonSize * 5) + (buttonSpacing * 4)
        pnlContainer.Height = (buttonSize * 3) + (buttonSpacing * 2)
    End Sub

    Private Sub Compass_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        pnlContainer.Left = CInt((Me.Width - pnlContainer.Width) / 2)
    End Sub

    Private Sub ButtonClicked(sender As Object, e As System.EventArgs)
        RaiseEvent RunCommand(buttonCommands(CInt(DirectCast(sender, Button).Tag)))
    End Sub

    Public Sub SetAvailableExits(exits As List(Of ListData))
        Dim enabledList As New List(Of Integer)

        For Each direction As ListData In exits
            If buttonCommands.Contains(direction.Text) Then
                buttons(buttonCommands.IndexOf(direction.Text)).Enabled = True
                enabledList.Add(buttonCommands.IndexOf(direction.Text))
            End If
        Next

        For Each key As Integer In buttons.Keys
            If Not enabledList.Contains(key) Then buttons(key).Enabled = False
        Next
    End Sub

    Friend Property CompassDirections() As List(Of String)
        Get
            Return buttonCommands
        End Get
        Set(value As List(Of String))
            buttonCommands = value
        End Set
    End Property

    Friend Sub ResetCompassDirections()
        buttonCommands = defaultButtonCommands
    End Sub

    Friend Property InLabel As String
        Get
            Return buttons(10).Text
        End Get
        Set(value As String)
            buttons(10).Text = value
        End Set
    End Property

    Friend Property OutLabel As String
        Get
            Return buttons(11).Text
        End Get
        Set(value As String)
            buttons(11).Text = value
        End Set
    End Property
End Class
