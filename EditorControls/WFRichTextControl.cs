using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;

namespace AxeSoftware.Quest.EditorControls
{
    public partial class WFRichTextControl : UserControl
    {
        private string m_oldValue;
        private EditorController m_controller;
        private string m_attribute;
        private string m_attributeName;
        private IEditorData m_data;
        private IHTMLDocument2 m_document;

        private string m_valueToSet;
        private static Dictionary<string, string> s_htmlToXml = new Dictionary<string, string> {
		    {"strong","b"},
		    {"em","i"},
		    {"b","b"},
		    {"i","i"},
		    {"u","u"},
		    {"br","br"}
	    };

        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);

        public WFRichTextControl()
        {
            InitializeComponent();
            ctlWebBrowser.GotFocus += ctlWebBrowser_GotFocus;
            ctlWebBrowser.LostFocus += ctlWebBrowser_LostFocus;
            ctlWebBrowser.Navigated += ctlWebBrowser_Navigated;
            butBold.Click += butBold_Click;
            butItalic.Click += butItalic_Click;
            butUnderline.Click += butUnderline_Click;

            SetUpBrowser();
        }

        public object Value
        {
            get { return GetValue(); }
            set
            {
                string stringValue = value as string;
                if (stringValue != null)
                {
                    HTML = ConvertXMLtoHTML(stringValue);
                }
                else
                {
                    HTML = string.Empty;
                }
                m_oldValue = stringValue;
            }
        }

        private string GetValue()
        {
            return ConvertHTMLtoXML(HTML);
        }

        public bool IsDirty
        {
            get { return GetValue() != m_oldValue; }
        }

        public string AttributeName
        {
            get { return m_attribute; }
        }

        public void Save(IEditorData data)
        {
            if (IsDirty)
            {
                string description = string.Format("Set {0} to '{1}'", m_attributeName, Value);
                m_controller.StartTransaction(description);
                data.SetAttribute(AttributeName, Value);
                m_controller.EndTransaction();
                // reset the dirty flag
                Value = Value;
                System.Diagnostics.Debug.Assert(!IsDirty);
            }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data != null)
            {
                ctlWebBrowser.Visible = !data.ReadOnly;
                butBold.Enabled = !data.ReadOnly;
                butItalic.Enabled = !data.ReadOnly;
                butUnderline.Enabled = !data.ReadOnly;
            }
            PopulateData(data);
        }

        protected virtual void PopulateData(IEditorData data)
        {
            if (m_data == null)
            {
                Value = string.Empty;
            }
            else
            {
                Value = data.GetAttribute(m_attribute);
            }
        }

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_controller = controller;
            if (controlData != null)
            {
                m_attribute = controlData.Attribute;
                m_attributeName = controlData.Caption;
            }
            else
            {
                m_attribute = null;
                m_attributeName = null;
            }
        }

        public virtual System.Type ExpectedType
        {
            get { return typeof(string); }
        }

        protected string OldValue
        {
            get { return m_oldValue; }
            set { m_oldValue = value; }
        }

        private void ctlWebBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            ctlWebBrowser.Document.Body.KeyUp += Document_KeyUp;
            if (m_valueToSet != null)
            {
                HTML = m_valueToSet;
            }
        }

        private void ctlWebBrowser_GotFocus(object sender, System.EventArgs e)
        {
            if (ctlWebBrowser.Document != null && ctlWebBrowser.Document.Body != null)
            {
                ctlWebBrowser.Document.Body.Focus();
            }
        }

        private void ctlWebBrowser_LostFocus(object sender, System.EventArgs e)
        {
            Save(m_data);
        }

        private void SetUpBrowser()
        {
            ctlWebBrowser.DocumentText = "<html><body></body></html>";
            m_document = (IHTMLDocument2)ctlWebBrowser.Document.DomDocument;
            m_document.designMode = "On";
        }

        private string HTML
        {
            get
            {
                if (ctlWebBrowser.Document != null && ctlWebBrowser.Document.Body != null)
                {
                    return ctlWebBrowser.Document.Body.InnerHtml;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (ctlWebBrowser.Document != null && ctlWebBrowser.Document.Body != null)
                {
                    ctlWebBrowser.Document.Body.InnerHtml = value;
                    m_valueToSet = null;
                }
                else
                {
                    m_valueToSet = value;
                }
            }
        }

        private string ConvertXMLtoHTML(string input)
        {
            return input;
        }

        private string ConvertHTMLtoXML(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            int pos = 0;
            bool finished = false;
            string result = string.Empty;

            input = input.Replace("&nbsp;", "");

            do
            {
                int nextTagStart = input.IndexOf('<', pos);
                if (nextTagStart == -1)
                {
                    finished = true;
                    result += input.Substring(pos);
                }
                else
                {
                    result += input.Substring(pos, nextTagStart - pos);
                    int nextTagEnd = input.IndexOf('>', nextTagStart);
                    string thisTag = input.Substring(nextTagStart + 1, nextTagEnd - nextTagStart - 1).ToLower();
                    bool endTag = false;

                    if (thisTag.Substring(0, 1) == "/")
                    {
                        endTag = true;
                        thisTag = thisTag.Substring(1);
                    }

                    if (thisTag == "p")
                    {
                        if (result.Length > 0 && !endTag)
                        {
                            result += "<br/><br/>";
                        }
                    }
                    else
                    {
                        if (s_htmlToXml.ContainsKey(thisTag))
                        {
                            if (thisTag == "br")
                            {
                                result += "<br/>";
                            }
                            else
                            {
                                result += "<";

                                if (endTag)
                                {
                                    result += "/";
                                }

                                result += s_htmlToXml[thisTag];

                                result += ">";
                            }
                        }
                    }

                    pos = nextTagEnd + 1;
                }
            } while (!(finished));

            return result;
        }

        void Document_KeyUp(object sender, HtmlElementEventArgs e)
        {
            if (IsDirty)
            {
                if (Dirty != null)
                {
                    Dirty(this, new DataModifiedEventArgs(m_oldValue, GetValue()));
                }
            }
        }

        private void Bold()
        {
            ctlWebBrowser.Document.ExecCommand("Bold", false, null);
        }

        private void Italic()
        {
            ctlWebBrowser.Document.ExecCommand("Italic", false, null);
        }

        private void Underline()
        {
            ctlWebBrowser.Document.ExecCommand("Underline", false, null);
        }

        private void butBold_Click(System.Object sender, System.EventArgs e)
        {
            Bold();
        }

        private void butItalic_Click(System.Object sender, System.EventArgs e)
        {
            Italic();
        }

        private void butUnderline_Click(System.Object sender, System.EventArgs e)
        {
            Underline();
        }
    }
}
