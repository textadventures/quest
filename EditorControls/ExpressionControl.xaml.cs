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

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("expression")]
    public partial class ExpressionControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private Control m_simpleEditor;
        private bool m_simpleMode;
        private bool m_templateMode;
        private bool m_useExpressionTemplates = false;
        private bool m_updatingList;
        private bool m_isSimpleModeAvailable = true;
        private bool m_saving = false;
        private IEditorData m_data;
        private bool m_booleanEditor;
        private ExpressionTemplate m_templateEditor;
        private string m_templatesFilter;

        public ExpressionControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += Initialise;
            m_helper.Uninitialise += Uninitialise;
            InitialiseInsertMenu();
        }

        void Initialise()
        {
            m_templatesFilter = m_helper.ControlDefinition.GetString("usetemplates");
            if (m_templatesFilter != null) UseExpressionTemplates = true;

            if (IsSimpleModeAvailable)
            {
                if (m_helper.ControlDefinition.GetString("simpleeditor") != "boolean" && m_helper.ControlDefinition.GetString("simple") == null)
                {
                    IsSimpleModeAvailable = false;
                }
            }

            if (IsSimpleModeAvailable)
            {
                InitialiseSimpleEditor();
            }

            if (UseExpressionTemplates)
            {
                PopulateExpressionTemplates();
            }
        }

        void Uninitialise()
        {
            if (m_templateEditor != null)
            {
                m_templateEditor.Controller = null;
                m_templateEditor.Dirty -= m_templateEditor_Dirty;
                m_templateEditor.RequestSave -= m_templateEditor_RequestSave;
                m_templateEditor.Uninitialise();
            }
            UninitialiseSimpleEditor();
        }

        private void InitialiseSimpleEditor()
        {
            string simpleEditor = m_helper.ControlDefinition.GetString("simpleeditor") ?? "textbox";

            m_updatingList = true;

            switch (simpleEditor)
            {
                case "textbox":
                    TextBox newTextBox = new TextBox();
                    newTextBox.MinHeight = 20;
                    newTextBox.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    newTextBox.TextChanged += SimpleEditor_TextChanged;
                    newTextBox.LostFocus += SimpleEditor_LostFocus;
                    if (m_helper.ControlDefinition.GetBool("multiline"))
                    {
                        newTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        newTextBox.TextWrapping = TextWrapping.Wrap;
                        newTextBox.AcceptsReturn = true;
                    }
                    m_simpleEditor = newTextBox;
                    break;
                case "file":
                    FileControl newFileControl = new FileControl();
                    newFileControl.Helper.DoInitialise(m_helper.Controller, m_helper.ControlDefinition);
                    newFileControl.RefreshFileList();
                    newFileControl.Helper.Dirty += SimpleEditor_Dirty;
                    newFileControl.SelectionChanged += FileControl_SelectionChanged;
                    m_simpleEditor = newFileControl;
                    break;
                case "boolean":
                    m_simpleEditor = null;
                    lstType.Items.Add("yes");
                    lstType.Items.Add("no");
                    m_booleanEditor = true;
                    break;
                case "objects":
                    DropDownObjectsControl newDropDownObjects = new DropDownObjectsControl();
                    newDropDownObjects.Helper.DoInitialise(m_helper.Controller, m_helper.ControlDefinition);
                    newDropDownObjects.Helper.Dirty += SimpleEditor_Dirty;
                    newDropDownObjects.lstDropdown.SelectionChanged += DropDownObjects_SelectionChanged;
                    m_simpleEditor = newDropDownObjects;
                    break;
                case "number":
                    NumberControl newNumber = new NumberControl();
                    newNumber.Helper.DoInitialise(m_helper.Controller, m_helper.ControlDefinition);
                    newNumber.Helper.Dirty += SimpleEditor_Dirty;
                    newNumber.LostFocus += SimpleEditor_LostFocus;
                    m_simpleEditor = newNumber;
                    break;
                case "numberdouble":
                    NumberDoubleControl newNumberDouble = new NumberDoubleControl();
                    newNumberDouble.Helper.DoInitialise(m_helper.Controller, m_helper.ControlDefinition);
                    newNumberDouble.Helper.Dirty += SimpleEditor_Dirty;
                    newNumberDouble.LostFocus += SimpleEditor_LostFocus;
                    m_simpleEditor = newNumberDouble;
                    break;
                case "dropdown":
                    DropDownControl newDropDown = new DropDownControl();
                    newDropDown.Helper.DoInitialise(m_helper.Controller, m_helper.ControlDefinition);
                    newDropDown.Helper.Dirty += SimpleEditor_Dirty;
                    newDropDown.lstDropdown.SelectionChanged += DropDown_SelectionChanged;
                    newDropDown.LostFocus += SimpleEditor_LostFocus;
                    m_simpleEditor = newDropDown;
                    break;
                default:
                    throw new InvalidOperationException("Invalid control type for expression");
            }

            if (m_simpleEditor != null)
            {
                m_simpleEditor.MinWidth = 40;
                Grid.SetRow(m_simpleEditor, Grid.GetRow(txtExpression));
                Grid.SetColumn(m_simpleEditor, Grid.GetColumn(txtExpression));
                grid.Children.Add(m_simpleEditor);
            }

            if (m_simpleEditor != null)
            {
                lstType.Items.Add(m_helper.ControlDefinition.GetString("simple"));
            }
            lstType.Items.Add("expression");
            m_updatingList = false;
        }

        private void UninitialiseSimpleEditor()
        {
            TextBox textBox = m_simpleEditor as TextBox;
            if (textBox != null)
            {
                textBox.TextChanged -= SimpleEditor_TextChanged;
                textBox.LostFocus -= SimpleEditor_LostFocus;
            }

            FileControl fileControl = m_simpleEditor as FileControl;
            if (fileControl != null)
            {
                fileControl.Helper.DoUninitialise();
                fileControl.Helper.Dirty -= SimpleEditor_Dirty;
                fileControl.SelectionChanged -= FileControl_SelectionChanged;
            }

            DropDownObjectsControl dropDownObjects = m_simpleEditor as DropDownObjectsControl;
            if (dropDownObjects != null)
            {
                dropDownObjects.Helper.DoUninitialise();
                dropDownObjects.Helper.Dirty -= SimpleEditor_Dirty;
                dropDownObjects.lstDropdown.SelectionChanged -= DropDownObjects_SelectionChanged;
            }

            NumberControl number = m_simpleEditor as NumberControl;
            if (number != null)
            {
                number.Helper.DoUninitialise();
                number.Helper.Dirty -= SimpleEditor_Dirty;
                number.LostFocus -= SimpleEditor_LostFocus;
            }

            NumberDoubleControl numberDouble = m_simpleEditor as NumberDoubleControl;
            if (numberDouble != null)
            {
                numberDouble.Helper.DoUninitialise();
                numberDouble.Helper.Dirty -= SimpleEditor_Dirty;
                numberDouble.LostFocus -= SimpleEditor_LostFocus;
            }

            DropDownControl dropDown = m_simpleEditor as DropDownControl;
            if (dropDown != null)
            {
                dropDown.Helper.DoUninitialise();
                dropDown.Helper.Dirty -= SimpleEditor_Dirty;
                dropDown.lstDropdown.SelectionChanged -= DropDown_SelectionChanged;
                dropDown.LostFocus -= SimpleEditor_LostFocus;
            }
        }

        void DropDownObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DropDownObjectsControl)m_simpleEditor).IsUpdatingList) return;
            SimpleEditor_SelectionChanged();
        }

        void DropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((DropDownControl)m_simpleEditor).IsUpdatingList) return;
            SimpleEditor_SelectionChanged();
        }

        void FileControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((FileControl)m_simpleEditor).IsUpdatingList) return;
            SimpleEditor_SelectionChanged();
        }

        private void SimpleEditor_SelectionChanged()
        {
            m_helper.SetDirty(ConvertFromSimpleExpression(SimpleValue));
            if (m_data.IsDirectlySaveable)
            {
                Save();
            }
            else
            {
                m_helper.RaiseRequestParentElementEditorSaveEvent();
            }
        }

        void SimpleEditor_Dirty(object sender, DataModifiedEventArgs e)
        {
            if (m_simpleEditor is DropDownControl)
            {
                m_helper.SetDirty(e.NewValue.ToString());
                // Don't want to save as that will cause repopulation, which is annoying if we're typing something in to a freetext dropdown
            }
            if (m_simpleEditor is NumberControl || m_simpleEditor is NumberDoubleControl)
            {
                m_helper.SetDirty(e.NewValue.ToString());
                Save();
            }
            else
            {
                m_helper.RaiseDirtyEvent(e.NewValue);
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
            m_data = data;
            if (data == null) return;
            m_helper.StartPopulating();

            string value = m_helper.Populate(data) ?? string.Empty;

            // Don't change the SimpleMode setting if we're just repopulating because this expression has
            // been saved
            if (!m_saving)
            {
                SimpleMode = IsSimpleExpression(value);
                TemplateMode = IsTemplateExpression(value);
            }

            PopulateSimpleControl();

            if (SimpleMode)
            {
                SimpleValue = ConvertToSimpleExpression(value);
            }

            if (TemplateMode)
            {
                PopulateTemplate(value);
            }

            txtExpression.Text = value;

            txtExpression.IsEnabled = m_helper.CanEdit(data);
            txtExpression.IsReadOnly = data.ReadOnly;

            lstType.IsEnabled = txtExpression.IsEnabled && !data.ReadOnly;
            lstTemplate.IsEnabled = txtExpression.IsEnabled && !data.ReadOnly;
            cmdInsert.IsEnabled = txtExpression.IsEnabled && !data.ReadOnly;

            if (m_simpleEditor is TextBox)
            {
                ((TextBox)m_simpleEditor).IsReadOnly = data.ReadOnly;
            }
            else
            {
                if (m_simpleEditor != null) m_simpleEditor.IsEnabled = !data.ReadOnly;
            }

            m_helper.FinishedPopulating();
        }

        private void PopulateSimpleControl()
        {
            if (m_simpleEditor is DropDownObjectsControl)
            {
                ((DropDownObjectsControl)m_simpleEditor).PopulateList();
            }
        }

        public void Save()
        {
            bool save = true;
            if (!m_helper.IsDirty) return;
            m_saving = true;
            string saveValue = null;
            if (SimpleMode)
            {
                saveValue = ConvertFromSimpleExpression(SimpleValue);
            }
            else if (TemplateMode)
            {
                saveValue = m_templateEditor.SaveExpression();
            }
            else
            {
                saveValue = txtExpression.Text;
            }
            ValidationResult result = m_helper.Controller.ValidateExpression(saveValue);
            save = result.Valid;
            if (!result.Valid)
            {
                PopupEditors.DisplayValidationError(result, saveValue, "Invalid expression");
            }
            if (m_helper.ControlDefinition.GetBool("nullable") && string.IsNullOrEmpty(saveValue))
            {
                saveValue = null;
            }
            if (save)
            {
                m_helper.Save(saveValue);
            }
            m_saving = false;
            if (!save)
            {
                Populate(m_data);
            }
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
                if (m_booleanEditor)
                {
                    if (m_simpleMode)
                    {
                        // forces the dropdown to set to the correct value
                        SimpleValue = SimpleValue;
                    }
                    else
                    {
                        lstType.SelectedIndex = 2;
                    }
                }
                else
                {
                    lstType.SelectedIndex = m_simpleMode ? 0 : 1;
                }
                m_updatingList = false;

                SetExpressionVisibility(!m_simpleMode);

                if (m_simpleEditor != null)
                {
                    ((Control)m_simpleEditor).Visibility = m_simpleMode ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private bool TemplateMode
        {
            get { return UseExpressionTemplates ? m_templateMode : false; }
            set
            {
                if (!UseExpressionTemplates) return;

                m_templateMode = value;

                if (!m_templateMode)
                {
                    lstTemplate.SelectedIndex = 0;
                }

                SetExpressionVisibility(!m_templateMode);

                if (m_templateEditor != null)
                {
                    m_templateEditor.Visibility = m_templateMode ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void SetExpressionVisibility(bool visible)
        {
            Visibility visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            txtExpression.Visibility = visibility;
            cmdInsert.Visibility = visibility;
        }

        private void lstType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_updatingList) return;

            if (m_booleanEditor)
            {
                if (lstType.SelectedIndex <= 1)
                {
                    m_helper.SetDirty(SimpleValue);
                    txtExpression.Text = SimpleValue;
                }
            }

            Save();

            if (m_booleanEditor)
            {
                SimpleMode = (lstType.SelectedIndex <= 1);
            }
            else
            {
                SimpleMode = (lstType.SelectedIndex == 0);
            }

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
                if (!m_booleanEditor)
                {
                    txtExpression.Text = ConvertFromSimpleExpression(SimpleValue);
                }
            }
        }

        private bool IsSimpleExpression(string expression)
        {
            if (!IsSimpleModeAvailable) return false;

            if (m_booleanEditor)
            {
                return (expression == "true" || expression == "false");
            }

            if (m_simpleEditor is DropDownObjectsControl)
            {
                return expression.Length == 0 || ((DropDownObjectsControl)m_simpleEditor).GetValidNames().Contains(expression);
            }

            if (m_simpleEditor is NumberControl)
            {
                int number;
                return int.TryParse(expression, out number);
            }

            if (m_simpleEditor is NumberDoubleControl)
            {
                double number;
                return double.TryParse(expression, out number);
            }

            return EditorUtility.IsSimpleStringExpression(expression);
        }

        private string ConvertToSimpleExpression(string expression)
        {
            if (!IsSimpleExpression(expression)) return string.Empty;

            if (m_booleanEditor
                || m_simpleEditor is DropDownObjectsControl
                || m_simpleEditor is NumberControl
                || m_simpleEditor is NumberDoubleControl
                )
            {
                return expression;
            }

            return EditorUtility.ConvertToSimpleStringExpression(expression);
        }

        private string ConvertFromSimpleExpression(string simpleValue)
        {
            if (simpleValue == null) return string.Empty;

            if (m_booleanEditor
                || m_simpleEditor is DropDownObjectsControl
                || m_simpleEditor is NumberControl
                || m_simpleEditor is NumberDoubleControl
                )
            {
                return simpleValue;
            }

            return EditorUtility.ConvertFromSimpleStringExpression(simpleValue);
        }

        private string SimpleValue
        {
            get
            {
                if (m_simpleEditor is TextBox)
                {
                    return ((TextBox)m_simpleEditor).Text;
                }
                else if (m_simpleEditor is FileControl)
                {
                    return ((FileControl)m_simpleEditor).Filename;
                }
                else if (m_booleanEditor)
                {
                    return lstType.SelectedIndex == 0 ? "true" : "false";
                }
                else if (m_simpleEditor is DropDownObjectsControl)
                {
                    return ((DropDownObjectsControl)m_simpleEditor).SelectedItem;
                }
                else if (m_simpleEditor is DropDownControl)
                {
                    return ((DropDownControl)m_simpleEditor).SelectedItem;
                }
                else if (m_simpleEditor is NumberControl)
                {
                    return ((NumberControl)m_simpleEditor).StringValue;
                }
                else if (m_simpleEditor is NumberDoubleControl)
                {
                    return ((NumberDoubleControl)m_simpleEditor).StringValue;
                }
                throw new InvalidOperationException("Unknown control type");
            }
            set
            {
                if (m_simpleEditor is TextBox)
                {
                    ((TextBox)m_simpleEditor).Text = value;
                }
                else if (m_simpleEditor is FileControl)
                {
                    ((FileControl)m_simpleEditor).Filename = value;
                }
                else if (m_booleanEditor)
                {
                    bool oldValue = m_updatingList;
                    m_updatingList = true;
                    lstType.SelectedIndex = (value == "true") ? 0 : 1;
                    m_updatingList = oldValue;
                }
                else if (m_simpleEditor is DropDownObjectsControl)
                {
                    ((DropDownObjectsControl)m_simpleEditor).SelectedItem = value;
                }
                else if (m_simpleEditor is DropDownControl)
                {
                    ((DropDownControl)m_simpleEditor).SelectedItem = value;
                }
                else if (m_simpleEditor is NumberControl)
                {
                    int number;
                    if (!int.TryParse(value, out number))
                    {
                        value = "0";
                    }
                    ((NumberControl)m_simpleEditor).StringValue = value;
                }
                else if (m_simpleEditor is NumberDoubleControl)
                {
                    double number;
                    if (!double.TryParse(value, out number))
                    {
                        value = "0.0";
                    }
                    ((NumberDoubleControl)m_simpleEditor).StringValue = value;
                }
                else
                {
                    throw new InvalidOperationException("Unknown control type");
                }
            }
        }

        public bool IsSimpleModeAvailable
        {
            get { return m_isSimpleModeAvailable; }
            set
            {
                m_isSimpleModeAvailable = value;
                UpdateListVisibility();
            }
        }

        public bool UseExpressionTemplates
        {
            get { return m_useExpressionTemplates; }
            set
            {
                m_useExpressionTemplates = value;
                UpdateListVisibility();
            }
        }

        private bool IsTemplateExpression(string expression)
        {
            return m_helper.Controller.GetExpressionEditorDefinition(expression, ExpressionTypeTemplateFilter) != null;
        }

        private void UpdateListVisibility()
        {
            lstType.Visibility = (IsSimpleModeAvailable && !UseExpressionTemplates) ? Visibility.Visible : Visibility.Collapsed;
            lstTemplate.Visibility = UseExpressionTemplates ? Visibility.Visible : Visibility.Collapsed;

            Binding heightBinding = new Binding(string.Format("ElementName={0}, Path=ActualHeight", UseExpressionTemplates ? "lstTemplate" : "lstType"));
            cmdInsert.SetBinding(Button.HeightProperty, heightBinding);
        }

        private void PopulateExpressionTemplates()
        {
            if (lstTemplate.Items.Count > 0) return;

            lstTemplate.Items.Add("expression");

            foreach (string item in m_helper.Controller.GetExpressionEditorNames(ExpressionTypeTemplateFilter))
            {
                lstTemplate.Items.Add(item);
            }
        }

        private void cmdInsert_Click(object sender, RoutedEventArgs e)
        {
            TextOptions.SetTextFormattingMode(mnuInsertMenu, TextFormattingMode.Display);
            mnuInsertMenu.IsOpen = true;
        }

        private void InitialiseInsertMenu()
        {
            AddInsertMenuItem("(Clear)", ClearText);
            mnuInsertMenu.Items.Add(new Separator());
            AddInsertMenuItem("Variable", InsertVariable);
            AddInsertMenuItem("Object", InsertObject);
            AddInsertMenuItem("Attribute", InsertProperty);
            AddInsertMenuItem("Function", InsertFunction);
            mnuInsertMenu.Items.Add(new Separator());
            AddInsertMenuItem("and", () => InsertString(" and "));
            AddInsertMenuItem("or", () => InsertString(" or "));
            AddInsertMenuItem("+ add", () => InsertString(" + "));
            AddInsertMenuItem("- subtract", () => InsertString(" - "));
            AddInsertMenuItem("* multiply", () => InsertString(" * "));
            AddInsertMenuItem("/ divide", () => InsertString(" / "));
            AddInsertMenuItem("= equals", () => InsertString(" = "));
            AddInsertMenuItem("<> not equals", () => InsertString(" <> "));
            AddInsertMenuItem("> greater than", () => InsertString(" > "));
            AddInsertMenuItem("< less than", () => InsertString(" < "));
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
            InsertFromList("a variable", m_data.GetVariablesInScope());
        }

        private void InsertObject()
        {
            InsertFromList("an object", m_helper.Controller.GetObjectNames("object", true));
        }

        private void InsertProperty()
        {
            InsertFromList("an attribute", m_helper.Controller.GetPropertyNames().OrderBy(n => n));
        }

        private void InsertFunction()
        {
            IEnumerable<string> coreFunctions = m_helper.Controller.GetElementNames("function", true);
            IEnumerable<string> builtInFunctions = m_helper.Controller.GetBuiltInFunctionNames();
            IEnumerable<string> allFunctions = coreFunctions.Union(builtInFunctions);

            InsertFromList("a function", allFunctions.OrderBy(n => n));
        }

        private void InsertFromList(string itemName, IEnumerable<string> items)
        {
            var result = PopupEditors.EditStringWithDropdown(
                string.Format("Please enter {0} name", itemName),
                string.Empty, null, null, "",
                items);

            if (result.Cancelled)
            {
                txtExpression.Focus();
                return;
            }

            InsertString(result.Result);
        }

        private void InsertString(string text)
        {
            int index = txtExpression.SelectionStart;
            txtExpression.Text = txtExpression.Text.Insert(index, text);
            txtExpression.SelectionStart = index + text.Length;
            txtExpression.Focus();
        }

        private void ClearText()
        {
            txtExpression.Text = "";
            txtExpression.Focus();
        }

        public Control FocusableControl
        {
            get
            {
                if (SimpleMode)
                {
                    return m_simpleEditor;
                }
                else
                {
                    return txtExpression;
                }
            }

        }

        private void lstTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_updatingList) return;

            Save();

            TemplateMode = (lstTemplate.SelectedIndex > 0);

            if (TemplateMode)
            {
                string expression = m_helper.Controller.GetNewExpression((string)lstTemplate.Items[lstTemplate.SelectedIndex]);
                PopulateTemplate(expression);
                m_helper.SetDirty(expression);
                Save();
            }
            else
            {
                txtExpression.Text = m_helper.Populate(m_data);
            }
        }

        private void PopulateTemplate(string expression)
        {
            if (m_templateEditor == null)
            {
                m_templateEditor = new ExpressionTemplate();
                m_templateEditor.Controller = m_helper.Controller;
                m_templateEditor.Dirty += m_templateEditor_Dirty;
                m_templateEditor.RequestSave += m_templateEditor_RequestSave;
                m_templateEditor.ExpressionTypeTemplateFilter = ExpressionTypeTemplateFilter;
                Grid.SetRow(m_templateEditor, Grid.GetRow(txtExpression));
                Grid.SetColumn(m_templateEditor, Grid.GetColumn(txtExpression));
                grid.Children.Add(m_templateEditor);
            }

            IEditorDefinition definition = m_helper.Controller.GetExpressionEditorDefinition(expression, ExpressionTypeTemplateFilter);

            m_updatingList = true;
            lstTemplate.Text = m_helper.Controller.GetExpressionEditorDefinitionName(expression, ExpressionTypeTemplateFilter);
            m_updatingList = false;

            m_templateEditor.Initialise(definition, expression, m_data);
        }

        void m_templateEditor_Dirty(string newValue)
        {
            m_helper.SetDirty(newValue);
        }

        void m_templateEditor_RequestSave()
        {
            Save();
        }

        public string ExpressionTypeTemplateFilter
        {
            get { return m_templatesFilter; }
            set { m_templatesFilter = value; }
        }
    }
}
