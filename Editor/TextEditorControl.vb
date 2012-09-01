Public Class TextEditorControl
    Public Event UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean)
    Public Event Dirty(sender As Object, args As DataModifiedEventArgs)
    Public Event RequestParentElementEditorSave()

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

    Public Sub Find()
        wpfTextEditor.Find()
    End Sub

    Public Sub Replace()
        wpfTextEditor.Replace()
    End Sub

    Public Property WordWrap As Boolean
        Get
            Return wpfTextEditor.WordWrap
        End Get
        Set(value As Boolean)
            wpfTextEditor.WordWrap = value
        End Set
    End Property
End Class
