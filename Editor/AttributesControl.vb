<ControlType("attributes")> _
Public Class AttributesControl
    Implements IElementEditorControl
    Implements IMultiAttributeElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_data As IEditorData

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
            ctlMultiControl.Controller = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return Nothing
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        lstAttributes.Items.Clear()
        m_data = data

        If data Is Nothing Then

        Else
            For Each attr In data.GetAttributeData
                Dim newItem As ListViewItem = lstAttributes.Items.Add(attr.AttributeName, attr.AttributeName, 0)
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

        End If
    End Sub

    Private Sub lstAttributes_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstAttributes.SelectedIndexChanged
        Dim selectedItem As String = GetSelectedAttribute()
        EditItem(selectedItem)
    End Sub

    Private Function GetSelectedAttribute() As String
        If lstAttributes.SelectedItems.Count = 0 Then Return Nothing
        Return lstAttributes.SelectedItems(0).Text
    End Function

    Private Sub EditItem(attribute As String)
        If (String.IsNullOrEmpty(attribute)) Then
            ctlMultiControl.Populate(Nothing)
        Else
            Dim controlData As New SubEditorControlData(attribute)
            ctlMultiControl.Initialise(m_controller, controlData)
            ctlMultiControl.Populate(m_data)
        End If
    End Sub

    Private Class SubEditorControlData
        Implements IEditorControl

        Private m_attribute As String

        Private Shared s_allTypes As New Dictionary(Of String, String) From {
            {"string", "String"},
            {"boolean", "Boolean"},
            {"script", "Script"},
            {"stringlist", "String List"}
        }

        Public Sub New(attribute As String)
            m_attribute = attribute
        End Sub

        Public ReadOnly Property Attribute As String Implements IEditorControl.Attribute
            Get
                Return m_attribute
            End Get
        End Property

        Public ReadOnly Property Caption As String Implements IEditorControl.Caption
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ControlType As String Implements IEditorControl.ControlType
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Expand As Boolean Implements IEditorControl.Expand
            Get
                Return False
            End Get
        End Property

        Public Function GetBool(tag As String) As Boolean Implements IEditorControl.GetBool
            Return False
        End Function

        Public Function GetDictionary(tag As String) As System.Collections.Generic.IDictionary(Of String, String) Implements IEditorControl.GetDictionary
            If tag = "types" Then
                Return s_allTypes
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function GetListString(tag As String) As System.Collections.Generic.IEnumerable(Of String) Implements IEditorControl.GetListString
            Throw New NotImplementedException
        End Function

        Public Function GetString(tag As String) As String Implements IEditorControl.GetString
            Select Case tag
                Case "checkbox"
                    Return "True"
                Case "editprompt"
                    Return "Please enter a value"
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        Public ReadOnly Property Height As Integer? Implements IEditorControl.Height
            Get
                Return Nothing
            End Get
        End Property

        Public Function IsControlVisible(data As IEditorData) As Boolean Implements IEditorControl.IsControlVisible
            Return True
        End Function

        Public ReadOnly Property Width As Integer? Implements IEditorControl.Width
            Get
                Return Nothing
            End Get
        End Property
    End Class

    Public Sub AttributeChanged(attribute As String, value As Object) Implements IMultiAttributeElementEditorControl.AttributeChanged
        Dim listViewItem As ListViewItem = lstAttributes.Items(attribute)
        listViewItem.SubItems(1).Text = GetDisplayString(value)
    End Sub
End Class
