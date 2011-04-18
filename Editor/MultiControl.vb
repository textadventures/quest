<ControlType("multi")> _
Public Class MultiControl
    Implements IElementEditorControl
    Implements IAdjustableHeightControl

    Private Const k_paddingTop As Integer = 5
    Private Const k_paddingLeft As Integer = 10
    Private Const k_defaultHeight As Integer = 60
    Private Const k_heightWithScriptEditor As Integer = 400

    Private Shared s_controlTypesMap As Dictionary(Of String, String) = New Dictionary(Of String, String) From {
        {"boolean", "checkbox"},
        {"string", "textbox"},
        {"script", "script"}
    }

    Private Shared s_typeNamesMap As Dictionary(Of Type, String) = New Dictionary(Of Type, String) From {
        {GetType(Boolean), "boolean"},
        {GetType(String), "string"},
        {GetType(IEditableScripts), "script"}
    }

    Private m_value As Object
    Private m_oldValue As Object
    Private m_controller As EditorController
    Private m_elementName As String
    Private m_attributeName As String
    Private m_currentEditor As IElementEditorControl
    Private m_data As IEditorData
    Private m_height As Integer

    Private m_loadedEditors As New Dictionary(Of String, IElementEditorControl)

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave
    Public Event HeightChanged(newHeight As Integer) Implements IAdjustableHeightControl.HeightChanged

    Private Structure TypesListItem
        Public TypeIndex As Integer
        Public TypeName As String
        Public TypeDescription As String
    End Structure

    Private m_types As New List(Of TypesListItem)

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
        End Set
    End Property

    Private Function GetTypeName(value As Object) As String
        If value Is Nothing Then Return String.Empty
        Dim type As Type = value.GetType
        Return s_typeNamesMap.First(Function(t) t.Key.IsAssignableFrom(type)).Value
    End Function

    Private Function GetEditorNameForType(typeName As String) As String
        If typeName.Length = 0 Then Return String.Empty
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
                m_currentEditor.Control.Width = Me.Width - m_currentEditor.Control.Left
                m_currentEditor.Control.Height = Me.Height - m_currentEditor.Control.Top
                m_currentEditor.Control.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                m_loadedEditors.Add(editorName, m_currentEditor)
            Else
                m_currentEditor = m_loadedEditors(editorName)
            End If

            m_currentEditor.Control.Visible = True
        End If

        For Each ctl As IElementEditorControl In m_loadedEditors.Values
            If Not ctl Is m_currentEditor Then
                ctl.Control.Visible = False
            End If
        Next

        Dim newHeight As Integer

        If editorName = "script" Then
            newHeight = k_heightWithScriptEditor
        Else
            newHeight = k_defaultHeight
        End If

        If newHeight <> m_height Then
            m_height = newHeight
            RaiseEvent HeightChanged(newHeight)
        End If
    End Sub

    Private Sub SetSelectedType(typeName As String)
        Dim selectedType = m_types.First(Function(t) t.TypeName = typeName)
        lstTypes.SelectedItem = lstTypes.Items(selectedType.TypeIndex)
    End Sub

    Private Sub UserSelectedNewType(type As TypesListItem)
        Controller.StartTransaction(String.Format("Change type of '{0}' {1} to '{2}'", m_elementName, m_attributeName, type.TypeDescription))

        Dim newValue As Object
        Select Case type.TypeName
            Case "boolean"
                newValue = False
            Case "string"
                newValue = ""
            Case "script"
                newValue = Controller.CreateNewEditableScripts(m_elementName, m_attributeName, Nothing, False)
            Case Else
                Throw New InvalidOperationException
        End Select

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

    End Sub

    ' TO DO: Need to bubble up subcontrol Dirty events

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        If data Is Nothing Then
            ElementName = Nothing
            Value = Nothing
        Else
            ElementName = data.Name
            Value = data.GetAttribute(AttributeName)
        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        lstTypes.Items.Clear()
        m_types.Clear()

        If controlData IsNot Nothing Then
            m_attributeName = controlData.Attribute

            Dim types As IDictionary(Of String, String) = controlData.GetDictionary("types")
            Dim index As Integer = 0

            For Each item In types
                lstTypes.Items.Add(item.Value)
                m_types.Add(New TypesListItem With {.TypeIndex = index, .TypeName = item.Key, .TypeDescription = item.Value})
                index += 1
            Next

        End If
    End Sub

    Private Sub lstTypes_SelectionChangeCommitted(sender As Object, e As System.EventArgs) Handles lstTypes.SelectionChangeCommitted
        Dim selectedType = m_types.First(Function(s) s.TypeIndex = lstTypes.SelectedIndex)
        UserSelectedNewType(selectedType)
    End Sub
End Class
