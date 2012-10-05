using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace TextAdventures.Utility
{
    public static class Registry
    {
        public static object GetSetting(string product, string keyName, string valueName, object defaultValue)
        {
            return GetKey(product, keyName).GetValue(valueName, defaultValue);
        }

        public static void SaveSetting(string product, string keyName, string valueName, object value)
        {
            GetKey(product, keyName).SetValue(valueName, value);
        }

        private static RegistryKey GetKey(string product, string keyName)
        {
            return Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\" + product + @"\" + keyName);
        }

        public static object GetHKLMSetting(string product, string keyName, string valueName, object defaultValue)
        {
            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\" + product + @"\" + keyName);
            if (key == null) return defaultValue;
            return key.GetValue(valueName, defaultValue);
        }
    }
}
