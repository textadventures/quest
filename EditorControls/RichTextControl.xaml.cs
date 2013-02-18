using System;
using System.Collections;
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
                var commandDataList = m_helper.Controller.GetElementDataAttribute("_RichTextControl_TextProcessorCommands", "data") as IEnumerable;

                var commands = (from IDictionary<string, string> commandData in commandDataList
                                select new TextProcessorCommand
                                {
                                    Command = commandData["command"],
                                    Info = commandData["info"],
                                    InsertBefore = commandData["insertbefore"],
                                    InsertAfter = commandData["insertafter"]
                                });

                foreach (var command in commands)
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
