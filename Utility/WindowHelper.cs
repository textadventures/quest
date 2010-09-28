using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Utility
{
    public class WindowHelper
    {
        private Microsoft.Win32.RegistryKey m_key;
        private System.Windows.Forms.Form m_form;
        private string m_topKey;
        private string m_leftKey;
        private string m_heightKey;
        private string m_widthKey;

        public WindowHelper(System.Windows.Forms.Form form, string product, string keyPrefix)
        {
            m_key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + product + @"\Window");
            m_form = form;

            m_topKey = keyPrefix + "Top";
            m_leftKey = keyPrefix + "Left";
            m_heightKey = keyPrefix + "Height";
            m_widthKey = keyPrefix + "Width";

            form.Load += new EventHandler(form_Load);
        }

        void form_Load(object sender, EventArgs e)
        {
            object top = m_key.GetValue(m_topKey, null);
            object left = m_key.GetValue(m_leftKey, null);
            object height = m_key.GetValue(m_heightKey, null);
            object width = m_key.GetValue(m_widthKey, null);

            if (top != null) m_form.Top = (int)top;
            if (left != null) m_form.Left = (int)left;
            if (height != null) m_form.Height = (int)height;
            if (width != null) m_form.Width = (int)width;

            m_form.ResizeEnd += SavePosition;
            m_form.Move += SavePosition;
        }

        void SavePosition(object sender, EventArgs e)
        {
            if (m_form.WindowState != System.Windows.Forms.FormWindowState.Normal) return;
            m_key.SetValue(m_topKey, m_form.Top);
            m_key.SetValue(m_leftKey, m_form.Left);
            m_key.SetValue(m_heightKey, m_form.Height);
            m_key.SetValue(m_widthKey, m_form.Width);
        }
    }

    public class SplitterHelper
    {
        private Microsoft.Win32.RegistryKey m_key;
        private System.Windows.Forms.SplitContainer m_splitter;
        private string m_regValue;

        public SplitterHelper(System.Windows.Forms.SplitContainer splitter, string product, string regValue)
        {
            m_key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + product + @"\Window");
            m_splitter = splitter;
            m_regValue = regValue;
        }

        // Annoyingly we have to call this manually, otherwise the splitter moves further and further to the
        // right. I think this is because it sets the position *before* the stored form size is loaded, and
        // the splitter moves to keep the relative proportions, so it gets distorted. There is probably a
        // neater way around this, but this is good enough for now.
        public void LoadSplitterPositions()
        {
            object distance = m_key.GetValue(m_regValue, null);
            if (distance != null) m_splitter.SplitterDistance = (int)distance;

            m_splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(m_splitter_SplitterMoved);
        }

        void m_splitter_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
        {
            if (m_splitter.Visible)
                m_key.SetValue(m_regValue, m_splitter.SplitterDistance);
        }
    }
}
