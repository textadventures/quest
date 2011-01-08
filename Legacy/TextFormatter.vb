Imports System.Collections.Generic

Public Class TextFormatter

    ' the Player generates style tags for us
    ' so all we need to do is have some kind of <color> <fontsize> <justify> tags etc.
    ' it would actually be a really good idea for the player to handle the <wait> and <clear> tags too...?

    Private bold As Boolean
    Private italic As Boolean
    Private underline As Boolean
    Private colour As String = ""
    Private fontSize As Integer = 0
    Private align As String = ""

    Public Function OutputHTML(ByVal input As String) As String
        Dim output As String = ""
        Dim position As Integer = 0
        Dim codePosition As Integer
        Dim finished As Boolean = False

        input = input.Replace("<", "&lt;").Replace(">", "&gt;").Replace(vbCrLf, "<br />")

        Do
            codePosition = input.IndexOf("|", position)
            If codePosition = -1 Then
                output += FormatText(input.Substring(position))
                finished = True
            Else
                output += FormatText(input.Substring(position, codePosition - position))
                position = codePosition + 1

                Dim oneCharCode As String = ""
                Dim twoCharCode As String = ""
                If position < input.Length Then
                    oneCharCode = input.Substring(position, 1)
                End If
                If position < (input.Length - 1) Then
                    twoCharCode = input.Substring(position, 2)
                End If

                Dim foundCode As Boolean = True

                Select Case twoCharCode
                    Case "xb"
                        bold = False
                    Case "xi"
                        italic = False
                    Case "xu"
                        underline = False
                    Case "cb"
                        colour = ""
                    Case "cr"
                        colour = "red"
                    Case "cl"
                        colour = "blue"
                    Case "cy"
                        colour = "yellow"
                    Case "cg"
                        colour = "green"
                    Case "jl"
                        align = ""
                    Case "jc"
                        align = "center"
                    Case "jr"
                        align = "right"
                    Case Else
                        foundCode = False
                End Select

                If foundCode Then
                    position += 2
                Else
                    foundCode = True
                    Select Case oneCharCode
                        Case "b"
                            bold = True
                        Case "i"
                            italic = True
                        Case "u"
                            underline = True
                        Case "n"
                            output += "<br />"
                        Case Else
                            foundCode = False
                    End Select

                    If foundCode Then
                        position += 1
                    End If
                End If

                If Not foundCode Then
                    If oneCharCode = "s" Then
                        ' |s00 |s10 etc.
                        If position < (input.Length - 2) Then
                            Dim sizeCode As String = input.Substring(position + 1, 2)
                            If Integer.TryParse(sizeCode, fontSize) Then
                                foundCode = True
                                position += 3
                            End If
                        End If
                    End If
                End If

                If Not foundCode Then
                    output += "|"
                End If

                ' can also have size codes

            End If

        Loop Until finished Or position >= input.Length

        Return output

    End Function

    Private Function FormatText(ByVal input As String) As String
        If input.Length = 0 Then Return input

        Dim output As String = ""

        If align.Length > 0 Then output += "<align align=""" + align + """>"
        If fontSize > 0 Then output += "<font size=""" + CStr(fontSize) + """>"
        If colour.Length > 0 Then output += "<color color=""" + colour + """>"
        If bold Then output += "<b>"
        If italic Then output += "<i>"
        If underline Then output += "<u>"
        output += input
        If underline Then output += "</u>"
        If italic Then output += "</i>"
        If bold Then output += "</b>"
        If colour.Length > 0 Then output += "</color>"
        If fontSize > 0 Then output += "</font>"
        If align.Length > 0 Then output += "</align>"

        Return output
    End Function

End Class
