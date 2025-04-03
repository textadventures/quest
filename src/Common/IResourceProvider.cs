namespace QuestViva.Common;

public interface IResourceProvider
{
    string GetUrl(string file);
}

public class DummyResourceProvider : IResourceProvider
{
    public string GetUrl(string file)
    {
        return string.Empty;
    }
}