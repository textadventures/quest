<ControlType("texteditor")> _
Public Class TextEditorControl
    Implements IElementEditorControl

    Private m_attribute As String
    Private m_controller As EditorController
    Private m_filename As String

    Public Event UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean)
    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements IElementEditorControl.Dirty
    Public Event RequestParentElementEditorSave() Implements IElementEditorControl.RequestParentElementEditorSave

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AddHandler wpfTextEditor.UndoRedoEnabledUpdated, AddressOf wpfTextEditor_UndoRedoEnabledUpdated

    End Sub

    Public Property EditText As String
        Get
            Return wpfTextEditor.EditText
        End Get
        Set(value As String)
            wpfTextEditor.EditText = value
        End Set
    End Property

    Public Sub LoadFile(filename As String)
        wpfTextEditor.Load(filename)
    End Sub

    Public Sub SaveFile(filename As String)
        wpfTextEditor.Save(filename)
    End Sub

    Public ReadOnly Property IsModified As Boolean
        Get
            Return wpfTextEditor.IsModified
        End Get
    End Property

    Public Sub Undo()
        wpfTextEditor.Undo()
    End Sub

    Public Sub Redo()
        wpfTextEditor.Redo()
    End Sub

    Public ReadOnly Property CanUndo As Boolean
        Get
            Return wpfTextEditor.CanUndo
        End Get
    End Property

    Public ReadOnly Property CanRedo As Boolean
        Get
            Return wpfTextEditor.CanRedo
        End Get
    End Property

    Public Sub Cut()
        wpfTextEditor.Cut()
    End Sub

    Public Sub Copy()
        wpfTextEditor.Copy()
    End Sub

    Public Sub Paste()
        wpfTextEditor.Paste()
    End Sub

    Private Sub wpfTextEditor_UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean)
        RaiseEvent UndoRedoEnabledUpdated(undoEnabled, redoEnabled)
    End Sub

    Public ReadOnly Property TextWasSaved As Boolean
        Get
            Return wpfTextEditor.TextWasSaved
        End Get
    End Property

    Public ReadOnly Property AttributeName As String Implements IElementEditorControl.AttributeName
        Get
            Return m_attribute
        End Get
    End Property

    Public ReadOnly Property Control As System.Windows.Forms.Control Implements IElementEditorControl.Control
        Get
            Return Me
        End Get
    End Property

    Public Property Controller As EditorController Implements IElementEditorControl.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public ReadOnly Property ExpectedType As System.Type Implements IElementEditorControl.ExpectedType
        Get
            Return GetType(String)
        End Get
    End Property

    Public Sub Initialise(controller As EditorController, controlData As IEditorControl) Implements IElementEditorControl.Initialise
        Me.BorderStyle = Windows.Forms.BorderStyle.FixedSingle
        m_controller = controller
        If controlData IsNot Nothing Then
            m_attribute = controlData.Attribute
        End If
    End Sub

    Public Sub Populate(data As IEditorData) Implements IElementEditorControl.Populate
        If data IsNot Nothing Then
            Value = data.GetAttribute(m_attribute)
        End If
    End Sub

    Public Sub Save(data As IEditorData) Implements IElementEditorControl.Save
        If m_filename Is Nothing Then Return
        wpfTextEditor.Save(m_filename)
    End Sub

    Public Property Value As Object Implements IElementEditorControl.Value
        Get
            Return m_filename
        End Get
        Set(value As Object)
            Dim stringValue As String = TryCast(value, String)
            If stringValue IsNot Nothing Then
                m_filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Controller.Filename), stringValue)
                wpfTextEditor.SetSyntaxHighlighting("JavaScript")
                wpfTextEditor.UseFolding = False
                wpfTextEditor.Load(m_filename)
            Else
                wpfTextEditor.EditText = ""
                m_filename = Nothing
            End If
        End Set
    End Property
End Class
