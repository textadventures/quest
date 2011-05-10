using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;

namespace AxeSoftware.Quest
{
    public static class GameLauncher
    {
        public static IASL GetGame(string filename, string libraryFolder)
        {
            switch (System.IO.Path.GetExtension(filename).ToLower())
            {
                case ".aslx":
                    return new WorldModel(filename, libraryFolder);
                case ".asl":
                case ".cas":
                case ".qsg":
                    return new AxeSoftware.Quest.LegacyASL.LegacyGame(filename);
                default:
                    return null;
            }
        }
    }
}
