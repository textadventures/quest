namespace QuestViva.Legacy;

public static class Resources
{
    public const string QuestDAT = "quest.dat";
    public const string stdverbs = "stdverbs.lib";
    public const string standard = "standard.lib";
    public const string q3ext = "q3ext.qlb";
    public const string Typelib = "Typelib.qlb";
    public const string net = "net.lib";
    
    private static Stream GetResource(string name)
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"QuestViva.Legacy.Libraries.{name}");
    }
    
    public static byte[] GetResourceBytes(string name)
    {
        using var stream = GetResource(name);
        if (stream == null) return null;

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}