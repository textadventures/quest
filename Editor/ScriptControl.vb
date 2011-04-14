<ControlType("script")> _
Public Class ScriptControl
    Implements IElementEditorControl

    Private m_oldValue As IEditableScripts
    Private m_controller As EditorController
    Private m_elementName As String

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave
    Public Event HeightChanged(sender As Object, height As Integer)

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return ctlScriptEditor.Value
        End Get
        Set(value As Object)
            ctlScriptEditor.Value = DirectCast(value, IEditableScripts)
            m_oldValue = DirectCast(value, IEditableScripts)
        End Set
    End Property

    ' TO DO: Reimplement text box for direct code editing
    'Private Sub txtScript_TextChanged(sender As Object, e As System.EventArgs)
    '    If IsDirty Then
    '        RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, txtScript.Text))
    '    End If
    'End Sub

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            ctlScriptEditor.Controller = value
        End Set
    End Property

    Private Property ElementName() As String
        Get
            Return m_elementName
        End Get
        Set(value As String)
            m_elementName = value
            ctlScriptEditor.ElementName = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return ctlScriptEditor.AttributeName
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        ctlScriptEditor.Save()
    End Sub

    Private Sub ctlScriptEditor_Dirty(sender As Object, args As DataModifiedEventArgs) Handles ctlScriptEditor.Dirty
        RaiseEvent Dirty(sender, args)
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        If data Is Nothing Then
            ElementName = Nothing
            Value = Nothing
        Else
            ElementName = data.Name
            Value = data.GetAttribute(AttributeName)
        End If
    End Sub

    Public Sub Populate(data As IEditableScripts)
        Value = data
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If Not controlData Is Nothing Then
            ctlScriptEditor.AttributeName = controlData.Attribute
        End If
        ctlScriptEditor.Initialise()
    End Sub

    Private Sub ctlScriptEditor_HeightChanged(sender As Object, height As Integer) Handles ctlScriptEditor.HeightChanged
        RaiseEvent HeightChanged(Me, height)
    End Sub
End Class
