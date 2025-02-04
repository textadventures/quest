using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TextAdventures.Quest;
using System.Web.Mvc;
using WebEditor.Models.Controls;

namespace WebEditor.Views.Edit
{
    public static class ControlHelpers
    {
        private static Dictionary<string, string> s_controlTypesMap = new Dictionary<string, string> {
            {"boolean", "checkbox"},
            {"string", "textbox"},
            {"script", "script"},
            {"stringlist", "list"},
            {"int", "number"},
            {"double", "numberdouble"},
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
            {typeof(double), "double"},
            {typeof(IEditableObjectReference), "object"},
            {typeof(IEditableCommandPattern), "simplepattern"},
            {typeof(IEditableDictionary<string>),"stringdictionary"},
            {typeof(IEditableDictionary<IEditableScripts>), "scriptdictionary"}
        };

        public static IEnumerable<SelectListItem> GetDropdownValues(IEditorControl ctl, string currentValue, EditorController controller)
        {
            IEnumerable<string> valuesList = ctl.GetListString("validvalues");
            IDictionary<string, string> valuesDictionary = ctl.GetDictionary("validvalues");
            string source = ctl.GetString("source");

            // TO DO: Need a way of allowing free text entry

            if (source == "basefonts")
            {
                valuesList = controller.AvailableBaseFonts();
            }
            else if (source == "webfonts")
            {
                valuesList = controller.AvailableWebFonts();
            }

            if (valuesList != null)
            {
                if (string.IsNullOrEmpty(currentValue))
                {
                    valuesList = new List<string> { "" }.Union(valuesList);
                }

                return valuesList.Select(v => new SelectListItem { Text = v, Value = v, Selected = (v == currentValue) });
            }
            else if (valuesDictionary != null)
            {
                return valuesDictionary.Select(kvp => new SelectListItem { Text = kvp.Value, Value = kvp.Key, Selected = (kvp.Key == currentValue) });
            }
            else
            {
                throw new Exception("Unknown source list for dropdown");
            }
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

        public static IEnumerable<string> GetObjectListNames(IEditorControl ctl, EditorController controller)
        {
            string source = ctl.GetString("source");
            if (source != null)
            {
                return controller.GetElementNames(source);
            }
            else
            {
                string objectType = ctl.GetString("objecttype");
                IEnumerable<string> objectNames = controller.GetObjectNames(objectType ?? "object");
                return new List<string> { "" }.Union(objectNames);
            }
        }

        public static void PopulateRichTextControlModel(IEditorControl ctl, EditorController controller, RichTextControl model)
        {
            if (ctl.GetBool("notextprocessor")) return;

            var commandDataList = controller.GetElementDataAttribute("_RichTextControl_TextProcessorCommands", "data") as IEnumerable;

            model.TextProcessorCommands = (from IDictionary<string, string> commandData in commandDataList
                                           select new RichTextControl.TextProcessorCommand
                                           {
                                               Command = GetDictionaryValue(commandData, "command"),
                                               Info = GetDictionaryValue(commandData, "info"),
                                               InsertBefore = GetDictionaryValue(commandData, "insertbefore"),
                                               InsertAfter = GetDictionaryValue(commandData, "insertafter"),
                                               Source = GetDictionaryValue(commandData, "source"),
                                               Extensions = GetExtensions(commandData)
                                           });

        }

        private static string GetDictionaryValue(IDictionary<string, string> dictionary, string key)
        {
            string value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        private static string GetExtensions(IDictionary<string, string> dictionary)
        {
            var value = GetDictionaryValue(dictionary, "extensions");
            return value == null ? null : string.Join(";", value.Split(';').Select(s => s.Substring(1)));
        }
    }
}