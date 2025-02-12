namespace WebInterfaces
{
    public struct CreateNewFileData
    {
        public string FullPath;
        public int Id;
    }

    public interface IEditorFileManager
    {
        string GetFile(int id);
        string GetPlayFilename(int id);
        void SaveFile(int id, string data);
        CreateNewFileData CreateNewFile(string filename, string gameName);
        void FinishCreatingNewFile(string filename, string data);
        string UploadPath(int id);
    }
}