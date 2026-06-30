namespace QuestViva.Utility;

public static class Files
{
    public static string RemoveFileColonPrefix(string path)
    {
        if (path.StartsWith(@"file:\"))
        {
            path = path.Substring(6);
        }

        if (path.StartsWith(@"file:"))
        {
            path = path.Substring(5);
        }

        return path;
    }
}