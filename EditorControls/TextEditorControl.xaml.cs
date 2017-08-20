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
using System.Text.RegularExpressions;

namespace TextAdventures.Quest.EditorControls
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
        private bool _findShownAlready;
        private bool _shouldShowFind;
        private bool _isUndoing;
        private bool _isReplacing;
        public event UndoRedoEnabledUpdatedEventHandler UndoRedoEnabledUpdated;
        public delegate void UndoRedoEnabledUpdatedEventHandler(bool undoEnabled, bool redoEnabled);
        private ControlDataHelper<string> m_helper;
        private string m_filename;

        /// <summary>Specifies how many times a text editor undo will be invoked for the current undo action. This helps with undoing replace all.</summary>
        private Stack<int> UndoActionStepCounts;
        /// <summary>Specifies how many times a text editor redo will be invoked for the current redo action.</summary>
        private Stack<int> RedoActionStepCounts;

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
            textEditor.TextChanged += TextChanged;

            UndoActionStepCounts = new Stack<int>();
            RedoActionStepCounts = new Stack<int>();
            
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

        private void TextChanged(object sender, EventArgs e)
        {
            if (_isUndoing || _isReplacing)
                return;

            UndoActionStepCounts.Push(1);
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
            _isUndoing = true;

            int iterations = UndoActionStepCounts.Pop();
            
            for (int i = 0; i < iterations; i++)
                textEditor.Undo();

            RedoActionStepCounts.Push(iterations);

            UpdateUndoRedoEnabled();

            _isUndoing = false;
        }

        public void Redo()
        {
            _isUndoing = true;

            int iterations = RedoActionStepCounts.Pop();

            for (int i = 0; i < iterations; i++ )
                textEditor.Redo();

            UndoActionStepCounts.Push(iterations);

            UpdateUndoRedoEnabled();

            _isUndoing = false;
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
                if (!m_undoEnabled)
                    UndoActionStepCounts.Clear();

                if (!m_redoEnabled)
                    RedoActionStepCounts.Clear();

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
            FindOrReplace(FindControlMode.Find);
        }

        public void Replace()
        {
            FindOrReplace(FindControlMode.Find | FindControlMode.Replace);
        }

        private void FindOrReplace(FindControlMode mode)
        {
            ctlFind.Mode = mode;
            ctlFind.Visibility = Visibility.Visible;

            /* About focusing the ctlFind.txtFind:
             * 
             * Calling the FocusFindTB should be enough. And it is, except for the first time the find bar is shown. Then for some reason the focus stays in the text editor control.
             * When debugging I found that this works even the first time, after the focus returns from the debugger to the app window. So, I simulated this with invoking the invalidate visual.
             * To see this behavior comment out the if block.
             *
             *  Usually, I would handle this differently with:
             *   var win = Window.GetWindow(this);
             *   FocusManager.SetFocusedElement(win, ctlFind.txtFind);
             * 
             * This really should be enough for focus to work in a WPF app, but I get null for win here. I think it's related to this control being hosted in a WinForms app.
             * I've also tried going down the call stack and calling focus on the different controls this control is hosted in, but it had no effect.
             * */

            if (!_findShownAlready)
            {
                _shouldShowFind = true;
                this.InvalidateVisual();
                return;
            }

            FocusFindTB();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            //This is called when the control is rendered after invalidate visual is completed.
            if (!_shouldShowFind || _findShownAlready)
                return;

            _findShownAlready = true;
            _shouldShowFind = false;

            FocusFindTB();
        }

        private void FocusFindTB()
        {
            ctlFind.txtFind.SelectAll();

            Keyboard.Focus(ctlFind.txtFind);
        }

        private void ctlFind_Find(string findText, bool useRegex)
        {
            int start = textEditor.SelectionStart + textEditor.SelectionLength;
            Regex search = GetRegexFor(findText, useRegex);
            Match match = search.Match(textEditor.Text, start);
            
            if (!match.Success && start > 0)
            {
                match = search.Match(textEditor.Text, 0, textEditor.SelectionStart);
            }
            if (!match.Success)
            {
                MessageBox.Show(string.Format("Text not found: {0}", findText));
            }
            else
            {
                textEditor.SelectionStart = match.Index;
                textEditor.SelectionLength = match.Value.Length;
                textEditor.ScrollToLine(textEditor.TextArea.Document.GetLineByOffset(match.Index).LineNumber);
            }
        }

        private void ctlFind_Replace(string findText, bool useRegex, string replaceText)
        {
            _isReplacing = true;

            if (textEditor.SelectedText.ToLower() == findText.ToLower())
            {
                textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, replaceText);
                UndoActionStepCounts.Push(1);
                UpdateUndoRedoEnabled();
            }

            ctlFind_Find(findText, useRegex);

            _isReplacing = false;
        }

        private void ctlFind_ReplaceAll(string findText, bool useRegex, string replaceText)
        {
            //concidered replacing all 'name' with 'namename'..
            _isReplacing = true;
            Regex search = GetRegexFor(findText, useRegex);
            MatchCollection matches = search.Matches(textEditor.Text);
            int offset = 0;
            foreach (Match match in matches)
            {
                textEditor.Document.Replace(match.Index + offset, match.Value.Length, replaceText);
                offset += replaceText.Length - match.Value.Length;
            }

            if (matches.Count <= 0)
            {
                MessageBox.Show(string.Format("Text not found: {0}", findText));
            }
            else
            {
                UndoActionStepCounts.Push(matches.Count);
                UpdateUndoRedoEnabled();
                MessageBox.Show(string.Format("Replaced {0} occurrences.", matches.Count));
            }
            _isReplacing = false;
        }

        private void ctlFind_Close()
        {
            ctlFind.Visibility = Visibility.Collapsed;
        }

        private Regex GetRegexFor(string text, bool useRegex)
        {
            if (!useRegex)
                text = Regex.Escape(text);

            return new Regex(text, RegexOptions.IgnoreCase);
        }

        public bool WordWrap
        {
            get { return textEditor.WordWrap; }
            set { textEditor.WordWrap = value; }
        }
    }
}
