using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;

namespace TextAdventures.Utility.Language
{
    public class L
    {
        public static string CurrentLanguage;
        private static Dictionary<string, string> Languages = new Dictionary<string, string>
        {
            { "English", "en" },
            { "Deutsch", "de" }
        };

        private static Dictionary<string, string> Templates = new Dictionary<string, string>();
        
        private const string DefaultLanguage = "English";
        private static Dictionary<string, string> TemplatesDefault = new Dictionary<string, string>();

        public static void LoadLanguage()
        {
            CurrentLanguage = Registry.GetSetting("Quest", "Settings", "Language", "").ToString();

            if (!Languages.ContainsKey(CurrentLanguage))
            {
                string currentculture = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                if (Languages.ContainsValue(currentculture)) {
                    CurrentLanguage = FindKey(currentculture, Languages);
                }
                else
                {
                    CurrentLanguage = DefaultLanguage;
                }
            }

            try
            {
                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(Languages[CurrentLanguage]);
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Quest", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ReadTemplates(Templates, CurrentLanguage);
            if (CurrentLanguage != DefaultLanguage) ReadTemplates(TemplatesDefault, DefaultLanguage);
        }
        private static string FindKey(string value, Dictionary<string, string> dic)
        {
            foreach (KeyValuePair<string, string> kvp in dic)
            {
                if (kvp.Value == value) return kvp.Key;
            }
            return "";
        }

        public static void SaveLanguage(string lang)
        {
            TextAdventures.Utility.Registry.SaveSetting("Quest", "Settings", "Language", lang);
        }

        private static void ReadTemplates(Dictionary<string, string> temp, string lang)
        {
            const string nodeTemplate = "template";
            const string nodeName = "name";
            const string endOfPart = "END OF PART";
            string languageFile = @".\Core\Languages\Editor" + lang + ".aslx";
            string key = null;
            string value = null;

            try
            {
                System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(languageFile);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case System.Xml.XmlNodeType.Comment:
                            if (reader.Value.Trim() == endOfPart)
                            {
                                return;
                            }
                            break;
                        case System.Xml.XmlNodeType.Element:
                            if (reader.Name == nodeTemplate && reader.GetAttribute(nodeName) != null)
                            {
                                key = reader.GetAttribute(nodeName);
                            }
                            break;
                        case System.Xml.XmlNodeType.Text:
                            value = reader.Value;
                            break;
                    }
                    if (key != null && value != null)
                    {
                        temp[key] = value;
                        key = null; value = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Quest", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string T(string key)
        {
            if (Templates.ContainsKey(key)) return Templates[key];
            else return TemplatesDefault[key];
        }
    }
}