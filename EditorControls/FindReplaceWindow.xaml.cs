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
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace AxeSoftware.Quest.EditorControls
{
    public partial class FindReplaceWindow : Window
    {
        [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
        private extern static Int32 GetWindowLongPtr(IntPtr hWnd, Int32 nIndex);

        [DllImport("User32.dll", EntryPoint = "SetWindowLong")]
        private extern static Int32 SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

        private const Int32 GWL_STYLE = -16;
        private const Int32 WS_MAXIMIZEBOX = 0x00010000;
        private const Int32 WS_MINIMIZEBOX = 0x00020000;

        public FindReplaceWindow()
        {
            InitializeComponent();
            this.Loaded += FindReplaceWindow_Loaded;
        }

        void FindReplaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DisableMinimize(this);
        }

        public static void DisableMinimize(Window window)
        {
            lock (window)
            {
                IntPtr hWnd = new WindowInteropHelper(window).Handle;
                Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
                SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX);
            }
        }
    }
}
