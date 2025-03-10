﻿Imports System.Threading

Friend Class WalkthroughRunner
    Private m_gameDebug As IASLDebug
    Private m_game As IASL
    Private m_walkthrough As String
    Private m_showingMenu As Boolean
    Private m_showingQuestion As Boolean
    Private m_waiting As Boolean
    Private m_pausing As Boolean
    Private m_menuOptions As IDictionary(Of String, String)
    Private m_cancelled As Boolean

    Public Event Output(text As String)
    Public Event MarkScrollPosition()

    Public Sub New(game As IASLDebug, walkthrough As String)
        m_gameDebug = game
        m_game = DirectCast(game, IASL)
        m_walkthrough = walkthrough
    End Sub

    Public Sub Run()
        Dim delay As Integer = 0
        Dim dateStart = DateTime.Now
        For Each cmd As String In m_gameDebug.Walkthroughs.Walkthroughs(m_walkthrough).Steps
            If m_cancelled Then Exit For
            RaiseEvent MarkScrollPosition()
            If m_showingMenu Then
                SetMenuResponse(cmd)
            ElseIf m_showingQuestion Then
                SetQuestionResponse(cmd)
            Else
                If cmd.StartsWith("assert:") Then
                    Dim expr As String = cmd.Substring(7)
                    WriteLine("<br/><b>Assert:</b> " + expr)
                    If m_gameDebug.Assert(expr) Then
                        WriteLine("<span style=""color:green""><b>Pass</b></span>")
                    Else
                        WriteLine("<span style=""color:red""><b>Failed</b></span>")
                        Return
                    End If
                ElseIf cmd.StartsWith("label:") Then
                    ' ignore
                ElseIf cmd.StartsWith("delay:") Then
                    delay = Int32.Parse(cmd.Substring(6).Trim)
                ElseIf cmd.StartsWith("event:") Then
                    Dim eventData As String() = cmd.Substring(6).Split(New Char() {";"c}, 2)
                    Dim eventName As String = eventData(0)
                    Dim param As String = eventData(1)
                    m_game.SendEvent(eventName, param)
                ElseIf cmd.StartsWith("runtime:") Then
                    Dim dateEnd = DateTime.Now
                    Dim diff = dateEnd.Subtract(dateStart)
                    Dim res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds)
                    WriteLine("Walkthrough Runtime: " + res)
                Else
                    m_game.SendCommand(cmd)
                End If
            End If

            If m_cancelled Then Exit For

            Do
                If m_waiting Then
                    m_waiting = False
                    FinishWait()
                End If

                If m_pausing Then
                    m_pausing = False
                    FinishPause()
                End If
            Loop Until (Not m_waiting And Not m_pausing) Or m_cancelled
            Thread.Sleep(delay)
        Next

    End Sub

    Public Sub ShowMenu(menu As MenuData)
        m_showingMenu = True
        WriteLine("Menu: " + menu.Caption)
        m_menuOptions = menu.Options
    End Sub

    Private Sub SetMenuResponse(response As String)
        If response.StartsWith("menu:") Then
            m_showingMenu = False
            Dim menuResponse As String = response.Substring(5)
            WriteLine("  - " + menuResponse)
            If (m_menuOptions.ContainsKey(menuResponse)) Then
                m_game.SetMenuResponse(menuResponse)
            Else
                Throw New Exception("Menu response was not an option")
            End If
        Else
            Throw New Exception("No menu response defined in walkthrough")
        End If
    End Sub

    Public Sub ShowQuestion(question As String)
        m_showingQuestion = True
        WriteLine("Question: " + question)
    End Sub

    Private Sub SetQuestionResponse(response As String)
        If response.StartsWith("answer:") Then
            m_showingQuestion = False
            Dim questionResponse As String = response.Substring(7)
            WriteLine("  - " + questionResponse)
            If questionResponse = "yes" Then
                m_game.SetQuestionResponse(True)
            ElseIf questionResponse = "no" Then
                m_game.SetQuestionResponse(False)
            Else
                Throw New Exception("Question response was invalid")
            End If
        Else
            Throw New Exception("Question response not defined in walkthrough")
        End If
    End Sub

    Public Sub BeginWait()
        m_waiting = True
    End Sub

    Private Sub FinishWait()
        m_game.FinishWait()
    End Sub

    Public Sub BeginPause()
        m_pausing = True
    End Sub

    Private Sub FinishPause()
        m_game.FinishPause()
    End Sub

    Private Sub WriteLine(text As String)
        RaiseEvent Output(text)
    End Sub

    Public ReadOnly Property Steps As Integer
        Get
            Return m_gameDebug.Walkthroughs.Walkthroughs(m_walkthrough).Steps.Count
        End Get
    End Property

    Public Sub Cancel()
        m_cancelled = True
    End Sub
End Class
