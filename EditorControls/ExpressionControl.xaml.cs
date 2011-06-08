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

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("expression")]
    public partial class ExpressionControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private Control m_simpleEditor;
        private bool m_simpleMode;
        private bool m_updatingList;

        public ExpressionControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += Initialise;
        }

        void Initialise()
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

            if (IsSimpleExpression(value))
            {
                SimpleMode = true;
                SimpleValue = ConvertToSimpleExpression(value);
            }
            else
            {
                SimpleMode = false;
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
            get { return m_simpleMode; }
            set
            {
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
    }
}
