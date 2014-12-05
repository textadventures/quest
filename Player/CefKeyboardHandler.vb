'Imports CefSharp

'Public Class CefKeyboardHandler
'    Implements IKeyboardHandler

'    Public Event KeyPressed(code As Keys)

'    Public Function OnKeyEvent(browser As IWebBrowser, type As KeyType, code As Integer, modifiers As Integer, isSystemKey As Boolean, isAfterJavaScript As Boolean) As Boolean Implements IKeyboardHandler.OnKeyEvent
'        If type = KeyType.KeyUp Then
'            Dim result As Keys = CType(code, Keys)
'            If (modifiers And 1) = 1 Then result = result Or Keys.Shift
'            If (modifiers And 2) = 2 Then result = result Or Keys.Control
'            If (modifiers And 4) = 4 Then result = result Or Keys.Alt
'            RaiseEvent KeyPressed(result)
'        End If
'    End Function
'End Class
