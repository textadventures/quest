<ControlType("dropdown")> _
Public Class DropDownControl
    Implements IElementEditorControl

    Private m_oldValue As String
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
            Return GetValue()
        End Get
        Set(value As Object)
            SetValue(value)
        End Set
    End Property

    Protected Overridable Function GetValue() As Object
        Return lstDropdown.Text
    End Function

    Protected Overridable Sub SetValue(value As Object)
        Dim stringValue As String = TryCast(value, String)
        If stringValue Is Nothing Then
            lstDropdown.Text = stringValue
        Else
            lstDropdown.Text = String.Empty
        End If
        m_oldValue = stringValue
    End Sub

    Public ReadOnly Property IsDirty() As Boolean
        Get
            Return lstDropdown.Text <> m_oldValue
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
        SaveData(data)
    End Sub

    Protected Overridable Sub SaveData(data As IEditorData)
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
        PopulateData(data)
    End Sub

    Protected Overridable Sub PopulateData(data As IEditorData)
        m_data = data

        If m_data Is Nothing Then
            Value = String.Empty
        Else
            Value = data.GetAttribute(m_attribute)
        End If
    End Sub

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        m_attribute = controlData.Attribute
        m_attributeName = controlData.Caption
        InitialiseControl(controlData)
    End Sub

    Protected Overridable Sub InitialiseControl(controlData As IEditorControl)
        If controlData IsNot Nothing Then
            Dim validValues As String() = New List(Of String)(controlData.GetListString("validvalues")).ToArray()
            lstDropdown.Items.AddRange(validValues)
        Else
            lstDropdown.Items.Clear()
        End If
    End Sub

    Public Sub Initialise(attributeName As String)
        m_attribute = attributeName
        m_attributeName = attributeName
    End Sub

    Private Sub lstDropdown_SelectionChangeCommitted(sender As Object, e As System.EventArgs) Handles lstDropdown.SelectionChangeCommitted
        Save(m_data)
    End Sub

    Private Sub lstDropdown_TextChanged(sender As Object, e As System.EventArgs) Handles lstDropdown.TextChanged
        If IsDirty Then
            RaiseEvent Dirty(Me, New DataModifiedEventArgs(m_oldValue, lstDropdown.Text))
        End If
    End Sub

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return GetType(String)
        End Get
    End Property
End Class
