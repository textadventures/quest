using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Utility
{
    public static class Utility
    {
        public static string RemoveFileColonPrefix(string path)
        {
            if (path.StartsWith(@"file:\")) path = path.Substring(6);
            if (path.StartsWith(@"file:")) path = path.Substring(5);
            return path;
        }
    }
}
