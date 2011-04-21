<ControlType("dropdowntypes")> _
Public Class DropDownTypesControl
    Inherits DropDownControl

    Private m_dropDownValues As IDictionary(Of String, String)
    Private m_currentType As String
    Private m_elementName As String

    Private Const k_noType As String = "*"

    Protected Overrides Sub InitialiseControl(controlData As IEditorControl)
        If controlData IsNot Nothing Then
            m_dropDownValues = controlData.GetDictionary("types")
            lstDropdown.Items.AddRange(m_dropDownValues.Values.ToArray())
        Else
            lstDropdown.Items.Clear()
        End If
    End Sub

    Protected Overrides Sub PopulateData(data As IEditorData)
        m_elementName = data.Name

        Dim inheritedTypes As New List(Of String)

        ' The inherited types look like:
        '   *=default; typename1=Type 1; typename2=Type2

        ' Find out which of the handled types are inherited by the object

        For Each item In m_dropDownValues.Where(Function(i) i.Key <> k_noType)
            If Controller.DoesElementInheritType(m_elementName, item.Key) Then
                inheritedTypes.Add(item.Key)
            End If
        Next

        ' If more than one type is inherited by the object, disable the control

        Me.Enabled = inheritedTypes.Count <= 1

        Select Case inheritedTypes.Count
            Case 0
                ' Default - no types inherited
                lstDropdown.Text = m_dropDownValues(k_noType)
                m_currentType = k_noType
            Case 1
                lstDropdown.Text = m_dropDownValues(inheritedTypes(0))
                m_currentType = inheritedTypes(0)
            Case Else
                lstDropdown.Text = ""
                m_currentType = Nothing
        End Select
    End Sub

    Protected Overrides Sub SaveData(data As IEditorData)
        Dim selectedType As String = GetSelectedType()
        If selectedType Is Nothing Then Return
        If m_currentType Is Nothing Then Return
        If selectedType = m_currentType Then Return

        Controller.StartTransaction(String.Format("Change type from '{0}' to '{1}'", m_dropDownValues(m_currentType), m_dropDownValues(selectedType)))

        If m_currentType <> k_noType Then
            Controller.RemoveInheritedTypeFromElement(m_elementName, m_currentType, False)
        End If

        If selectedType <> k_noType Then
            Controller.AddInheritedTypeToElement(m_elementName, selectedType, False)
        End If

        Controller.EndTransaction()
    End Sub

    Private Function GetSelectedType() As String
        If lstDropdown.Text.Length = 0 Then Return Nothing
        Return m_dropDownValues.Keys.ToArray()(lstDropdown.SelectedIndex)
    End Function

    Protected Overrides Function GetValue() As Object
        Return Nothing
    End Function

    Protected Overrides Sub SetValue(value As Object)
        ' do nothing
    End Sub
End Class
