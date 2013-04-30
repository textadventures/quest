namespace WebInterfaces
{
    public interface IFileManager
    {
        string GetFileForID(string id);
        void NotifySave(IUser user, string gameId, string filename);
        string GetSaveFileForID(IUser user, string id, out string gameId);
    }
}