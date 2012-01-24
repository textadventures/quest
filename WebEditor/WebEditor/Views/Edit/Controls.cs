using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;

namespace WebEditor.Views.Edit
{
    public static class Controls
    {
        public static IEnumerable<string> GetDropdownValues(IEditorControl ctl)
        {
            IEnumerable<string> valuesList = ctl.GetListString("validvalues");
            IDictionary<string, string> valuesDictionary = ctl.GetDictionary("validvalues");
            bool fontsList = ctl.GetBool("fontslist");

            // TO DO: Need a way of allowing free text entry

            if (valuesDictionary != null)
            {
                valuesList = valuesDictionary.Values.ToArray();
            }
            else if (fontsList)
            {
                // TO DO: Fonts list should be a standard list of web-safe fonts, not the list of fonts on the server
                List<string> fonts = new List<string>();
                foreach (var family in System.Drawing.FontFamily.Families)
                {
                    fonts.Add(family.Name);
                }
                valuesList = fonts;
            }

            if (valuesList == null)
            {
                throw new Exception("Invalid type for validvalues");
            }

            return valuesList;
        }
    }
}