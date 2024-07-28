using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace TextAdventures.Utility
{
    public static class Utility
    {
        public static string RemoveFileColonPrefix(string path)
        {
            if (path.StartsWith(@"file:\")) path = path.Substring(6);
            if (path.StartsWith(@"file:")) path = path.Substring(5);
            return path;
        }

        public static bool AreFilesEqual(string file1, string file2)
        {
            // From http://stackoverflow.com/questions/211008/c-sharp-file-management

            // get file length and make sure lengths are identical
            long length = new FileInfo(file1).Length;
            if (length != new FileInfo(file2).Length)
                return false;

            byte[] buf1 = new byte[4096];
            byte[] buf2 = new byte[4096];

            // open both for reading
            using (FileStream stream1 = File.OpenRead(file1))
            using (FileStream stream2 = File.OpenRead(file2))
            {
                // compare content for equality
                int b1, b2;
                while (length > 0)
                {
                    // figure out how much to read
                    int toRead = buf1.Length;
                    if (toRead > length)
                        toRead = (int)length;
                    length -= toRead;

                    // read a chunk from each and compare
                    b1 = stream1.Read(buf1, 0, toRead);
                    b2 = stream2.Read(buf2, 0, toRead);
                    for (int i = 0; i < toRead; ++i)
                        if (buf1[i] != buf2[i])
                            return false;
                }
            }

            return true;
        }

        public static string FileMD5Hash(string filename)
        {
            return CalculateMD5Hash(File.ReadAllBytes(filename));
        }

        public static string CalculateMD5Hash(byte[] input)
        {
            MD5 md5 = MD5.Create();
            var hash = md5.ComputeHash(input);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        
        //public static void LaunchURL(string url)
        //{
        //    try
        //    {
        //        System.Diagnostics.Process.Start(url);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(string.Format("Error launching {0}\n\n{1}", url, ex.Message), "Quest", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
    }
}
