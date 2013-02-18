using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("richtext")]
    public partial class RichTextControl : UserControl, IElementEditorControl
    {
        private bool m_nullable = false;
        private ControlDataHelper<string> m_helper;
        private bool m_initialised;

        private class TextProcessorCommand
        {
            public string Command { get; set; }
            public string InsertBefore { get; set; }
            public string InsertAfter { get; set; }
            public string Info { get; set; }
        }

        private static List<TextProcessorCommand> s_commands = new List<TextProcessorCommand>
        {
            new TextProcessorCommand { Command = "Bold", InsertBefore = "<b>", InsertAfter = "</b>", Info = "<b>"},
            new TextProcessorCommand { Command = "Italic", InsertBefore = "<i>", InsertAfter = "</i>", Info = "<i>"},
            new TextProcessorCommand { Command = "Underline", InsertBefore = "<u>", InsertAfter = "</u>", Info = "<u>"},
            new TextProcessorCommand { Command = "Once", InsertBefore = "{once:", InsertAfter = "}", Info = "{once}"},
            new TextProcessorCommand { Command = "Object link", InsertBefore = "{object:", InsertAfter = "}", Info = "{object}"},
            new TextProcessorCommand { Command = "Command link", InsertBefore = "{command:", InsertAfter = "}", Info = "{command}"},
            new TextProcessorCommand { Command = "If...", InsertBefore = "{if ", InsertAfter = "object.attribute=value:}", Info = "{if}"},
            new TextProcessorCommand { Command = "Random text", InsertBefore = "{random:", InsertAfter = "}", Info = "{random}"},
            new TextProcessorCommand { Command = "Image", InsertBefore = "{img:", InsertAfter = "}", Info = "{img}"},
        };

        public RichTextControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += Initialise;
        }

        void Initialise()
        {
            if (m_initialised) return;
            m_initialised = true;
            m_nullable = m_helper.ControlDefinition.GetBool("nullable");
            if (m_helper.ControlDefinition.GetBool("notextprocessor"))
            {
                textProcessorOptions.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (var command in s_commands)
                {
                    stackCommandButtons.Children.Add(new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new Button
                            {
                                Content = command.Command,
                                Padding = new Thickness(5),
                                Margin = new Thickness(3),
                                Tag = command
                            },
                            new TextBlock
                            {
                                Text = command.Info,
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(5, 0, 0, 0)
                            }
                        }
                    });
                }

                foreach (StackPanel stackPanel in stackCommandButtons.Children)
                {
                    var button = (Button)stackPanel.Children[0];
                    button.Click += button_Click;
                }
            }
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            var command = (TextProcessorCommand)((Button)sender).Tag;
            textBox.SelectedText = command.InsertBefore + command.InsertAfter;
            textBox.SelectionStart = textBox.SelectionStart + command.InsertBefore.Length;
            textBox.SelectionLength = 0;
            textBox.Focus();
        }

        public IControlDataHelper Helper { get { return m_helper; } }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            textBox.Text = m_helper.Populate(data).Replace("<br/>", Environment.NewLine).Replace("<br />",Environment.NewLine);
            textBox.IsEnabled = m_helper.CanEdit(data);
            textBox.IsReadOnly = data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            string saveValue = textBox.Text.Replace(Environment.NewLine, "<br/>");
            if (saveValue.Length == 0 && m_nullable) saveValue = null;
            m_helper.Save(saveValue);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.SetDirty(textBox.Text);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public Control FocusableControl
        {
            get { return textBox; }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Utility.Utility.LaunchURL("http://quest5.net/wiki/Text_processor");
        }
    }
}
