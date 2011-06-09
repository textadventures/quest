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
using System.Windows.Controls.Primitives;

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("expression")]
    public partial class ExpressionControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private Control m_simpleEditor;
        private bool m_simpleMode;
        private bool m_updatingList;
        private bool m_isSimpleModeAvailable = true;
        private bool m_focusing = false;
        private bool m_saving = false;

        public ExpressionControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += Initialise;
            InitialiseInsertMenu();
        }

        void Initialise()
        {
            if (IsSimpleModeAvailable)
            {
                string simpleEditor = m_helper.ControlDefinition.GetString("simpleeditor") ?? "textbox";

                // TO DO: Also want to handle checkbox and file controls

                switch (simpleEditor)
                {
                    case "textbox":
                        TextBox newTextBox = new TextBox();
                        newTextBox.TextChanged += SimpleEditor_TextChanged;
                        newTextBox.LostFocus += SimpleEditor_LostFocus;
                        m_simpleEditor = newTextBox;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid control type for expression");
                }
                Grid.SetRow(m_simpleEditor, Grid.GetRow(txtExpression));
                Grid.SetColumn(m_simpleEditor, Grid.GetColumn(txtExpression));
                grid.Children.Add(m_simpleEditor);

                m_updatingList = true;
                lstType.Items.Add(m_helper.ControlDefinition.GetString("simple"));
                lstType.Items.Add("expression");
                m_updatingList = false;
            }
        }

        void SimpleEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            Save();
        }

        void SimpleEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.SetDirty(ConvertFromSimpleExpression(SimpleValue));
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();

            string value = m_helper.Populate(data) ?? string.Empty;

            // Don't change the SimpleMode setting if we're just repopulating because this expression has
            // been saved
            if (!m_saving)
            {
                SimpleMode = IsSimpleExpression(value);
            }

            if (SimpleMode)
            {
                SimpleValue = ConvertToSimpleExpression(value);
            }
            else
            {
                txtExpression.Text = m_helper.Populate(data);
            }

            txtExpression.IsEnabled = m_helper.CanEdit(data);
            txtExpression.IsReadOnly = data.ReadOnly;

            lstType.IsEnabled = !data.ReadOnly;

            // TO DO: Enabled/Readonly state for simple editor

            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            m_saving = true;
            string saveValue = null;
            if (SimpleMode)
            {
                saveValue = ConvertFromSimpleExpression(SimpleValue);
            }
            else
            {
                saveValue = txtExpression.Text;
            }
            m_helper.Save(saveValue);
            m_saving = false;
        }

        private void txtExpression_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.SetDirty(txtExpression.Text);
        }

        private void txtExpression_LostFocus(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private bool SimpleMode
        {
            get { return IsSimpleModeAvailable ? m_simpleMode : false; }
            set
            {
                if (!IsSimpleModeAvailable) return;
                m_simpleMode = value;

                m_updatingList = true;
                lstType.SelectedIndex = m_simpleMode ? 0 : 1;
                m_updatingList = false;

                Visibility visibility = m_simpleMode ? Visibility.Collapsed : Visibility.Visible;
                txtExpression.Visibility = visibility;
                cmdInsert.Visibility = visibility;

                ((Control)m_simpleEditor).Visibility = m_simpleMode ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void lstType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_updatingList) return;
            Save();
            SimpleMode = (lstType.SelectedIndex == 0);
            if (SimpleMode)
            {
                if (IsSimpleExpression(txtExpression.Text))
                {
                    SimpleValue = ConvertToSimpleExpression(txtExpression.Text);
                }
                else
                {
                    SimpleValue = "";
                }
            }
            else
            {
                txtExpression.Text = ConvertFromSimpleExpression(SimpleValue);
            }
        }

        private bool IsSimpleExpression(string expression)
        {
            if (!IsSimpleModeAvailable) return false;

            // must start and end with quote character
            if (!(expression.StartsWith("\"") && expression.EndsWith("\""))) return false;

            // must not contain a quote character
            return !ConvertToSimpleExpression(expression).Contains("\"");
        }

        private string ConvertToSimpleExpression(string expression)
        {
            return expression.Substring(1, expression.Length - 2);
        }

        private string ConvertFromSimpleExpression(string simpleValue)
        {
            return string.Format("\"{0}\"", simpleValue);
        }

        private string SimpleValue
        {
            get
            {
                return ((TextBox)m_simpleEditor).Text;
            }
            set
            {
                ((TextBox)m_simpleEditor).Text = value;
            }
        }

        public bool IsSimpleModeAvailable
        {
            get { return m_isSimpleModeAvailable; }
            set
            {
                m_isSimpleModeAvailable = value;

                lstType.Visibility = m_isSimpleModeAvailable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void cmdInsert_Click(object sender, RoutedEventArgs e)
        {
            TextOptions.SetTextFormattingMode(mnuInsertMenu, TextFormattingMode.Display);
            mnuInsertMenu.IsOpen = true;
        }

        private void InitialiseInsertMenu()
        {
            AddInsertMenuItem("Variable", InsertVariable);
            AddInsertMenuItem("Object", InsertObject);
            AddInsertMenuItem("Property", InsertProperty);
            AddInsertMenuItem("Function", InsertFunction);
            mnuInsertMenu.Items.Add(new Separator());
            AddInsertMenuItem("and", () => InsertString(" and "));
            AddInsertMenuItem("or", () => InsertString(" or "));
            AddInsertMenuItem("+", () => InsertString(" + "));
            AddInsertMenuItem("-", () => InsertString(" - "));
            AddInsertMenuItem("*", () => InsertString(" * "));
            AddInsertMenuItem("/", () => InsertString(" / "));
            AddInsertMenuItem("=", () => InsertString(" = "));
        }

        private void AddInsertMenuItem(string caption, Action insertAction)
        {
            MenuItem newItem = new MenuItem();
            newItem.Header = caption;
            newItem.Click += (object sender, RoutedEventArgs e) => insertAction.Invoke();
            mnuInsertMenu.Items.Add(newItem);
        }

        private void InsertVariable()
        {
        }

        private void InsertObject()
        {
        }

        private void InsertProperty()
        {
        }

        private void InsertFunction()
        {
        }

        private void InsertString(string text)
        {
            int index = txtExpression.SelectionStart;
            txtExpression.Text = txtExpression.Text.Insert(index, text);
            txtExpression.SelectionStart = index + text.Length;
            m_focusing = true;
            txtExpression.Focus();
            m_focusing = false;
        }
    }
}
