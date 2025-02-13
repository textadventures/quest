﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using QuestViva.Common;

namespace QuestViva.Engine.GameLoader
{
    internal class PackageReader
    {
        public class ReadResult(ZipArchive zip)
        {
            public Stream GameFile;
            
            public Stream GetFile(string filename)
            {
                var entry = zip.GetEntry(filename);
                return entry?.Open();
            }
            
            public IEnumerable<string> GetFileNames()
            {
                return zip.Entries.Select(entry => entry.FullName);
            }
        }

        public Task<ReadResult> LoadPackage(IGameData gameData)
        {
            var packageStream = gameData.Data;
            var zip = new ZipArchive(packageStream, ZipArchiveMode.Read);
            
            var gameFile = zip.GetEntry("game.aslx");
            
            if (gameFile == null)
            {
                throw new InvalidDataException("Invalid game file");
            }
            
            return Task.FromResult(new ReadResult(zip)
            {
                GameFile = gameFile.Open()
            });
        }
    }
}
