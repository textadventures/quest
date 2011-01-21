using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using System.Collections;

namespace AxeSoftware.Quest.Functions
{
    internal class ExpressionOwner
    {
        private WorldModel m_worldModel;

        public ExpressionOwner(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public string Template(string template)
        {
            return m_worldModel.Template.GetText(template);
        }

        public string DynamicTemplate(string template, Element obj)
        {
            return m_worldModel.Template.GetDynamicText(template, obj);
        }

        public string DynamicTemplate(string template, string text)
        {
            return m_worldModel.Template.GetDynamicText(template, text);
        }

        public bool HasString(Element obj, string property)
        {
            return obj.Fields.HasString(property);
        }

        public string GetString(Element obj, string property)
        {
            return obj.Fields.GetString(property);
        }

        public bool HasBoolean(Element obj, string property)
        {
            return obj.Fields.HasType<bool>(property);
        }

        public bool GetBoolean(Element obj, string property)
        {
            return obj.Fields.GetAsType<bool>(property);
        }

        public bool HasScript(Element obj, string property)
        {
            return obj.Fields.HasType<IScript>(property);
        }

        public bool HasObject(Element obj, string property)
        {
            return obj.Fields.HasType<Element>(property);
        }

        public bool HasDelegateImplementation(Element obj, string property)
        {
            return obj.Fields.HasType<DelegateImplementation>(property);
        }

        public string GetExitByLink(Element from, Element to)
        {
            foreach (Element e in m_worldModel.Objects)
            {
                if (e.Parent == from && e.Fields[FieldDefinitions.To] == to) return e.Name;
            }

            return null;
        }

        public string GetExitByName(Element parent, string name)
        {
            foreach (Element e in m_worldModel.Objects)
            {
                if (e.Parent == parent && e.Fields[FieldDefinitions.Alias] == name) return e.Name;
            }

            return null;
        }

        public bool Contains(Element parent, Element name)
        {
            return m_worldModel.ObjectContains(parent, name);
        }

        public QuestList<Element> NewObjectList()
        {
            return new QuestList<Element>();
        }

        public QuestList<string> NewStringList()
        {
            return new QuestList<string>();
        }

        public QuestDictionary<string> NewStringDictionary()
        {
            return new QuestDictionary<string>();
        }

        public QuestDictionary<Element> NewObjectDictionary()
        {
            return new QuestDictionary<Element>();
        }

        public QuestDictionary<object> NewDictionary()
        {
            return new QuestDictionary<object>();
        }

        public bool ListContains(IQuestList list, object item)
        {
            return list.Contains(item);
        }

        public QuestList<Element> AllObjects()
        {
            QuestList<Element> result = new QuestList<Element>();

            foreach (Element item in m_worldModel.GetAllObjects().Where(o => o.Type == ObjectType.Object))
            {
                result.Add(item);
            }
            return result;
        }

        public QuestList<Element> AllExits()
        {
            QuestList<Element> result = new QuestList<Element>();

            foreach (Element item in m_worldModel.GetAllObjects().Where(o => o.Type == ObjectType.Exit))
            {
                result.Add(item);
            }
            return result;
        }

        public QuestList<Element> AllCommands()
        {
            QuestList<Element> result = new QuestList<Element>();
            foreach (Element item in m_worldModel.GetAllObjects())
            {
                if (item.Type == ObjectType.Command)
                    result.Add(item);
            }
            return result;
        }

        public int ListCount(ICollection list)
        {
            return list.Count;
        }

        public object ListItem(IQuestList list, int index)
        {
            return list[index];
        }

        public string StringListItem(IQuestList list, int index)
        {
            return list[index] as string;
        }

        public Element ObjectListItem(IQuestList list, int index)
        {
            return list[index] as Element;
        }

        public Element GetObject(string name)
        {
            return m_worldModel.Object(name);
        }

        public string TypeOf(Element obj, string attribute)
        {
            object value = obj.Fields.Get(attribute);

            if (value == null) return "null";
            if (value is DelegateImplementation) return ((DelegateImplementation)value).TypeName;

            return WorldModel.ConvertTypeToTypeName(value.GetType());
        }

        public object RunDelegateFunction(Element obj, string del, params object[] parameters)
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

            return m_worldModel.RunDelegateScript(impl.Implementation.Fields[FieldDefinitions.Script], paramValues, obj);
        }

        //public string SafeXML(string input)
        //{
        //    return Utility.SafeXML(input);
        //}

        public bool IsRegexMatch(string regexPattern, string input)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexPattern);
            return regex.IsMatch(input);
        }

        public int GetMatchStrength(string regexPattern, string input)
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

        public QuestDictionary<string> Populate(string regexPattern, string input)
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

        public object DictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key];
        }

        public string StringDictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key] as string;
        }

        public Element ObjectDictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key] as Element;
        }

        public string ShowMenu(string caption, QuestDictionary<string> options, bool allowCancel)
        {
            return m_worldModel.DisplayMenu(caption, options, allowCancel);
        }

        public string ShowMenu(string caption, QuestList<string> options, bool allowCancel)
        {
            return m_worldModel.DisplayMenu(caption, options, allowCancel);
        }

        public bool DictionaryContains(IDictionary dictionary, string key)
        {
            return dictionary.Contains(key);
        }

        public int DictionaryCount(IDictionary dictionary)
        {
            return dictionary.Count;
        }

        public int ToInt(string number)
        {
            return int.Parse(number);
        }

        public string ToString(int number)
        {
            return number.ToString();
        }

        public bool IsInt(string number)
        {
            int result;
            return int.TryParse(number, out result);
        }
    }
}
