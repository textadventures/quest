using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WebInterfaces;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

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

        private string fileRoot;
        private TASessionManager sessionManager = new TASessionManager();

        public TAEditorFileManager()
        {
            fileRoot = ConfigurationManager.AppSettings["TextAdvEditorFileRoot"];
            if (string.IsNullOrEmpty(fileRoot))
            {
                fileRoot = @"D:\Editor Games";
            }
        }

        public string GetFile(int id)
        {
            TAUser taUser = sessionManager.GetTAUser();
            if (taUser == null)
            {
                return null;
            }

            var editorGame = Api.GetData<ApiEditorGame>("api/editorgame?id=" + id + "&userid=" + taUser.UserId);

            if (editorGame == null) return null;
            if (string.IsNullOrEmpty(editorGame.Filename)) return null;

            if (Config.AzureFiles)
            {
                return ConfigurationManager.AppSettings["AzureFilesBase"] + editorGame.Filename.Replace(@"\", "/");
            }

            string filePath = System.IO.Path.Combine(fileRoot, editorGame.Filename);
            return filePath;
        }

        public string GetPlayFilename(int id)
        {
            TAUser taUser = sessionManager.GetTAUser();
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
            if (Config.AzureFiles)
            {
                var url = GetFile(id);
                var container = GetAzureBlobContainer("editorgames");
                var blob = container.GetBlockBlobReference(url);
                blob.Properties.ContentType = "application/octet-stream";
                blob.UploadText(data);
                return;
            }

            string savePath = GetFile(id);
            System.IO.File.WriteAllText(savePath, data);

            UploadOutputToAzure(savePath, data);
        }

        private void UploadOutputToAzure(string filename, string data)
        {
            // filename will be like "D:\Editor Games\guid\file.aslx"
            var container = GetAzureBlobContainer("editorgames");
            var blob = container.GetBlockBlobReference(filename.Substring(16).Replace(@"\", "/"));
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
            TAUser taUser = sessionManager.GetTAUser();
            if (taUser == null) return result;

            string directory = Guid.NewGuid().ToString();
            string filePath = System.IO.Path.Combine(directory, filename);
            string fullPath = null;

            if (!Config.AzureFiles)
            {
                string fullDirectoryPath = System.IO.Path.Combine(fileRoot, directory);
                fullPath = System.IO.Path.Combine(fullDirectoryPath, filename);
                System.IO.Directory.CreateDirectory(fullDirectoryPath);
            }
            else
            {
                fullPath = filePath;
            }

            var newGame = Api.PostData<ApiEditorGame, ApiEditorGame>("api/editorgame", new ApiEditorGame
            {
                UserId = int.Parse(taUser.UserId),
                Filename = filePath,
                GameName = gameName
            });

            result.Id = newGame.GameId;
            result.FullPath = fullPath;

            return result;
        }

        public void FinishCreatingNewFile(string filename, string data)
        {
            if (Config.AzureFiles)
            {
                var container = GetAzureBlobContainer("editorgames");
                var blob = container.GetBlockBlobReference(filename);
                blob.Properties.ContentType = "application/octet-stream";
                blob.UploadText(data);
                return;
            }

            UploadOutputToAzure(filename, data);
        }

        public string UploadPath(int id)
        {
            var filename = GetFile(id);
            if (filename == null) return null;

            if (Config.AzureFiles)
            {
                return filename.Substring(0, filename.LastIndexOf("/"));
            }

            return System.IO.Path.GetDirectoryName(filename);
        }
    }
}
