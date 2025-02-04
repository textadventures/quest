using System.Linq;
using System.Windows;
using TextAdventures.Utility.Language;

namespace TextAdventures.Quest.EditorControls
{
    public partial class FilePopUp : Window
    {
        public FilePopUp()
        {
            InitializeComponent();
        }

        public void Initialise(EditorController controller)
        {
            fileDropDown.Initialise(controller);
            const string source = "*.jpg;*.jpeg;*.png;*.gif";
            fileDropDown.Source = source;
            fileDropDown.BasePath = System.IO.Path.GetDirectoryName(controller.Filename);
            fileDropDown.FileFilter = string.Format("{0} ({1})|{1}", "Picture Files", source);
            fileDropDown.Preview = true;
            fileDropDown.RefreshFileList();
            fileDropDown.Filename = null;
        }

        private void ok_OnClick(object sender, RoutedEventArgs e)
        {
            Filename = fileDropDown.Filename;
            Hide();
        }

        private void cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Filename = null;
            Hide();
        }

        public string Filename { get; private set; }

        private void ok_Initialized(object sender, System.EventArgs e)
        {
            ok.Content = L.T("EditorOkButton");
        }

        private void cancel_Initialized(object sender, System.EventArgs e)
        {
            cancel.Content = L.T("EditorCancelButton");
        }

        private void ImageSelectionWindow_Initialized(object sender, System.EventArgs e)
        {
            ImageSelectionWindow.Title = L.T("EditorImageSelectionWindowTitle");
        }
    }
}