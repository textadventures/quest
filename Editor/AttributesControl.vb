<ControlType("attributes")> _
Public Class AttributesControl
    Implements IElementEditorControl
    Implements IMultiAttributeElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_data As IEditorDataExtendedAttributeInfo

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
        ctlMultiControl.Save(data)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        lstAttributes.Items.Clear()
        ctlMultiControl.Visible = False
        m_data = DirectCast(data, IEditorDataExtendedAttributeInfo)

        If Not data Is Nothing Then
            For Each attr In m_data.GetAttributeData
                AddListItem(attr)
            Next
        End If
    End Sub

    Private Sub AddListItem(attr As IEditorAttributeData)
        Dim newItem As ListViewItem = lstAttributes.Items.Add(attr.AttributeName, attr.AttributeName, 0)
        newItem.ForeColor = GetAttributeColour(attr)
        Dim value As Object = m_data.GetAttribute(attr.AttributeName)
        Dim displayValue As String = GetDisplayString(value)
        newItem.SubItems.Add(displayValue)
        newItem.SubItems.Add(attr.Source)
    End Sub

    Private Function GetDisplayString(value As Object) As String
        Dim scriptValue As IEditableScripts = TryCast(value, IEditableScripts)
        Dim listStringValue As IEditableList(Of String) = TryCast(value, IEditableList(Of String))
        Dim dictionaryStringValue As IEditableDictionary(Of String) = TryCast(value, IEditableDictionary(Of String))
        Dim result As String

        If scriptValue IsNot Nothing Then
            result = scriptValue.DisplayString()
        ElseIf listStringValue IsNot Nothing Then
            result = GetListDisplayString(listStringValue.DisplayItems)
        ElseIf dictionaryStringValue IsNot Nothing Then
            result = GetDictionaryDisplayString(dictionaryStringValue.DisplayItems)
        Else
            result = value.ToString()
        End If

        Return Utility.FormatAsOneLine(result)
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
    End Sub

    Private Sub lstAttributes_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstAttributes.SelectedIndexChanged
        Dim selectedAttribute As String = GetSelectedAttribute()
        cmdDelete.Enabled = DeleteAllowed(selectedAttribute)
        EditItem(selectedAttribute)
    End Sub

    Private Function GetSelectedAttribute() As String
        If lstAttributes.SelectedItems.Count = 0 Then Return Nothing
        Return lstAttributes.SelectedItems(0).Text
    End Function

    Private Function DeleteAllowed(attribute As String) As Boolean
        If String.IsNullOrEmpty(attribute) Then Return False
        Return Not m_data.GetAttributeData(attribute).IsInherited
    End Function

    Private Sub EditItem(attribute As String)
        If (String.IsNullOrEmpty(attribute)) Then
            ctlMultiControl.Visible = False
            ctlMultiControl.Populate(Nothing)
        Else
            ctlMultiControl.Visible = True
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
        AttributeChangedInternal(attribute, value, True)
    End Sub

    Private Sub AttributeChangedInternal(attribute As String, value As Object, updateMultiControl As Boolean)
        Dim listViewItem As ListViewItem = lstAttributes.Items(attribute)

        If value Is Nothing Then
            ' Remove attribute
            lstAttributes.Items.Remove(listViewItem)
        Else
            ' Add or update attribute
            If listViewItem Is Nothing Then
                AddListItem(m_data.GetAttributeData(attribute))
            Else
                listViewItem.SubItems(1).Text = GetDisplayString(value)
                Dim data As IEditorAttributeData = m_data.GetAttributeData(attribute)
                listViewItem.SubItems(2).Text = data.Source
                listViewItem.ForeColor = GetAttributeColour(data)
                If updateMultiControl Then
                    If attribute = GetSelectedAttribute() Then ctlMultiControl.Value = value
                End If
            End If
        End If

    End Sub

    Private Sub ctlMultiControl_Dirty(sender As Object, args As DataModifiedEventArgs) Handles ctlMultiControl.Dirty
        args.Attribute = GetSelectedAttribute()
        AttributeChangedInternal(args.Attribute, args.NewValue, False)
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Function GetAttributeColour(data As IEditorAttributeData) As Color
        Return If(data.IsInherited, Color.Gray, SystemColors.WindowText)
    End Function

    Private Sub cmdAdd_Click(sender As System.Object, e As System.EventArgs) Handles cmdAdd.Click
        Dim result As PopupEditors.EditStringResult = PopupEditors.EditString("Please enter a name for the new attribute", String.Empty)
        If result.Cancelled Then Return

        If Not lstAttributes.Items.ContainsKey(result.Result) Then
            m_controller.StartTransaction(String.Format("Add '{0}' attribute", result.Result))
            m_data.SetAttribute(result.Result, String.Empty)
            m_controller.EndTransaction()
        End If

        lstAttributes.Items(result.Result).Selected = True
        lstAttributes.SelectedItems(0).EnsureVisible()
    End Sub

    Private Sub cmdDelete_Click(sender As System.Object, e As System.EventArgs) Handles cmdDelete.Click
        Dim selectedAttribute As String = GetSelectedAttribute()
        m_controller.StartTransaction(String.Format("Remove '{0}' attribute", selectedAttribute))
        m_data.RemoveAttribute(selectedAttribute)
        m_controller.EndTransaction()
    End Sub

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return Nothing
        End Get
    End Property
End Class
