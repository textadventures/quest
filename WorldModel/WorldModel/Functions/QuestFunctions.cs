using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Collections;

namespace AxeSoftware.Quest.Functions
{
    public static class QuestFunctions
    {
        public static string Template(string template)
        {
            return WorldModel.Instance.Template.GetText(template);
        }

        public static string DynamicTemplate(string template, Element obj)
        {
            return WorldModel.Instance.Template.GetDynamicText(template, obj);
        }

        public static string DynamicTemplate(string template, string text)
        {
            return WorldModel.Instance.Template.GetDynamicText(template, text);
        }        

        public static bool HasString(Element obj, string property)
        {
            return obj.Fields.HasString(property);
        }

        public static string GetString(Element obj, string property)
        {
            return obj.Fields.GetString(property);
        }

        public static bool HasBoolean(Element obj, string property)
        {
            return obj.Fields.HasType<bool>(property);
        }

        public static bool GetBoolean(Element obj, string property)
        {
            return obj.Fields.GetAsType<bool>(property);
        }

        public static bool HasScript(Element obj, string property)
        {
            return obj.Fields.HasType<IScript>(property);
        }

        public static bool HasObject(Element obj, string property)
        {
            return obj.Fields.HasType<Element>(property);
        }

        public static bool HasDelegateImplementation(Element obj, string property)
        {
            return obj.Fields.HasType<DelegateImplementation>(property);
        }

        public static string GetExitByLink(Element from, Element to)
        {
            foreach (Element e in WorldModel.Instance.Objects)
            {
                if (e.Parent == from && e.Fields[FieldDefinitions.To] == to) return e.Name;
            }

            return null;
        }

        public static string GetExitByName(Element parent, string name)
        {
            foreach (Element e in WorldModel.Instance.Objects)
            {
                if (e.Parent == parent && e.Fields[FieldDefinitions.Alias] == name) return e.Name;
            }

            return null;
        }

        public static bool Contains(Element parent, Element name)
        {
            return WorldModel.Instance.ObjectContains(parent, name);
        }

        public static QuestList<Element> NewObjectList()
        {
            return new QuestList<Element>();
        }

        public static QuestList<string> NewStringList()
        {
            return new QuestList<string>();
        }

        public static QuestDictionary<string> NewStringDictionary()
        {
            return new QuestDictionary<string>();
        }

        public static QuestDictionary<Element> NewObjectDictionary()
        {
            return new QuestDictionary<Element>();
        }

        public static bool ListContains(IQuestList list, object item)
        {
            return list.Contains(item);
        }

        public static QuestList<Element> AllObjects()
        {
            QuestList<Element> result = new QuestList<Element>();

            foreach (Element item in WorldModel.Instance.GetAllObjects().Where(o => o.Type == ObjectType.Object))
            {
                result.Add(item);
            }
            return result;
        }

        public static QuestList<Element> AllExits()
        {
            QuestList<Element> result = new QuestList<Element>();

            foreach (Element item in WorldModel.Instance.GetAllObjects().Where(o => o.Type == ObjectType.Exit))
            {
                result.Add(item);
            }
            return result;
        }

        public static QuestList<Element> AllCommands()
        {
            QuestList<Element> result = new QuestList<Element>();
            foreach (Element item in WorldModel.Instance.GetAllObjects())
            {
                if (item.Type == ObjectType.Command)
                    result.Add(item);
            }
            return result;
        }

        public static int ListCount(ICollection list)
        {
            return list.Count;
        }

        public static object ListItem(IQuestList list, int index)
        {
            return list[index];
        }

        public static string StringListItem(IQuestList list, int index)
        {
            return list[index] as string;
        }

        public static Element ObjectListItem(IQuestList list, int index)
        {
            return list[index] as Element;
        }

        public static Element GetObject(string name)
        {
            return WorldModel.Instance.Object(name);
        }

        public static string TypeOf(Element obj, string attribute)
        {
            object value = obj.Fields.Get(attribute);

            if (value == null) return "null";
            if (value is DelegateImplementation) return ((DelegateImplementation)value).TypeName;

            return WorldModel.ConvertTypeToTypeName(value.GetType());
        }

        public static object RunDelegateFunction(Element obj, string del, params object[] parameters)
        {
            DelegateImplementation impl = obj.Fields.Get(del) as DelegateImplementation;

            if (impl == null)
            {
                throw new Exception(string.Format("Object '{0}' has no delegate implementation '{1}'", obj.Name, del));
            }

            Parameters paramValues = new Parameters();

            int cnt = 0;
            foreach (object p in parameters)
            {
                paramValues.Add((string)impl.Definition.Fields[FieldDefinitions.ParamNames][cnt], p);
                cnt++;
            }

            return WorldModel.Instance.RunDelegateScript(impl.Implementation.Fields[FieldDefinitions.Script], paramValues, obj);
        }

        //public static string SafeXML(string input)
        //{
        //    return Utility.SafeXML(input);
        //}

        public static bool IsRegexMatch(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);
            return regex.IsMatch(input);
        }

        public static int GetMatchStrength(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            // The idea is that you have a regex like
            //          look at (?<object>.*)
            // And you have a string like
            //          look at thing
            // The strength is the length of the "fixed" bit of the string, in this case "look at ".
            // So we calculate this as the length of the input string, minus the length of the
            // text that matches the named groups.

            int lengthOfTextMatchedByGroups = 0;

            foreach (string groupName in regex.GetGroupNames())
            {
                // exclude group names like "0", we only want the explicitly named groups
                if (!AxeSoftware.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    lengthOfTextMatchedByGroups += groupMatch.Length;
                }
            }

            return input.Length - lengthOfTextMatchedByGroups;
        }

        public static QuestDictionary<string> Populate(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);
            if (!regex.IsMatch(input)) throw new Exception(string.Format("String '{0}' is not a match for Regex '{1}'", input, regexPattern));

            QuestDictionary<string> result = new QuestDictionary<string>();

            foreach (string groupName in regex.GetGroupNames())
            {
                if (!AxeSoftware.Utility.Strings.IsNumeric(groupName))
                {
                    string groupMatch = regex.Match(input).Groups[groupName].Value;
                    result.Add(groupName, groupMatch);
                }
            }

            return result;
        }

        public static object DictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key];
        }

        public static string StringDictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key] as string;
        }

        public static Element ObjectDictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key] as Element;
        }

        public static string ShowMenu(string caption, QuestDictionary<string> options, bool allowCancel)
        {
            return WorldModel.Instance.DisplayMenu(caption, options, allowCancel);
        }

        public static string ShowMenu(string caption, QuestList<string> options, bool allowCancel)
        {
            return WorldModel.Instance.DisplayMenu(caption, options, allowCancel);
        }

        public static bool DictionaryContains(IDictionary dictionary, string key)
        {
            return dictionary.Contains(key);
        }

        public static int DictionaryCount(IDictionary dictionary)
        {
            return dictionary.Count;
        }
    }
}
