using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WebInterfaces;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace TASessionManager
{
    class TAEditorFileManager : IEditorFileManager
    {
        public class ApiEditorGame
        {
            public string Filename { get; set; }
            public int GameId { get; set; }
            public int UserId { get; set; }
            public string GameName { get; set; }
        }

        private TASessionManager sessionManager = new TASessionManager();

        public string GetFile(int id)
        {
            User taUser = sessionManager.GetTAUser();
            if (taUser == null)
            {
                return null;
            }

            var editorGame = Api.GetData<ApiEditorGame>("api/editorgame?id=" + id + "&userid=" + taUser.UserId);

            if (editorGame == null) return null;
            if (string.IsNullOrEmpty(editorGame.Filename)) return null;

            return ConfigurationManager.AppSettings["AzureFilesBase"] + editorGame.Filename.Replace(@"\", "/");
        }

        public string GetPlayFilename(int id)
        {
            User taUser = sessionManager.GetTAUser();
            if (taUser == null)
            {
                return null;
            }

            var editorGame = Api.GetData<ApiEditorGame>("api/editorgame?id=" + id + "&userid=" + taUser.UserId);

            if (editorGame == null) return null;
            if (string.IsNullOrEmpty(editorGame.Filename)) return null;

            return editorGame.Filename.Replace("\\", "/");
        }

        public void SaveFile(int id, string data)
        {
            var url = GetFile(id);

            var pathRoot = ConfigurationManager.AppSettings["AzureFilesBase"];
            if (!url.StartsWith(pathRoot))
            {
                throw new Exception("Expected filename to start with " + pathRoot);
            }

            var filename = url.Substring(pathRoot.Length);

            var container = GetAzureBlobContainer("editorgames");
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "application/octet-stream";
            blob.UploadText(data);
        }

        private static CloudBlobContainer GetAzureBlobContainer(string containerName)
        {
            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);

            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            return container;
        }

        public CreateNewFileData CreateNewFile(string filename, string gameName)
        {
            CreateNewFileData result = new CreateNewFileData();
            User taUser = sessionManager.GetTAUser();
            if (taUser == null) return result;

            string directory = Guid.NewGuid().ToString();
            string filePath = System.IO.Path.Combine(directory, filename);

            var newGame = Api.PostData<ApiEditorGame, ApiEditorGame>("api/editorgame", new ApiEditorGame
            {
                UserId = int.Parse(taUser.UserId),
                Filename = filePath,
                GameName = gameName
            });

            result.Id = newGame.GameId;
            result.FullPath = filePath;

            return result;
        }

        public void FinishCreatingNewFile(string filename, string data)
        {
            var container = GetAzureBlobContainer("editorgames");
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "application/octet-stream";
            blob.UploadText(data);
        }

        public string UploadPath(int id)
        {
            var filename = GetFile(id);
            if (filename == null) return null;

            var pathRoot = ConfigurationManager.AppSettings["AzureFilesBase"];
            if (!filename.StartsWith(pathRoot))
            {
                throw new Exception("Expected filename to start with " + pathRoot);
            }

            filename = filename.Substring(pathRoot.Length);

            return filename.Substring(0, filename.LastIndexOf("/"));
        }
    }
}
