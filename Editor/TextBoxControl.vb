<ControlType("textbox")> _
Public Class TextBoxControl
    Inherits TextBox
    Implements IElementEditorControl

    Private m_oldValue As String
    Private m_controller As EditorController
    Private m_attribute As String
    Private m_attributeName As String
    Private m_data As IEditorData

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty

    Public ReadOnly Property Control() As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Value() As Object Implements IElementEditorControl.Value
        Get
            Return Text
        End Get
        Set(ByVal value As Object)
            Text = value
            m_oldValue = value
        End Set
    End Property

    Private Sub TextBoxControl_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Leave
        Save(m_data)
    End Sub

    Private Sub TextBoxControl_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.TextChanged
        If IsDirty Then
            RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, Text))
        End If
    End Sub

    Public ReadOnly Property IsDirty() As Boolean
        Get
            Return Text <> m_oldValue
        End Get
    End Property

    Public Property Controller() As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
        End Set
    End Property

    Public ReadOnly Property AttributeName() As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public Sub Save(ByVal data As IEditorData) Implements IElementEditorControl.Save
        If IsDirty Then
            Dim description As String = String.Format("Set {0} to '{1}'", m_attributeName, Value)
            m_controller.StartTransaction(description)
            data.SetAttribute(AttributeName, Value)
            m_controller.EndTransaction()
            ' reset the dirty flag
            Value = Value
            Debug.Assert(Not IsDirty)
        End If
    End Sub

    Public Sub Populate(ByVal data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data
        Value = data.GetAttribute(m_attribute)
    End Sub

    Public Sub Initialise(ByVal controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_attribute = controlData.Attribute
        m_attributeName = controlData.Caption
    End Sub

    Public Sub Initialise(ByVal attributeName As String)
        m_attribute = attributeName
        m_attributeName = attributeName
    End Sub
End Class
