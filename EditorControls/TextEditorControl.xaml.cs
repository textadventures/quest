using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Document;

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("texteditor")]
    public partial class TextEditorControl : UserControl, IElementEditorControl
    {
        private FoldingManager m_foldingManager;
        private XmlFoldingStrategy m_foldingStrategy = new XmlFoldingStrategy();
        private CompletionWindow m_completionWindow;
        private bool m_undoEnabled;
        private bool m_redoEnabled;
        private bool m_textWasSaved;
        private bool m_useFolding = true;
        public event UndoRedoEnabledUpdatedEventHandler UndoRedoEnabledUpdated;
        public delegate void UndoRedoEnabledUpdatedEventHandler(bool undoEnabled, bool redoEnabled);
        private ControlDataHelper<string> m_helper;
        private string m_filename;

        public TextEditorControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Options.Scrollable = true;
            m_helper.Initialise += new Action(m_helper_Initialise);

            SetSyntaxHighlighting("XML");
            textEditor.HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
            textEditor.Padding = new System.Windows.Thickness(5);

            m_foldingManager = FoldingManager.Install(textEditor.TextArea);

            textEditor.TextArea.TextEntering += TextEntering;
            textEditor.TextArea.TextEntered += TextEntered;
            textEditor.TextArea.KeyUp += KeyPressed;

            UpdateUndoRedoEnabled(true);
        }

        void m_helper_Initialise()
        {
        }

        private void Initialise()
        {
            if (m_useFolding)
            {
                m_foldingStrategy.UpdateFoldings(m_foldingManager, textEditor.Document);
            }
            UpdateUndoRedoEnabled();
            m_textWasSaved = false;
        }

        public void SetSyntaxHighlighting(string name)
        {
            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
        }

        public bool UseFolding
        {
            get { return m_useFolding; }
            set
            {
                m_useFolding = value;
                if (!value)
                {
                    textEditor.Padding = new System.Windows.Thickness(0);
                }
            }
        }

        public string EditText
        {
            get { return textEditor.Text; }
            set
            {
                textEditor.Text = value;
                Initialise();
            }
        }

        private void TextEntered(object sender, TextCompositionEventArgs e)
        {
            UpdateUndoRedoEnabled();
            if (e.Text == "<")
            {
                m_completionWindow = new CompletionWindow(textEditor.TextArea);
                dynamic data = m_completionWindow.CompletionList.CompletionData;
                data.Add(new CompletionData("object"));
                data.Add(new CompletionData("command"));
                data.Add(new CompletionData("verb"));
                data.Add(new CompletionData("exit"));
                data.Add(new CompletionData("function"));
                data.Add(new CompletionData("type"));
                data.Add(new CompletionData("walkthrough"));
                data.Add(new CompletionData("javascript"));
                data.Add(new CompletionData("inherit"));
                m_completionWindow.Show();
                m_completionWindow.Closed += m_completionWindow_Closed;
            }
        }

        void m_completionWindow_Closed(object sender, EventArgs e)
        {
            m_completionWindow.Closed -= m_completionWindow_Closed;
            m_completionWindow = null;
        }

        private void TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && m_completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    m_completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            UpdateUndoRedoEnabled();
        }

        private class CompletionData : ICompletionData
        {
            private string m_text;
            public CompletionData(string text)
            {
                m_text = text;
            }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }

            public object Content
            {
                get { return m_text; }
            }

            public object Description
            {
                get { return null; }
            }

            public ImageSource Image
            {
                get { return null; }
            }

            public double Priority
            {
                get { return 0; }
            }

            public string Text
            {
                get { return m_text; }
            }
        }

        public void Load(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                textEditor.IsEnabled = true;
                textEditor.Load(filename);
                Initialise();
            }
            else
            {
                textEditor.Text = "";
                textEditor.IsEnabled = false;
            }
        }

        public void Save(string filename)
        {
            if (!textEditor.IsModified) return;
            textEditor.Save(filename);
            m_textWasSaved = true;
        }

        public bool IsModified
        {
            get { return textEditor.IsModified; }
        }

        public void Undo()
        {
            textEditor.Undo();
            UpdateUndoRedoEnabled();
        }

        public void Redo()
        {
            textEditor.Redo();
            UpdateUndoRedoEnabled();
        }

        public bool CanUndo
        {
            get { return textEditor.CanUndo; }
        }

        public bool CanRedo
        {
            get { return textEditor.CanRedo; }
        }

        public void Cut()
        {
            textEditor.Cut();
        }

        public void Copy()
        {
            textEditor.Copy();
        }

        public void Paste()
        {
            textEditor.Paste();
        }

        private void UpdateUndoRedoEnabled(bool force = false)
        {
            bool oldUndoEnabled = m_undoEnabled;
            bool oldRedoEnabled = m_redoEnabled;
            m_undoEnabled = textEditor.CanUndo;
            m_redoEnabled = textEditor.CanRedo;

            if (force || oldUndoEnabled != m_undoEnabled || oldRedoEnabled != m_redoEnabled)
            {
                if (UndoRedoEnabledUpdated != null)
                {
                    UndoRedoEnabledUpdated(m_undoEnabled, m_redoEnabled);
                }
            }
        }

        public bool TextWasSaved
        {
            get { return m_textWasSaved; }
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();

            string filename = m_helper.Populate(data);
            textEditor.IsEnabled = m_helper.CanEdit(data) && !data.ReadOnly && !string.IsNullOrEmpty(filename);
            SetSyntaxHighlighting("JavaScript");
            UseFolding = false;
            if (!string.IsNullOrEmpty(filename))
            {
                m_filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(m_helper.Controller.Filename), filename);
                Load(m_filename);
            }

            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (m_filename == null) return;
            Save(m_filename);
        }

        public Control FocusableControl
        {
            get { return textEditor; }
        }

        public void Find()
        {
            ctlFind.Visibility = Visibility.Visible;
            ctlFind.txtFind.Focus();
        }

        private void ctlFind_Find(string findText)
        {
            int start = textEditor.SelectionStart;
            int findIndex = -1;
            if (start < textEditor.Text.Length)
            {
                findIndex = textEditor.Text.IndexOf(findText, start + 1, StringComparison.CurrentCultureIgnoreCase);
            }
            if (findIndex == -1)
            {
                findIndex = textEditor.Text.IndexOf(findText, 0, StringComparison.CurrentCultureIgnoreCase);
            }
            if (findIndex > -1)
            {
                textEditor.SelectionStart = findIndex;
                textEditor.SelectionLength = findText.Length;
                textEditor.ScrollToLine(textEditor.TextArea.Document.GetLineByOffset(findIndex).LineNumber);
            }
            else
            {
                MessageBox.Show(string.Format("Text not found: {0}", findText));
            }
        }

        private void ctlFind_Close()
        {
            ctlFind.Visibility = Visibility.Collapsed;
        }

        public bool WordWrap
        {
            get { return textEditor.WordWrap; }
            set { textEditor.WordWrap = value; }
        }
    }
}
