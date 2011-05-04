<ControlType("multi")> _
Public Class MultiControl
    Implements IElementEditorControl
    Implements IAdjustableHeightControl

    Private Const k_paddingTop As Integer = 5
    Private Const k_paddingLeft As Integer = 10
    Private Const k_defaultHeight As Integer = 50
    Private m_heightWithScriptEditor As Integer = 400

    Private Shared s_controlTypesMap As Dictionary(Of String, String) = New Dictionary(Of String, String) From {
        {"boolean", "checkbox"},
        {"string", "textbox"},
        {"script", "script"},
        {"stringlist", "list"},
        {"int", "number"},
        {"object", "objects"}
    }

    Private Shared s_typeNamesMap As Dictionary(Of Type, String) = New Dictionary(Of Type, String) From {
        {GetType(Boolean), "boolean"},
        {GetType(String), "string"},
        {GetType(IEditableScripts), "script"},
        {GetType(IEditableList(Of String)), "stringlist"},
        {GetType(Integer), "int"},
        {GetType(IEditableObjectReference), "object"}
    }

    Private m_storedValues As Dictionary(Of String, Object) = New Dictionary(Of String, Object)

    Private m_value As Object
    Private m_oldValue As Object
    Private m_controller As EditorController
    Private m_elementName As String
    Private m_attributeName As String
    Private WithEvents m_currentEditor As IElementEditorControl
    Private WithEvents m_currentEditorAdjustableHeight As IAdjustableHeightControl
    Private m_data As IEditorData
    Private m_height As Integer
    Private m_controlData As IEditorControl

    Private m_loadedEditors As New Dictionary(Of String, IElementEditorControl)

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave
    Public Event HeightChanged(sender As Object, newHeight As Integer) Implements IAdjustableHeightControl.HeightChanged

    Private Structure TypesListItem
        Public TypeIndex As Integer
        Public TypeName As String
        Public TypeDescription As String
    End Structure

    Private m_types As List(Of TypesListItem)

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return m_value
        End Get
        Set(value As Object)
            m_value = value

            Dim typeName As String = GetTypeName(m_value)
            Dim editorName As String = GetEditorNameForType(typeName)
            SetSelectedType(typeName)
            GetOrCreateEditorControl(editorName)
            If Not String.IsNullOrEmpty(typeName) Then
                m_storedValues(typeName) = value
            End If
        End Set
    End Property

    Private Function GetTypeName(value As Object) As String
        If value Is Nothing Then Return String.Empty
        Dim type As Type = value.GetType
        Return s_typeNamesMap.FirstOrDefault(Function(t) t.Key.IsAssignableFrom(type)).Value
    End Function

    Private Function GetEditorNameForType(typeName As String) As String
        If String.IsNullOrEmpty(typeName) Then Return String.Empty
        Return s_controlTypesMap(typeName)
    End Function

    Private Sub GetOrCreateEditorControl(editorName As String)
        If (String.IsNullOrEmpty(editorName)) Then
            m_currentEditor = Nothing
        Else
            If Not m_loadedEditors.ContainsKey(editorName) Then
                Dim createType As Type = Controller.GetControlType(editorName)
                m_currentEditor = DirectCast(Activator.CreateInstance(createType), IElementEditorControl)
                m_currentEditor.Control.Parent = Control
                m_currentEditor.Control.Top = lstTypes.Top + lstTypes.Height + k_paddingTop
                m_currentEditor.Control.Left = k_paddingLeft
                m_currentEditor.Control.Width = Me.Width - m_currentEditor.Control.Left - Me.Padding.Right
                m_currentEditor.Control.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                If TypeOf m_currentEditor Is IResizableElementEditorControl Then
                    m_currentEditor.Control.Height = Me.Height - m_currentEditor.Control.Top - Me.Padding.Bottom
                    m_currentEditor.Control.Anchor = m_currentEditor.Control.Anchor Or AnchorStyles.Bottom
                End If
                m_loadedEditors.Add(editorName, m_currentEditor)
            Else
                m_currentEditor = m_loadedEditors(editorName)
            End If

            m_currentEditor.Controller = m_controller
            m_currentEditor.Initialise(m_controller, m_controlData)
            m_currentEditor.Populate(m_data)
            m_currentEditor.Control.Visible = True

            m_currentEditorAdjustableHeight = TryCast(m_currentEditor, IAdjustableHeightControl)
        End If

        HideOtherEditors()
        SetEditorHeight(editorName)
        SetEditorAttributes()
    End Sub

    Private Sub HideOtherEditors()
        For Each ctl As IElementEditorControl In m_loadedEditors.Values
            If Not ctl Is m_currentEditor Then
                ctl.Control.Visible = False
                ctl.Controller = Nothing
                ctl.Initialise(Nothing, Nothing)
            End If
        Next
    End Sub

    Private Sub SetEditorHeight(editorName As String)
        Dim newHeight As Integer

        If editorName = "script" Then
            newHeight = m_heightWithScriptEditor
        Else
            newHeight = k_defaultHeight
        End If

        If newHeight <> m_height Then
            m_height = newHeight
            RaiseEvent HeightChanged(Me, newHeight)
        End If
    End Sub

    Private Sub SetEditorAttributes()
        Dim checkBoxControl As CheckBoxControl = TryCast(m_currentEditor, CheckBoxControl)

        If checkBoxControl IsNot Nothing Then
            checkBoxControl.SetCaption(m_controlData.GetString("checkbox"))
        End If
    End Sub

    Private Sub SetSelectedType(typeName As String)
        If m_types Is Nothing Then Return
        If String.IsNullOrEmpty(typeName) Then Return

        Dim selectedType = m_types.First(Function(t) t.TypeName = typeName)
        lstTypes.SelectedItem = lstTypes.Items(selectedType.TypeIndex)
    End Sub

    Private Sub UserSelectedNewType(type As TypesListItem)
        Controller.StartTransaction(String.Format("Change type of '{0}' {1} to '{2}'", m_elementName, m_attributeName, type.TypeDescription))

        Dim newValue As Object

        ' If the user has previously selected this type, use the previous value, otherwise create a new
        ' default value for that type. This allows the user to switch back and forth between different
        ' types without the value being cleared out if they change their mind.

        If m_storedValues.ContainsKey(type.TypeName) Then
            newValue = m_storedValues(type.TypeName)
        Else
            Select Case type.TypeName
                Case "boolean"
                    newValue = False
                Case "string"
                    newValue = ""
                Case "int"
                    newValue = 0
                Case "script"
                    newValue = Controller.CreateNewEditableScripts(m_elementName, m_attributeName, Nothing, False)
                Case "stringlist"
                    newValue = Controller.CreateNewEditableList(m_elementName, m_attributeName, Nothing, False)
                Case "object"
                    newValue = Controller.CreateNewEditableObjectReference(m_elementName, m_attributeName, False)
                Case Else
                    Throw New InvalidOperationException
            End Select
        End If

        m_data.SetAttribute(m_attributeName, newValue)

        Controller.EndTransaction()
    End Sub

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Private Property ElementName() As String
        Get
            Return m_elementName
        End Get
        Set(value As String)
            m_elementName = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attributeName
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        If m_currentEditor Is Nothing Then Return
        m_currentEditor.Save(data)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        m_storedValues.Clear()
        If data Is Nothing Then
            ElementName = Nothing
            Value = Nothing
        Else
            ElementName = data.Name
            Dim currentValue As Object = data.GetAttribute(AttributeName)
            If CanEditType(currentValue) Then Value = currentValue
        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        lstTypes.Items.Clear()
        m_types = New List(Of TypesListItem)

        m_controlData = controlData

        If controlData IsNot Nothing Then
            m_attributeName = controlData.Attribute

            Dim types As IDictionary(Of String, String) = controlData.GetDictionary("types")
            InitialiseTypesList(types)

            Dim width As Integer = controlData.GetInt("listwidth")
            If width > 0 Then
                lstTypes.Width = width
                lstTypes.Anchor = AnchorStyles.Left Or AnchorStyles.Top
            End If
        End If
    End Sub

    Private Sub InitialiseTypesList(types As IDictionary(Of String, String))
        Dim index As Integer = 0

        For Each item In types
            lstTypes.Items.Add(item.Value)
            m_types.Add(New TypesListItem With {.TypeIndex = index, .TypeName = item.Key, .TypeDescription = item.Value})
            index += 1
        Next
    End Sub

    Private Sub lstTypes_SelectionChangeCommitted(sender As Object, e As System.EventArgs) Handles lstTypes.SelectionChangeCommitted
        Dim selectedType = m_types.First(Function(s) s.TypeIndex = lstTypes.SelectedIndex)
        UserSelectedNewType(selectedType)
    End Sub

    Private Sub m_currentEditor_Dirty(sender As Object, args As DataModifiedEventArgs) Handles m_currentEditor.Dirty
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Sub m_currentEditor_RequestParentElementEditorSave() Handles m_currentEditor.RequestParentElementEditorSave
        RaiseEvent RequestParentElementEditorSave()
    End Sub

    Private Sub m_currentEditorAdjustableHeight_HeightChanged(sender As Object, newHeight As Integer) Handles m_currentEditorAdjustableHeight.HeightChanged
        m_heightWithScriptEditor = m_currentEditorAdjustableHeight.Control.Top + newHeight
        RaiseEvent HeightChanged(Me, m_heightWithScriptEditor)
    End Sub

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return Nothing
        End Get
    End Property

    Public Function CanEditType(value As Object) As Boolean
        Dim typeName As String = GetTypeName(value)
        Return m_types.Any(Function(t) t.TypeName = typeName)
    End Function

End Class
