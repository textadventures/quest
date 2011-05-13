<ControlType("attributes")> _
Public Class AttributesControl
    Implements IElementEditorControl
    Implements IMultiAttributeElementEditorControl

    Private Shared s_allTypes As New Dictionary(Of String, String) From {
        {"string", "String"},
        {"boolean", "Boolean"},
        {"int", "Integer"},
        {"script", "Script"},
        {"stringlist", "String List"},
        {"object", "Object"},
        {"simplepattern", "Command pattern"}
    }

    Private Class SubEditorControlData
        Implements IEditorControl

        Private m_attribute As String
        Private m_allowedTypes As Dictionary(Of String, String)

        Public Sub New(attribute As String, allowedTypes As Dictionary(Of String, String))
            m_attribute = attribute
            m_allowedTypes = allowedTypes
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
                Return m_allowedTypes
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function GetInt(tag As String) As Integer Implements IEditorControl.GetInt
            Return 0
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

        Public ReadOnly Property PaddingTop As Integer? Implements IEditorControl.PaddingTop
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Parent As IEditorDefinition Implements IEditorControl.Parent
            Get
                Return Nothing
            End Get
        End Property
    End Class

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_data As IEditorDataExtendedAttributeInfo
    Private m_inheritedTypeData As New Dictionary(Of String, IEditorAttributeData)

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
        lstTypes.Items.Clear()
        m_inheritedTypeData.Clear()
        cmdDelete.Enabled = False
        cmdDeleteType.Enabled = False
        ctlMultiControl.Visible = False
        m_data = DirectCast(data, IEditorDataExtendedAttributeInfo)

        If Not data Is Nothing Then
            For Each type In m_data.GetInheritedTypes
                m_inheritedTypeData.Add(type.AttributeName, type)
                AddListItem(lstTypes, type, AddressOf GetTypeDisplayString)
            Next

            For Each attr In m_data.GetAttributeData
                If CanDisplayAttribute(attr.AttributeName, m_data.GetAttribute(attr.AttributeName)) Then
                    AddListItem(lstAttributes, attr, AddressOf GetAttributeDisplayString)
                End If
            Next
        End If
    End Sub

    Protected Overridable Function CanDisplayAttribute(attribute As String, value As Object) As Boolean
        Return True
    End Function

    Private Sub AddListItem(attr As IEditorAttributeData)
        AddListItem(lstAttributes, attr, AddressOf GetAttributeDisplayString)
    End Sub

    Private Sub AddListItem(listView As ListView, attr As IEditorAttributeData, displayStringFunction As Func(Of IEditorAttributeData, String))
        Dim newItem As ListViewItem = listView.Items.Add(attr.AttributeName, attr.AttributeName, 0)
        newItem.ForeColor = GetAttributeColour(attr)
        Dim displayValue As String = displayStringFunction(attr)
        newItem.SubItems.Add(displayValue)
        newItem.SubItems.Add(attr.Source)
    End Sub

    Private Function GetDisplayString(value As Object) As String
        Dim scriptValue As IEditableScripts = TryCast(value, IEditableScripts)
        Dim listStringValue As IEditableList(Of String) = TryCast(value, IEditableList(Of String))
        Dim dictionaryStringValue As IEditableDictionary(Of String) = TryCast(value, IEditableDictionary(Of String))
        Dim wrappedValue As IDataWrapper = TryCast(value, IDataWrapper)
        Dim result As String

        If scriptValue IsNot Nothing Then
            result = scriptValue.DisplayString()
        ElseIf listStringValue IsNot Nothing Then
            result = GetListDisplayString(listStringValue.DisplayItems)
        ElseIf dictionaryStringValue IsNot Nothing Then
            result = GetDictionaryDisplayString(dictionaryStringValue.DisplayItems)
        ElseIf wrappedValue IsNot Nothing Then
            result = wrappedValue.DisplayString
        ElseIf value Is Nothing Then
            result = "(null)"
        Else
            result = value.ToString()
        End If

        Return Utility.FormatAsOneLine(result)
    End Function

    Private Function GetAttributeDisplayString(attr As IEditorAttributeData) As String
        Return GetDisplayString(m_data.GetAttribute(attr.AttributeName))
    End Function

    Private Function GetTypeDisplayString(attr As IEditorAttributeData) As String
        Return attr.AttributeName
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
            Dim controlData As New SubEditorControlData(attribute, AllowedTypes)
            ctlMultiControl.Initialise(m_controller, controlData)
            ctlMultiControl.Populate(m_data)
        End If
    End Sub

    Public Sub AttributeChanged(attribute As String, value As Object) Implements IMultiAttributeElementEditorControl.AttributeChanged
        AttributeChangedInternal(attribute, value, True)
    End Sub

    Private Sub AttributeChangedInternal(attribute As String, value As Object, updateMultiControl As Boolean)
        Dim listViewItem As ListViewItem = lstAttributes.Items(attribute)

        If value Is Nothing OrElse Not CanDisplayAttribute(attribute, value) Then
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
        If args.Attribute Is Nothing Then Return
        AttributeChangedInternal(args.Attribute, args.NewValue, False)
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Function GetAttributeColour(data As IEditorAttributeData) As Color
        Return If(data.IsInherited OrElse data.IsDefaultType, Color.Gray, SystemColors.WindowText)
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

    Private Sub cmdAddType_DropDownOpening(sender As Object, e As System.EventArgs) Handles cmdAddType.DropDownOpening
        For Each item As ToolStripItem In cmdAddType.DropDownItems
            RemoveHandler item.Click, AddressOf cmdAddTypeDropDownItem_Click
        Next

        cmdAddType.DropDownItems.Clear()
        Dim availableTypes As IEnumerable(Of String) = m_controller.GetElementNames("type")

        For Each item As String In availableTypes.Where(Function(t) Not m_controller.IsDefaultTypeName(t))
            Dim menuItem As ToolStripItem = cmdAddType.DropDownItems.Add(item)
            AddHandler menuItem.Click, AddressOf cmdAddTypeDropDownItem_Click
            menuItem.Enabled = Not lstTypes.Items.ContainsKey(item)
        Next
    End Sub

    Private Sub cmdAddTypeDropDownItem_Click(sender As System.Object, e As System.EventArgs)
        Dim typeToAdd As String = DirectCast(sender, ToolStripItem).Text
        m_controller.AddInheritedTypeToElement(m_data.Name, typeToAdd, True)
    End Sub

    Private Sub lstTypes_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles lstTypes.SelectedIndexChanged
        Dim selectedAttribute As String = GetSelectedType()
        cmdDeleteType.Enabled = DeleteTypeAllowed(selectedAttribute)
    End Sub

    Private Function GetSelectedType() As String
        If lstTypes.SelectedItems.Count = 0 Then Return Nothing
        Return lstTypes.SelectedItems(0).Text
    End Function

    Private Function DeleteTypeAllowed(type As String) As Boolean
        If String.IsNullOrEmpty(type) Then Return False
        Dim typeData As IEditorAttributeData = m_inheritedTypeData(type)
        Return Not (typeData.IsInherited OrElse typeData.IsDefaultType)
    End Function

    Private Sub cmdDeleteType_Click(sender As System.Object, e As System.EventArgs) Handles cmdDeleteType.Click
        Dim selectedType As String = GetSelectedType()
        m_controller.RemoveInheritedTypeFromElement(m_data.Name, selectedType, True)
    End Sub

    Protected Overridable ReadOnly Property AllowedTypes As Dictionary(Of String, String)
        Get
            Return s_allTypes
        End Get
    End Property
End Class
