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
using System.IO;

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("file")]
    public partial class FileControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private string m_fileFilter;
        private string m_source;
        private Func<IEnumerable<string>> m_fileLister;
        private IEditorData m_data;

        public FileControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            m_source = m_helper.ControlDefinition.GetString("source");
            if (m_source == "libraries")
            {
                m_source = "*.aslx";
                m_fileLister = GetAvailableLibraries;
            }
            else
            {
                m_fileLister = GetFilesInGamePath;
            }
            m_fileFilter = string.Format("{0} ({1})|{1}", m_helper.ControlDefinition.GetString("filefiltername"), m_source);
        }

        private void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_helper.SetDirty((string)lstFiles.SelectedItem);
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
            RefreshFileList();
            lstFiles.Text = m_helper.Populate(data);
            lstFiles.IsEnabled = m_helper.CanEdit(data) && !data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            string saveValue = (string)lstFiles.SelectedItem;
            m_helper.Save(saveValue);
        }

        private void RefreshFileList()
        {
            lstFiles.Items.Clear();
            foreach (string file in m_fileLister.Invoke())
            {
                lstFiles.Items.Add(file);
            }
        }

        private IEnumerable<string> GetAvailableLibraries()
        {
            return m_helper.Controller.GetAvailableLibraries();
        }

        private IEnumerable<string> GetFilesInGamePath()
        {
            return m_helper.Controller.GetAvailableExternalFiles(m_source);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlgOpenFile = new Microsoft.Win32.OpenFileDialog();
            dlgOpenFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dlgOpenFile.Multiselect = false;
            dlgOpenFile.Filter = m_fileFilter;
            dlgOpenFile.FileName = "";
            dlgOpenFile.ShowDialog();

            if (dlgOpenFile.FileName.Length > 0)
            {
                string gameFolder = Path.GetDirectoryName(m_helper.Controller.Filename);
                string filename = Path.GetFileName(dlgOpenFile.FileName);
                
                try
                {
                    File.Copy(dlgOpenFile.FileName, Path.Combine(gameFolder, filename));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Unable to copy file. The following error occurred:{0}",
                                          Environment.NewLine + Environment.NewLine + ex.Message),
                                          "Error copying file",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                    return;
                }

                m_helper.Controller.StartTransaction(String.Format("Set filename to '{0}'", filename));
                m_data.SetAttribute(m_helper.ControlDefinition.Attribute, filename);
                m_helper.Controller.EndTransaction();

                Populate(m_data);
            }
        }
    }
}
