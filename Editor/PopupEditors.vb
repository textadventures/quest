Public Class PopupEditors

    ' TO DO: This has been migrated to EditorControls so should be removed

    Public Structure EditStringResult
        Public Cancelled As Boolean
        Public Result As String
        Public ListResult As String
    End Structure

    Private Shared s_validationMessages As New Dictionary(Of ValidationMessage, String) From {
        {ValidationMessage.OK, "No error"},
        {ValidationMessage.ItemAlreadyExists, "Item '{0}' already exists in the list"},
        {ValidationMessage.ElementAlreadyExists, "An element called '{0}' already exists in this game"}
    }

    Public Shared Function EditString(prompt As String, defaultResult As String, Optional autoCompleteList As IEnumerable(Of String) = Nothing) As EditStringResult
        Return EditStringWithDropdown(prompt, defaultResult, Nothing, Nothing, Nothing, autoCompleteList)
    End Function

    Public Shared Function EditStringWithDropdown(prompt As String, defaultResult As String, listCaption As String, listItems As IEnumerable(Of String), defaultListSelection As String, Optional autoCompleteList As IEnumerable(Of String) = Nothing) As EditStringResult
        Dim result As New EditStringResult

        Dim inputWindow As New InputWindow
        inputWindow.lblPrompt.Text = prompt + ":"
        If autoCompleteList IsNot Nothing Then
            inputWindow.SetAutoComplete(autoCompleteList)
        End If
        inputWindow.ActiveInputControl.Text = defaultResult
        inputWindow.ActiveInputControl.Focus()
        inputWindow.txtInput.SelectAll()

        If listItems IsNot Nothing Then
            inputWindow.SetDropdown(listCaption, listItems, defaultListSelection)
        End If

        inputWindow.ShowDialog()

        Dim inputResult As String = inputWindow.ActiveInputControl.Text
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
End Class
