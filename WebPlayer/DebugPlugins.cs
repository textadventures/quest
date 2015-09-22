using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Threading.Tasks;

namespace WebPlayer
{
    internal class DebugFileManager : IFileManager
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<SourceFileData> GetFileForID(string id)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new SourceFileData
            {
                Filename = ConfigurationManager.AppSettings["DebugFileManagerFile"]
            };
        }
    }
}