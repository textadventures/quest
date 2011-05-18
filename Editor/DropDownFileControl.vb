<ControlType("dropdownfile")> _
Public Class DropDownFileControl
    Inherits DropDownControl

    Private m_fileLister As Func(Of IEnumerable(Of String))
    Private m_source As String

    Protected Overrides Sub InitialiseControl(controlData As IEditorControl)

        ' TO DO: we may want to allow a "freetext" attribute (as for base DropDownControl)
        'lstDropdown.DropDownStyle = ComboBoxStyle.DropDown

        If controlData IsNot Nothing Then
            Dim source As String = controlData.GetString("source")
            If source = "libraries" Then
                m_fileLister = AddressOf GetAvailableLibraries
            Else
                m_source = source
                m_fileLister = AddressOf GetFilesInGamePath
            End If

            RefreshFileList()
        End If

    End Sub

    Private Sub RefreshFileList()
        lstDropdown.Items.Clear()
        lstDropdown.Items.AddRange(m_fileLister.Invoke().ToArray())
    End Sub

    Protected Overrides Sub PopulateData(data As IEditorData)
        RefreshFileList()
        MyBase.PopulateData(data)
    End Sub

    Private Function GetAvailableLibraries() As IEnumerable(Of String)
        Return Controller.GetAvailableLibraries()
    End Function

    Private Function GetFilesInGamePath() As IEnumerable(Of String)
        Return Controller.GetAvailableExternalFiles(m_source)
    End Function
End Class
