<ControlType("attributes")> _
Public Class AttributesControl
    Implements IElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return Nothing
        End Get
        Set(value As Object)
        End Set
    End Property

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            ' Hmm, we're going to be editing ALL attributes in here. I think this is only really read by ElementEditor
            ' in UpdateField.
            Return Nothing
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        lstAttributes.Items.Clear()

        If data Is Nothing Then

        Else
            For Each attr In data.GetAttributeData
                Dim newItem As ListViewItem = lstAttributes.Items.Add(attr.AttributeName)
                If attr.IsInherited Then newItem.ForeColor = Color.Gray
                Dim value As Object = data.GetAttribute(attr.AttributeName)
                Dim displayValue As String = GetDisplayString(value)
                newItem.SubItems.Add(Utility.FormatAsOneLine(displayValue))
            Next
        End If
    End Sub

    Private Function GetDisplayString(value As Object) As String
        Dim scriptValue As IEditableScripts = TryCast(value, IEditableScripts)
        Dim listStringValue As IEditableList(Of String) = TryCast(value, IEditableList(Of String))
        Dim dictionaryStringValue As IEditableDictionary(Of String) = TryCast(value, IEditableDictionary(Of String))

        If scriptValue IsNot Nothing Then
            Return scriptValue.DisplayString()
        ElseIf listStringValue IsNot Nothing Then
            Return GetListDisplayString(listStringValue.DisplayItems)
        ElseIf dictionaryStringValue IsNot Nothing Then
            Return GetDictionaryDisplayString(dictionaryStringValue.DisplayItems)
        Else
            Return value.ToString()
        End If
    End Function

    Private Function GetListDisplayString(items As IEnumerable(Of KeyValuePair(Of String, String))) As String
        Dim result As String = String.Empty

        For Each item In items
            If result.Length > 0 Then result += ", "
            result += item.Value
        Next

        Return result
    End Function


    Private Function GetDictionaryDisplayString(items As IEnumerable(Of KeyValuePair(Of String, String))) As String
        Dim result As String = String.Empty

        For Each item In items
            If result.Length > 0 Then result += ", "
            result += item.Key + "=" + item.Value
        Next

        Return result
    End Function


    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If controlData IsNot Nothing Then
            ctlMultiControl.InitialiseForAttributesEditor()

        End If
    End Sub

End Class
