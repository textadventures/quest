using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace JawsApi
{
    public static class JawsApi
    {
        [DllImport("jfwapi.dll")]
        public static extern bool JFWSayString(string lpszStrinToSpeak, bool bInterrupt);
    }
}
