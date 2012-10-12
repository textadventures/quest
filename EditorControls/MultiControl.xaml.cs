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

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("multi")]
    public partial class MultiControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        private static Dictionary<string, string> s_controlTypesMap = new Dictionary<string, string> {
            {"boolean", "checkbox"},
            {"string", "textbox"},
            {"script", "script"},
            {"stringlist", "list"},
            {"int", "number"},
            {"double", "numberdouble"},
            {"object", "objects"},
            {"simplepattern", "pattern"},
            {"stringdictionary", "stringdictionary"},
            {"scriptdictionary", "scriptdictionary"},
            {"null", null}
        };

        private static Dictionary<Type, string> s_typeNamesMap = new Dictionary<Type, string> {
            {typeof(bool), "boolean"},
            {typeof(string), "string"},
            {typeof(IEditableScripts), "script"},
            {typeof(IEditableList<string>), "stringlist"},
            {typeof(int), "int"},
            {typeof(double), "double"},
            {typeof(IEditableObjectReference), "object"},
            {typeof(IEditableCommandPattern), "simplepattern"},
            {typeof(IEditableDictionary<string>),"stringdictionary"},
            {typeof(IEditableDictionary<IEditableScripts>), "scriptdictionary"}
        };

        private Dictionary<string, object> m_storedValues = new Dictionary<string, object>();
        private IDictionary<string, string> m_overrideControlTypesMap;

        private EditorController m_controller;
        private IEditorControl m_definition;
        private ControlDataOptions m_options = new ControlDataOptions();
        private IEditorData m_data;
        private Dictionary<string, Control> m_loadedEditors = new Dictionary<string, Control>();
        private Control m_currentEditor;
        private bool m_settingType = false;

        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;

        private struct TypesListItem
        {
            public int TypeIndex;
            public string TypeName;
            public string TypeDescription;
        }

        private List<TypesListItem> m_types;

        public MultiControl()
        {
            InitializeComponent();
        }

        private IElementEditorControl CurrentElementEditor
        {
            get { return m_currentEditor as IElementEditorControl; }
        }

        private Control CurrentEditor
        {
            get { return m_currentEditor; }
            set
            {
                if (CurrentElementEditor != null)
                {
                    CurrentElementEditor.Helper.Dirty -= CurrentElementEditor_Dirty;
                    CurrentElementEditor.Helper.RequestParentElementEditorSave -= CurrentElementEditor_RequestParentElementEditorSave;
                }

                m_currentEditor = value;

                if (CurrentElementEditor != null)
                {
                    CurrentElementEditor.Helper.Dirty += CurrentElementEditor_Dirty;
                    CurrentElementEditor.Helper.RequestParentElementEditorSave += CurrentElementEditor_RequestParentElementEditorSave;
                }
            }
        }

        void CurrentElementEditor_Dirty(object sender, DataModifiedEventArgs e)
        {
            Dirty(sender, e);
        }

        void CurrentElementEditor_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        private void lstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // only respond to user changes
            if (m_settingType) return;
            TypesListItem selectedType = m_types.First(s => s.TypeIndex == lstTypes.SelectedIndex);
            UserSelectedNewType(selectedType);
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            m_storedValues.Clear();

            if (m_data == null)
            {
                if (CurrentElementEditor != null) CurrentElementEditor.Populate(m_data);
                CurrentEditor = null;
                HideOtherEditors();
                return;
            }

            object value = m_data.GetAttribute(m_definition.Attribute);

            bool canEdit = CanEditType(value) && m_definition.Attribute != "type" && m_definition.Attribute != "elementtype";

            lstTypes.IsEnabled = canEdit && !data.ReadOnly && m_definition.Attribute != "name";

            if (canEdit)
            {
                string typeName = GetTypeName(value);
                string editorName = GetEditorNameForType(typeName);
                SetSelectedType(typeName);
                GetOrCreateEditorControl(editorName);
                if (!string.IsNullOrEmpty(typeName))
                {
                    m_storedValues[typeName] = value;
                }
            }
            else
            {
                CurrentEditor = null;
                HideOtherEditors();
            }
        }

        private string GetTypeName(object value)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            return s_typeNamesMap.FirstOrDefault(t => t.Key.IsAssignableFrom(type)).Value;
        }

        private string GetEditorNameForType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return string.Empty;
            if (m_overrideControlTypesMap != null)
            {
                if (m_overrideControlTypesMap.ContainsKey(typeName))
                {
                    return m_overrideControlTypesMap[typeName];
                }
            }
            return s_controlTypesMap[typeName];
        }

        private void SetSelectedType(string typeName)
        {
            if (m_types == null) return;
            if (string.IsNullOrEmpty(typeName)) return;

            var selectedType = m_types.First(t => t.TypeName == typeName);
            m_settingType = true;
            lstTypes.SelectedIndex = selectedType.TypeIndex;
            m_settingType = false;
        }

        private void GetOrCreateEditorControl(string editorName)
        {
            if (string.IsNullOrEmpty(editorName))
            {
                CurrentEditor = null;
            }
            else
            {
                if (!m_loadedEditors.ContainsKey(editorName))
                {
                    Control newControl = InitialiseEditorControl(editorName);
                    Grid.SetRow(newControl, 1);
                    grid.Children.Add(newControl);

                    CurrentEditor = newControl;

                    m_loadedEditors.Add(editorName, newControl);
                }
                else
                {
                    CurrentEditor = m_loadedEditors[editorName];
                }

                if (CurrentEditor != null)
                {
                    GridUnitType controlRowGridUnit = GridUnitType.Auto;

                    if (CurrentElementEditor != null)
                    {
                        CurrentElementEditor.Helper.DoInitialise(m_controller, m_definition);
                        CurrentElementEditor.Populate(m_data);
                        if (CurrentElementEditor.Helper.Options.Resizable || m_definition.Expand)
                        {
                            controlRowGridUnit = GridUnitType.Star;
                        }
                    }

                    CurrentEditor.Visibility = Visibility.Visible;
                    controlRow.Height = new GridLength(1, controlRowGridUnit);
                }

            }

            HideOtherEditors();
            SetEditorAttributes();
        }

        private void HideOtherEditors()
        {
            foreach (Control ctl in m_loadedEditors.Values)
            {
                if (ctl != CurrentEditor)
                {
                    ctl.Visibility = Visibility.Collapsed;
                    IElementEditorControl editorCtl = ctl as IElementEditorControl;
                    if (editorCtl != null)
                    {
                        editorCtl.Populate(null);
                        editorCtl.Helper.DoUninitialise();
                    }
                }
            }
        }

        private void SetEditorAttributes()
        {
            CheckBoxControl checkBoxControl = CurrentEditor as CheckBoxControl;

            if (checkBoxControl != null)
            {
                checkBoxControl.SetCaption(m_definition.GetString("checkbox"));
            }
        }

        private Control InitialiseEditorControl(string controlType)
        {
            Control newControl = ControlFactory.CreateEditorControl(m_controller, controlType);
            newControl.Margin = new Thickness(0, 8, 0, 0);
            return (Control)newControl;
        }

        public void Save()
        {
            if (CurrentElementEditor != null)
            {
                CurrentElementEditor.Save();
            }
        }

        public Type ExpectedType
        {
            get { return null; }
        }

        public string Attribute
        {
            get { return m_definition.Attribute; }
        }

        public ControlDataOptions Options
        {
            get { return m_options; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
            m_definition = definition;

            m_settingType = true;
            lstTypes.Items.Clear();
            m_settingType = false;

            m_types = new List<TypesListItem>();

            if (m_definition != null)
            {
                IDictionary<string, string> types = definition.GetDictionary("types");
                InitialiseTypesList(types);

                m_overrideControlTypesMap = definition.GetDictionary("editors");

                string selfCaption = definition.GetString("selfcaption");
                if (selfCaption != null)
                {
                    lblSelfCaption.Visibility = Visibility.Visible;
                    lblSelfCaption.Text = selfCaption + ":";
                }
            }
        }

        public void DoUninitialise()
        {
            m_controller = null;
            m_definition = null;
            m_overrideControlTypesMap = null;
        }

        private void InitialiseTypesList(IDictionary<string, string> types)
        {
            int index = 0;

            foreach (var item in types)
            {
                lstTypes.Items.Add(item.Value);
                m_types.Add(new TypesListItem { TypeIndex = index, TypeName = item.Key, TypeDescription = item.Value });
                index += 1;
            }
        }

        private void UserSelectedNewType(TypesListItem type)
        {
            m_controller.StartTransaction(string.Format("Change type of '{0}' {1} to '{2}'", m_data.Name, m_definition.Attribute, type.TypeDescription));

            object newValue;

            // If the user has previously selected this type, use the previous value, otherwise create a new
            // default value for that type. This allows the user to switch back and forth between different
            // types without the value being cleared out if they change their mind.

            if (m_storedValues.ContainsKey(type.TypeName))
            {
                newValue = m_storedValues[type.TypeName];
            }
            else
            {
                switch (type.TypeName)
                {
                    case "boolean":
                        newValue = false;
                        break;
                    case "string":
                        newValue = "";
                        break;
                    case "int":
                        newValue = 0;
                        break;
                    case "double":
                        newValue = 0.0;
                        break;
                    case "script":
                        newValue = m_controller.CreateNewEditableScripts(m_data.Name, m_definition.Attribute, null, false);
                        break;
                    case "stringlist":
                        newValue = m_controller.CreateNewEditableList(m_data.Name, m_definition.Attribute, null, false);
                        break;
                    case "object":
                        newValue = m_controller.CreateNewEditableObjectReference(m_data.Name, m_definition.Attribute, false);
                        break;
                    case "simplepattern":
                        newValue = m_controller.CreateNewEditableCommandPattern(m_data.Name, m_definition.Attribute, "", false);
                        break;
                    case "stringdictionary":
                        newValue = m_controller.CreateNewEditableStringDictionary(m_data.Name, m_definition.Attribute, null, null, false);
                        break;
                    case "scriptdictionary":
                        newValue = m_controller.CreateNewEditableScriptDictionary(m_data.Name, m_definition.Attribute, null, null, false);
                        break;
                    case "null":
                        newValue = null;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            var result = m_data.SetAttribute(m_definition.Attribute, newValue);

            if (!result.Valid)
            {
                PopupEditors.DisplayValidationError(result, newValue as string, "Unable to set attribute value");
            }

            m_controller.EndTransaction();
        }

        private bool CanEditType(object value)
        {
            string typeName = GetTypeName(value);
            return m_types.Any(t => t.TypeName == typeName);
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
