Imports ICSharpCode.AvalonEdit.Highlighting
Imports ICSharpCode.AvalonEdit.Folding
Imports ICSharpCode.AvalonEdit.CodeCompletion

Public Class TextEditor

    Private m_foldingManager As FoldingManager
    Private m_foldingStrategy As New XmlFoldingStrategy
    Private m_completionWindow As CompletionWindow

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

    End Sub

    Public Property EditText As String
        Get
            Return textEditor.Text
        End Get
        Set(value As String)
            textEditor.Text = value
            m_foldingStrategy.UpdateFoldings(m_foldingManager, textEditor.Document)
        End Set
    End Property

    Private Sub TextEntered(sender As Object, e As Windows.Input.TextCompositionEventArgs)
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

End Class
