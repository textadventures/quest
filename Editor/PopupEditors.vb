Public Class PopupEditors

    ' TO DO: Rename this to SharedEditors or something?? PopupEditors?

    Public Structure EditStringResult
        Public Cancelled As Boolean
        Public Result As String
        Public ListResult As String
    End Structure

    Private Shared s_validationMessages As New Dictionary(Of ValidationMessage, String) From {
        {ValidationMessage.OK, "No error"},
        {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"}
    }

    Public Shared Function EditString(prompt As String, defaultResult As String) As EditStringResult
        Return EditStringWithDropdown(prompt, defaultResult, Nothing, Nothing, Nothing)
    End Function

    Public Shared Function EditStringWithDropdown(prompt As String, defaultResult As String, listCaption As String, listItems As IEnumerable(Of String), defaultListSelection As String) As EditStringResult
        Dim result As New EditStringResult

        Dim inputWindow As New InputWindow
        inputWindow.lblPrompt.Text = prompt + ":"
        inputWindow.txtInput.Text = defaultResult
        inputWindow.txtInput.Focus()
        inputWindow.txtInput.SelectAll()

        If listItems IsNot Nothing Then
            inputWindow.SetDropdown(listCaption, listItems, defaultListSelection)
        End If

        inputWindow.ShowDialog()

        Dim inputResult As String = inputWindow.txtInput.Text
        result.Cancelled = (inputResult.Length = 0)
        result.Result = inputResult

        If listItems IsNot Nothing Then
            result.ListResult = inputWindow.lstDropdown.Text
        End If

        Return result
    End Function

    Public Shared Function GetError(validationMessage As ValidationMessage, item As String) As String
        Return String.Format(s_validationMessages(validationMessage), item)
    End Function

    Public Shared Sub EditScript(controller As EditorController, ByRef scripts As IEditableScripts, attribute As String, element As String)
        Dim popOut As New ScriptEditorPopOut

        With popOut.ctlScriptEditor
            .PopOut = True
            .Scripts = scripts
            .Controller = controller
            .AttributeName = attribute
            .ElementName = element
            .Initialise()
            .UpdateList()
        End With
        popOut.ShowDialog()
        scripts = popOut.ctlScriptEditor.Scripts
        popOut.ctlScriptEditor.Save()
    End Sub
End Class
