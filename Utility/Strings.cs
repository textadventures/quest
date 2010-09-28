using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Utility
{
    public static class Strings
    {
        public static bool IsNumeric(string expression)
        {
            return Microsoft.VisualBasic.Information.IsNumeric(expression);
        }
    }
}
