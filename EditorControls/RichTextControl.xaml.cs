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
            public string Source { get; set; }
            public string Extensions { get; set; }
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
                                    Command = GetDictionaryItem(commandData, "command"),
                                    Info = GetDictionaryItem(commandData, "info"),
                                    InsertBefore = GetDictionaryItem(commandData, "insertbefore"),
                                    InsertAfter = GetDictionaryItem(commandData, "insertafter"),
                                    Source = GetDictionaryItem(commandData, "source"),
                                    Extensions = GetDictionaryItem(commandData, "extensions")
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

        private string GetDictionaryItem(IDictionary<string, string> dictionary, string key)
        {
            string value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            var command = (TextProcessorCommand)((Button)sender).Tag;
            switch (command.Source)
            {
                case "objects":
                    InsertObject(command);
                    break;
                case "images":
                    InsertPicture(command);
                    break;
                case "pages":
                    InsertPage(command);
                    break;
                default:
                    InsertText(command.InsertBefore, command.InsertAfter);
                    break;
            }
            textBox.Focus();
        }

        private void InsertObject(TextProcessorCommand command)
        {
            var objects = m_helper.Controller.GetObjectNames("object", true).OrderBy(n => n);
            var result = PopupEditors.EditStringWithDropdown(
                "Please choose an object",
                string.Empty, null, null, string.Empty, objects);

            if (!result.Cancelled)
            {
                InsertText(command.InsertBefore + result.Result + command.InsertAfter, string.Empty);
            }
        }

        private void InsertPicture(TextProcessorCommand command)
        {
            var window = new FilePopUp();
            window.Initialise(m_helper.Controller);
            window.ShowDialog();
            if (!string.IsNullOrEmpty(window.Filename))
            {
                InsertText(command.InsertBefore + window.Filename + command.InsertAfter, string.Empty);
            }
        }

        private void InsertPage(TextProcessorCommand command)
        {
            var pages = m_helper.Controller.GetObjectNames("object", true).Where(n => n != "player").OrderBy(n => n);
            var result = PopupEditors.EditStringWithDropdown("Link text", string.Empty, "Add link to", pages, pages.First(), allowEmptyString: true);

            if (!result.Cancelled)
            {
                InsertText(
                    command.InsertBefore + result.ListResult + (result.Result.Length > 0 ? ":" + result.Result : "") +
                    command.InsertAfter, string.Empty);
            }
        }

        private void InsertText(string before, string after)
        {
            var existingText = textBox.SelectedText;
            textBox.SelectedText = before + existingText + after;
            textBox.SelectionStart = textBox.SelectionStart + before.Length;
            textBox.SelectionLength = existingText.Length;
        }

        public IControlDataHelper Helper { get { return m_helper; } }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            var value = m_helper.Populate(data);
            if (value != null) value = value.Replace("<br/>", Environment.NewLine).Replace("<br />", Environment.NewLine);
            textBox.Text = value;
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
            Utility.Utility.LaunchURL("http://docs.textadventures.co.uk/quest/text_processor.html");
        }
    }
}
