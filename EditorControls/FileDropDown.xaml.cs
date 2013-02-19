using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace TextAdventures.Quest.EditorControls
{
    public partial class FileDropDown : UserControl
    {
        private bool m_updatingList;
        private EditorController m_controller;

        public FileDropDown()
        {
            InitializeComponent();
        }

        public void Initialise(EditorController controller)
        {
            m_controller = controller;
            FileLister = GetFilesInGamePath;
        }

        private IEnumerable<string> GetFilesInGamePath()
        {
            yield return "";
            foreach (string result in m_controller.GetAvailableExternalFiles(Source))
            {
                yield return result;
            }
        }

        public bool Preview { get; set; }
        public Func<IEnumerable<string>> FileLister { get; set; }
        public string FileFilter { get; set; }
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public Action<string> FilenameUpdated { get; set; }
        public string BasePath { get; set; }
        public string Source { get; set; }

        private class FileListItem
        {
            public string Title { get; set; }
            public string Filename { get; set; }
            public ImageSource Image { get; set; }
            public Visibility ImageVisibility { get; set; }
        }

        private List<FileListItem> m_fileListItems;

        public void RefreshFileList()
        {
            m_updatingList = true;
            lstFiles.Items.Clear();
            m_fileListItems = new List<FileListItem>();
            foreach (string file in FileLister())
            {
                BitmapImage bitmap = null;
                if (Preview && !string.IsNullOrEmpty(file))
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file);
                    bitmap.EndInit();
                }

                var item = new FileListItem
                {
                    Filename = Path.GetFileName(file),
                    Image = bitmap,
                    ImageVisibility = (bitmap == null) ? Visibility.Collapsed : Visibility.Visible
                };

                if (string.IsNullOrEmpty(item.Filename))
                {
                    item.Title = "None";
                }
                else
                {
                    item.Title = item.Filename;
                }

                lstFiles.Items.Add(item);
                m_fileListItems.Add(item);
            }
            m_updatingList = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlgOpenFile = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false,
                Filter = FileFilter,
                FileName = ""
            };
            dlgOpenFile.ShowDialog();

            if (dlgOpenFile.FileName.Length > 0)
            {
                string gameFolder = BasePath;
                string filename = Path.GetFileName(dlgOpenFile.FileName);
                string destFile = Path.Combine(gameFolder, filename);
                bool copyRequired = true;
                bool allowOverwrite = false;

                if (File.Exists(destFile))
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

                RefreshFileList();
                Filename = filename;
                if (FilenameUpdated != null)
                {
                    FilenameUpdated(filename);
                }
            }
        }

        private string GetUniqueFilename(string filename)
        {
            int i = 1;
            string directory = Path.GetDirectoryName(filename);
            string baseFilename = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string newFilename;
            do
            {
                i++;
                newFilename = Path.Combine(directory, baseFilename + " " + i.ToString() + extension);
            }
            while (File.Exists(newFilename));
            return newFilename;
        }

        public string Filename
        {
            get
            {
                if (lstFiles.SelectedItem == null) return string.Empty;
                return ((FileListItem)lstFiles.SelectedItem).Filename;
            }
            set
            {
                if (value == null) value = string.Empty;
                lstFiles.SelectedItem = m_fileListItems.FirstOrDefault(f => f.Filename == value);
                if (lstFiles.SelectedItem == null)
                {
                    // add a new temporary item. This may be needed if file does not exist or file is in a parent directory
                    var item = new FileListItem
                    {
                        Filename = value,
                        Title = value,
                        ImageVisibility = Visibility.Collapsed
                    };
                    lstFiles.Items.Add(item);
                    lstFiles.SelectedItem = item;
                }
            }
        }

        public bool IsUpdatingList
        {
            get { return m_updatingList; }
        }

        private void lstFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null) SelectionChanged(sender, e);
        }

        public bool Enabled
        {
            get { return lstFiles.IsEnabled; }
            set { lstFiles.IsEnabled = value; }
        }

        public Control FocusableControl
        {
            get
            {
                return lstFiles; 
            }
        }
    }
}
