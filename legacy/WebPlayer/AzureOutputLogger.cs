using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;

namespace WebPlayer
{
    public class AzureOutputLogger
    {
        private readonly StringBuilder m_output = new StringBuilder();

        public string BlobId { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }

        public void AddText(string text)
        {
            m_output.Append(ConvertHtmlToText(text));
        }

        private static readonly Regex s_regexNewLine = new Regex(@"\<br\s*/\>");
        private static readonly Regex s_regexHtml = new Regex(@"\<.+?\>");

        private static string ConvertHtmlToText(string text)
        {
            text = s_regexNewLine.Replace(text, Environment.NewLine);
            text = s_regexHtml.Replace(text, "");
            text = text.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
            return text;
        }

        public void WriteLog()
        {
            if (m_output.Length == 0) return;
            if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(SessionId) || string.IsNullOrEmpty(BlobId)) return;

            var connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
            var account = CloudStorageAccount.Parse(connectionString);

            var blobClient = account.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["AzureLogSessionsBlob"]);
            blobContainer.CreateIfNotExists();

            var blob = blobContainer.GetBlockBlobReference(BlobId);
            blob.UploadText(m_output.ToString());

            Api.PostData("games/endsession", new
            {
                UserId,
                SessionId,
            });
        }
    }
}