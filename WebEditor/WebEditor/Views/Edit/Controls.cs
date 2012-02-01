using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxeSoftware.Quest;
using System.Web.Mvc;

namespace WebEditor.Views.Edit
{
    public static class Controls
    {
        private static Dictionary<string, string> s_controlTypesMap = new Dictionary<string, string> {
            {"boolean", "checkbox"},
            {"string", "textbox"},
            {"script", "script"},
            {"stringlist", "list"},
            {"int", "number"},
            {"object", "objects"},
            {"simplepattern", "pattern"},
            {"stringdictionary", "stringdictionary"},
            {"scriptdictionary", "scriptdictionary"},
            {"null", null}
        };

        private static Dictionary<Type, string> s_typeNamesMap = new Dictionary<Type, string> {
            {typeof(bool), "boolean"},
            {typeof(string), "string"},
            {typeof(IEditableScripts), "script"},
            {typeof(IEditableList<string>), "stringlist"},
            {typeof(int), "int"},
            {typeof(IEditableObjectReference), "object"},
            {typeof(IEditableCommandPattern), "simplepattern"},
            {typeof(IEditableDictionary<string>),"stringdictionary"},
            {typeof(IEditableDictionary<IEditableScripts>), "scriptdictionary"}
        };

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

        public static string GetTypeName(object value)
        {
            if (value == null) return "null";
            Type type = value.GetType();
            return s_typeNamesMap.FirstOrDefault(t => t.Key.IsAssignableFrom(type)).Value;
        }

        public static string GetEditorNameForType(string typeName, IDictionary<string, string> editorsOverride)
        {
            if (string.IsNullOrEmpty(typeName)) return string.Empty;
            if (editorsOverride != null)
            {
                if (editorsOverride.ContainsKey(typeName))
                {
                    return editorsOverride[typeName];
                }
            }
            return s_controlTypesMap[typeName];
        }

        public static IEnumerable<SelectListItem> GetDropDownTypesControlItems(IEditorControl ctl, EditorController controller, string element)
        {
            IDictionary<string, string> types = ctl.GetDictionary("types");

            // TO DO: If more than one type is inherited by the object, disable the control
            string selectedItem = controller.GetSelectedDropDownType(ctl, element);

            return types.Select(t => new SelectListItem { Value = t.Key, Text = t.Value, Selected = (selectedItem == t.Key) });
        }
    }
}