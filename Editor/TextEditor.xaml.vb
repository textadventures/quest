Imports ICSharpCode.AvalonEdit.Highlighting
Imports ICSharpCode.AvalonEdit.Folding
Imports ICSharpCode.AvalonEdit.CodeCompletion

Public Class TextEditor

    Private m_foldingManager As FoldingManager
    Private m_foldingStrategy As New XmlFoldingStrategy
    Private m_completionWindow As CompletionWindow
    Private m_undoEnabled As Boolean
    Private m_redoEnabled As Boolean
    Private m_textWasSaved As Boolean

    Public Event UndoRedoEnabledUpdated(undoEnabled As Boolean, redoEnabled As Boolean)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML")
        textEditor.HorizontalScrollBarVisibility = Windows.Controls.ScrollBarVisibility.Auto
        textEditor.Padding = New System.Windows.Thickness(5)

        m_foldingManager = FoldingManager.Install(textEditor.TextArea)

        AddHandler textEditor.TextArea.TextEntering, AddressOf TextEntering
        AddHandler textEditor.TextArea.TextEntered, AddressOf TextEntered
        AddHandler textEditor.TextArea.KeyUp, AddressOf KeyPressed

        UpdateUndoRedoEnabled(True)

    End Sub

    Private Sub Initialise()
        m_foldingStrategy.UpdateFoldings(m_foldingManager, textEditor.Document)
        UpdateUndoRedoEnabled()
        m_textWasSaved = False
    End Sub

    Public Property EditText As String
        Get
            Return textEditor.Text
        End Get
        Set(value As String)
            textEditor.Text = value
            Initialise()
        End Set
    End Property

    Private Sub TextEntered(sender As Object, e As Windows.Input.TextCompositionEventArgs)
        UpdateUndoRedoEnabled()
        If e.Text = "<" Then
            m_completionWindow = New CompletionWindow(textEditor.TextArea)
            Dim data = m_completionWindow.CompletionList.CompletionData
            data.Add(New CompletionData("object"))
            data.Add(New CompletionData("command"))
            m_completionWindow.Show()
            AddHandler m_completionWindow.Closed, Sub() m_completionWindow = Nothing
        End If
    End Sub

    Private Sub TextEntering(sender As Object, e As Windows.Input.TextCompositionEventArgs)
        If e.Text.Length > 0 AndAlso m_completionWindow IsNot Nothing Then
            If Not Char.IsLetterOrDigit(e.Text(0)) Then
                ' Whenever a non-letter is typed while the completion window is open,
                ' insert the currently selected element.
                m_completionWindow.CompletionList.RequestInsertion(e)
            End If
        End If
    End Sub

    Private Sub KeyPressed(sender As Object, e As Windows.Input.KeyEventArgs)
        UpdateUndoRedoEnabled()
    End Sub

    Private Class CompletionData
        Implements ICompletionData

        Private m_text As String

        Public Sub New(text As String)
            m_text = text
        End Sub

        Public Sub Complete(textArea As ICSharpCode.AvalonEdit.Editing.TextArea, completionSegment As ICSharpCode.AvalonEdit.Document.ISegment, insertionRequestEventArgs As System.EventArgs) Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Complete
            textArea.Document.Replace(completionSegment, Text)
        End Sub

        Public ReadOnly Property Content As Object Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Content
            Get
                Return m_text
            End Get
        End Property

        Public ReadOnly Property Description As Object Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Description
            Get
                Return "Description for " + m_text
            End Get
        End Property

        Public ReadOnly Property Image As System.Windows.Media.ImageSource Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Image
            Get
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property Priority As Double Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Priority
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property Text As String Implements ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Text
            Get
                Return m_text
            End Get
        End Property

    End Class

    Public Sub Load(filename As String)
        textEditor.Load(filename)
        Initialise()
    End Sub

    Public Sub Save(filename As String)
        If Not textEditor.IsModified Then Return
        textEditor.Save(filename)
        m_textWasSaved = True
    End Sub

    Public ReadOnly Property IsModified As Boolean
        Get
            Return textEditor.IsModified
        End Get
    End Property

    Public Sub Undo()
        textEditor.Undo()
        UpdateUndoRedoEnabled()
    End Sub

    Public Sub Redo()
        textEditor.Redo()
        UpdateUndoRedoEnabled()
    End Sub

    Public ReadOnly Property CanUndo As Boolean
        Get
            Return textEditor.CanUndo
        End Get
    End Property

    Public ReadOnly Property CanRedo As Boolean
        Get
            Return textEditor.CanRedo
        End Get
    End Property

    Public Sub Cut()
        textEditor.Cut()
    End Sub

    Public Sub Copy()
        textEditor.Copy()
    End Sub

    Public Sub Paste()
        textEditor.Paste()
    End Sub

    Private Sub UpdateUndoRedoEnabled(Optional force As Boolean = False)
        Dim oldUndoEnabled As Boolean = m_undoEnabled
        Dim oldRedoEnabled As Boolean = m_redoEnabled
        m_undoEnabled = textEditor.CanUndo
        m_redoEnabled = textEditor.CanRedo

        If force OrElse oldUndoEnabled <> m_undoEnabled OrElse oldRedoEnabled <> m_redoEnabled Then
            RaiseEvent UndoRedoEnabledUpdated(m_undoEnabled, m_redoEnabled)
        End If
    End Sub

    Public ReadOnly Property TextWasSaved As Boolean
        Get
            Return m_textWasSaved
        End Get
    End Property

End Class
