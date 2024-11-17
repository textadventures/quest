Imports CefSharp

Public Class CefKeyboardHandler
    Implements IKeyboardHandler

    Public Event KeyPressed(code As Keys)

    Public Function OnKeyEvent(chromiumWebBrowser As IWebBrowser, browser As IBrowser, type As KeyType, windowsKeyCode As Integer, nativeKeyCode As Integer, modifiers As CefEventFlags, isSystemKey As Boolean) As Boolean Implements IKeyboardHandler.OnKeyEvent
        If type = KeyType.KeyUp Then
            Dim result = CType(windowsKeyCode, Keys)
            If (modifiers And CefEventFlags.ShiftDown) = CefEventFlags.ShiftDown Then result = result Or Keys.Shift
            If (modifiers And CefEventFlags.ControlDown) = CefEventFlags.ControlDown Then result = result Or Keys.Control
            If (modifiers And CefEventFlags.AltDown) = CefEventFlags.AltDown Then result = result Or Keys.Alt
            RaiseEvent KeyPressed(result)
        End If
    End Function

    Public Function OnPreKeyEvent(chromiumWebBrowser As IWebBrowser, browser As IBrowser, type As KeyType, windowsKeyCode As Integer, nativeKeyCode As Integer, modifiers As CefEventFlags, isSystemKey As Boolean, ByRef isKeyboardShortcut As Boolean) As Boolean Implements IKeyboardHandler.OnPreKeyEvent

    End Function
End Class
