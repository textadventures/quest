<ControlType("checkbox")> _
Public Class CheckBoxControl
    Implements IElementEditorControl

    Private m_oldValue As Boolean
    Private m_controller As EditorController
    Private m_attribute As String
    Private m_attributeName As String
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
            Return chkCheckBox.Checked
        End Get
        Set(value As Object)
            chkCheckBox.Checked = DirectCast(value, Boolean)
            m_oldValue = DirectCast(value, Boolean)
        End Set
    End Property

    Private Sub chkCheckBox_Click(sender As Object, e As System.EventArgs) Handles chkCheckBox.Click
        Save(m_data)
    End Sub

    Private Sub CheckBoxControl_Leave(sender As Object, e As System.EventArgs) Handles chkCheckBox.Leave
        Save(m_data)
    End Sub

    Private Sub CheckBoxControl_TextChanged(sender As Object, e As System.EventArgs) Handles chkCheckBox.TextChanged
        If IsDirty Then
            RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, chkCheckBox.Checked))
        End If
    End Sub

    Public ReadOnly Property IsDirty() As Boolean
        Get
            Return chkCheckBox.Checked <> m_oldValue
        End Get
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
            Return m_attribute
        End Get
    End Property

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
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

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        m_data = data

        If m_data Is Nothing Then
            Value = String.Empty
        Else
            Value = data.GetAttribute(m_attribute)
        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        If controlData IsNot Nothing Then
            m_attribute = controlData.Attribute
            m_attributeName = controlData.Caption
        Else
            m_attribute = Nothing
            m_attributeName = Nothing
        End If
    End Sub

    Public Sub Initialise(attributeName As String)
        m_attribute = attributeName
        m_attributeName = attributeName
    End Sub

    Public Sub SetCaption(caption As String)
        chkCheckBox.Text = caption
    End Sub
End Class
