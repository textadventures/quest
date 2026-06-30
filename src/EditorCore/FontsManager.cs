using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuestViva.EditorCore;

[JsonSerializable(typeof(FontsManager.WebFontsResult))]
internal partial class FontsManagerJsonContext : JsonSerializerContext { }

internal class FontsManager
{
    private static readonly HttpClient s_client = new();

    private readonly List<string> _basefonts = new()
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
        "'Lucida Console', Monaco, monospace"
    };

    private List<string> _webFonts = new() { string.Empty };

    public FontsManager()
    {
        _ = FetchWebFontsAsync();
    }

    private async Task FetchWebFontsAsync()
    {
        try
        {
            var json = await s_client.GetStringAsync(
                "https://www.googleapis.com/webfonts/v1/webfonts?key=AIzaSyDs93IH2UgudQK5IyNSdvKnm1N8TIYzlcM");
            var result = JsonSerializer.Deserialize(json, FontsManagerJsonContext.Default.WebFontsResult);
            if (result?.items != null)
            {
                _webFonts = new List<string>(result.items.Select(i => i.family));
                _webFonts.Insert(0, string.Empty);
            }
        }
        catch { }
    }

    public List<string> GetBaseFonts() => _basefonts;

    public List<string> GetWebFonts() => _webFonts;

    internal class WebFontResult
    {
        public string kind { get; set; } = string.Empty;
        public string family { get; set; } = string.Empty;
        public List<string> variants { get; set; } = new();
        public List<string> subsets { get; set; } = new();
    }

    internal class WebFontsResult
    {
        public string kind { get; set; } = string.Empty;
        public List<WebFontResult> items { get; set; } = new();
    }
}
