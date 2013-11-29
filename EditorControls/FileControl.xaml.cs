using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("file")]
    public partial class FileControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private IEditorData m_data;

        public FileControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            string source = m_helper.ControlDefinition.GetString("source");
            fileDropDown.BasePath = System.IO.Path.GetDirectoryName(m_helper.Controller.Filename);
            fileDropDown.Source = source;
            fileDropDown.Initialise(m_helper.Controller);
            
            fileDropDown.Preview = m_helper.ControlDefinition.GetBool("preview");

            if (source == "libraries")
            {
                source = "*.aslx";
                fileDropDown.FileLister = GetAvailableLibraries;
            }

            fileDropDown.FileFilter = string.Format("{0} ({1})|{1}", m_helper.ControlDefinition.GetString("filefiltername"), source);

            fileDropDown.ShowNewButton = !string.IsNullOrEmpty(m_helper.ControlDefinition.GetString("newfile"));
            fileDropDown.DefaultFilename = m_helper.ControlDefinition.GetString("newfile");
        }

        private void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_helper.SetDirty(fileDropDown.Filename);
            Save();
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
            fileDropDown.RefreshFileList();
            fileDropDown.Filename = m_helper.Populate(data);
            fileDropDown.Enabled = m_helper.CanEdit(data) && !data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (m_data == null) return;
            if (!m_helper.IsDirty) return;
            string saveValue = Filename;
            m_helper.Save(saveValue);
        }

        private IEnumerable<string> GetAvailableLibraries()
        {
            yield return "";
            foreach (string result in m_helper.Controller.GetAvailableLibraries())
            {
                yield return result;
            }
        }

        public void RefreshFileList()
        {
            fileDropDown.RefreshFileList();
        }

        public string Filename
        {
            get { return fileDropDown.Filename; }
            set { fileDropDown.Filename = value; }
        }

        private void FilenameUpdated(string filename)
        {
            if (m_data != null)
            {
                m_helper.Controller.StartTransaction(String.Format("Set filename to '{0}'", filename));
                m_data.SetAttribute(m_helper.ControlDefinition.Attribute, filename);
                m_helper.Controller.EndTransaction();
                Populate(m_data);
            }
        }

        public bool IsUpdatingList
        {
            get { return fileDropDown.IsUpdatingList; }
        }

        public Control FocusableControl
        {
            get { return fileDropDown.FocusableControl; }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged
        {
            add
            {
                fileDropDown.SelectionChanged += value;
            }
            remove
            {
                fileDropDown.SelectionChanged -= value;
            }
        }
    }
}
