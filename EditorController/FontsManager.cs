using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class FontsManager
    {
        private System.Net.WebClient m_client;

        private List<string> m_basefonts = new List<string>
        {
            "Georgia, serif",
            "'Palatino Linotype', 'Book Antiqua', Palatino, serif",
            "'Times New Roman', Times, serif",
            "Arial, Helvetica, sans-serif",
            "'Arial Black', Gadget, sans-serif",
            "'Comic Sans MS', cursive, sans-serif",
            "Impact, Charcoal, sans-serif",
            "'Lucida Sans Unicode', 'Lucida Grande', sans-serif",
            "Tahoma, Geneva, sans-serif",
            "'Trebuchet MS', Helvetica, sans-serif",
            "Verdana, Geneva, sans-serif",
            "'Courier New', Courier, monospace",
            "'Lucida Console', Monaco, monospace",
        };

        private List<string> m_webFonts = new List<string>
        {
            string.Empty
        };

        public FontsManager()
        {
            m_client = new System.Net.WebClient();
            m_client.DownloadStringCompleted += m_client_DownloadStringCompleted;
            m_client.Proxy = null;
            m_client.DownloadStringAsync(new Uri("https://www.googleapis.com/webfonts/v1/webfonts?key=AIzaSyDs93IH2UgudQK5IyNSdvKnm1N8TIYzlcM"));
        }

        private class WebFontResult
        {
            public string kind { get; set; }
            public string family { get; set; }
            public List<string> variants { get; set; }
            public List<string> subsets { get; set; }
        }

        private class WebFontsResult
        {
            public string kind { get; set; }
            public List<WebFontResult> items { get; set; }
        }

        void m_client_DownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<WebFontsResult>(e.Result);
                m_webFonts = new List<string>(result.items.Select(i => i.family));
                m_webFonts.Insert(0, string.Empty);
            }
        }

        public List<string> GetBaseFonts()
        {
            return m_basefonts;
        }

        public List<string> GetWebFonts()
        {
            return m_webFonts;
        }
    }
}
