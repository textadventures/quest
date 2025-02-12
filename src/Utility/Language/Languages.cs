//using System;
//using System.Globalization;
//using System.Threading;
//using System.Collections.Generic;
//using System.Xml;

///* Languages (SoonGames)

//    The templates can be used as follows:

//    In VB source code, insert "Imports TextAdventures.Utility.Language.L".
//    Replace the corresponding texts with T ("Templatename") and add them to the templates.resx.

//    In C source code, insert "using TextAdventures.Utility.Language".
//    Replace the corresponding texts with L.T("Templatename") and add them to the templates.resx.

//*/
//namespace TextAdventures.Utility.Language
//{
//    public class L
//    {
//        public static string CurrentLanguage;

//        // Additional languages can be added here. Create a corresponding template.**.resx for this purpose. **The extension is the TwoLetterISO-language-code.
//        // It is not necessary to replace all templates. Templates that have not been translated are displayed in English.
//        private static Dictionary<string, string> Languages = new Dictionary<string, string>
//        {
//            { "English", "en" },
//            { "Deutsch", "de" }
//        };
               
//        private const string DefaultLanguage = "English";

//        public static void LoadLanguage()
//        {
//            CurrentLanguage = Registry.GetSetting("Quest", "Settings", "Language", "").ToString();

//            if (!Languages.ContainsKey(CurrentLanguage))
//            {
//                string currentculture = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
//                if (Languages.ContainsValue(currentculture)) {
//                    CurrentLanguage = FindKey(currentculture, Languages);
//                }
//                else
//                {
//                    CurrentLanguage = DefaultLanguage;
//                }
//            }

//            try
//            {
//                System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo(Languages[CurrentLanguage]);
//                Thread.CurrentThread.CurrentUICulture = culture;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message, "Quest", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }
//        private static string FindKey(string value, Dictionary<string, string> dic)
//        {
//            foreach (KeyValuePair<string, string> kvp in dic)
//            {
//                if (kvp.Value == value) return kvp.Key;
//            }
//            return "";
//        }

//        public static void SaveLanguage(string lang)
//        {
//            TextAdventures.Utility.Registry.SaveSetting("Quest", "Settings", "Language", lang);
//        }

//        public static string T(string key)
//        {
//            return Templates.ResourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);
//        }
//    }
//}