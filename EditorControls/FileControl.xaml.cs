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

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("file")]
    public partial class FileControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private string m_fileFilter;
        private string m_source;
        private Func<IEnumerable<string>> m_fileLister;
        private IEditorData m_data;
        private bool m_updatingList;

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
            if (m_data == null) return;
            if (!m_helper.IsDirty) return;
            string saveValue = (string)lstFiles.SelectedItem;
            m_helper.Save(saveValue);
        }

        public void RefreshFileList()
        {
            m_updatingList = true;
            lstFiles.Items.Clear();
            foreach (string file in m_fileLister.Invoke())
            {
                lstFiles.Items.Add(file);
            }
            m_updatingList = false;
        }

        private IEnumerable<string> GetAvailableLibraries()
        {
            return m_helper.Controller.GetAvailableLibraries();
        }

        private IEnumerable<string> GetFilesInGamePath()
        {
            yield return "";
            foreach (string result in m_helper.Controller.GetAvailableExternalFiles(m_source))
            {
                yield return result;
            }
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
                string destFile = Path.Combine(gameFolder, filename);
                bool copyRequired = true;
                bool allowOverwrite = false;

                if (System.IO.File.Exists(destFile))
                {
                    if (Utility.Utility.AreFilesEqual(dlgOpenFile.FileName, destFile))
                    {
                        copyRequired = false;
                    }
                    else
                    {
                        var result = MessageBox.Show(string.Format("A different file called {0} already exists in the game folder.\n\nWould you like to overwrite it?",
                                              filename),
                                              "Overwrite file?",
                                              MessageBoxButton.YesNoCancel,
                                              MessageBoxImage.Exclamation);

                        switch (result)
                        {
                            case MessageBoxResult.Yes:
                                allowOverwrite = true;
                                break;
                            case MessageBoxResult.No:
                                destFile = GetUniqueFilename(destFile);
                                filename = Path.GetFileName(destFile);
                                break;
                            case MessageBoxResult.Cancel:
                                return;
                        }
                    }
                }

                if (copyRequired)
                {
                    try
                    {
                        File.Copy(dlgOpenFile.FileName, destFile, allowOverwrite);
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
                }

                if (m_data != null)
                {
                    m_helper.Controller.StartTransaction(String.Format("Set filename to '{0}'", filename));
                    m_data.SetAttribute(m_helper.ControlDefinition.Attribute, filename);
                    m_helper.Controller.EndTransaction();
                    Populate(m_data);
                }
                else
                {
                    // m_data is null if this control is being used in an ExpressionControl, so in this case
                    // just set the list text
                    RefreshFileList();
                    lstFiles.Text = filename;
                }
            }
        }

        private string GetUniqueFilename(string filename)
        {
            int i = 1;
            string directory = System.IO.Path.GetDirectoryName(filename);
            string baseFilename = System.IO.Path.GetFileNameWithoutExtension(filename);
            string extension = System.IO.Path.GetExtension(filename);
            string newFilename;
            do
            {
                i++;
                newFilename = System.IO.Path.Combine(directory, baseFilename + " " + i.ToString() + extension);
            }
            while (System.IO.File.Exists(newFilename));
            return newFilename;
        }

        public string Filename
        {
            get
            {
                return (string)lstFiles.SelectedItem;
            }
            set
            {
                lstFiles.Text = value;
            }
        }

        public bool IsUpdatingList
        {
            get { return m_updatingList; }
        }

        public Control FocusableControl
        {
            get { return lstFiles; }
        }
    }
}
