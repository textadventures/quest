using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;
using System.Collections;

namespace TextAdventures.Quest.Functions
{
    internal class ExpressionOwner
    {
        private WorldModel m_worldModel;
        private Random m_random = new Random();

        public ExpressionOwner(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public string Template(string template)
        {
            return m_worldModel.Template.GetText(template);
        }

        public string DynamicTemplate(string template, params Element[] obj)
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

        public bool HasInt(Element obj, string property)
        {
            return obj.Fields.HasType<int>(property);
        }

        public int GetInt(Element obj, string property)
        {
            return obj.Fields.GetAsType<int>(property);
        }

        public bool HasDouble(Element obj, string property)
        {
            return obj.Fields.HasType<double>(property);
        }

        public double GetDouble(Element obj, string property)
        {
            return obj.Fields.GetAsType<double>(property);
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

        public object GetAttribute(Element obj, string property)
        {
            return obj.Fields.Get(property);
        }

        public bool HasAttribute(Element obj, string property)
        {
            return obj.Fields.Exists(property, true);
        }

        public QuestList<string> GetAttributeNames(Element obj, bool includeInheritedAttributes)
        {
            return new QuestList<string>(obj.Fields.GetAttributeNames(includeInheritedAttributes));
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

        public QuestList<Element> AllTurnScripts()
        {
            QuestList<Element> result = new QuestList<Element>();

            foreach (Element item in m_worldModel.GetAllObjects().Where(o => o.Type == ObjectType.TurnScript))
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
            try
            {
                return list[index];
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new Exception(string.Format("ListItem: index {0} is out of range for this list ({1} items, last index is {2})", index, list.Count, list.Count - 1), ex);
            }
        }

        public string StringListItem(IQuestList list, int index)
        {
            try
            {
                return list[index] as string;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new Exception(string.Format("StringListItem: index {0} is out of range for this list ({1} items, last index is {2})", index, list.Count, list.Count - 1), ex);
            }
        }

        public Element ObjectListItem(IQuestList list, int index)
        {
            try
            {
                return list[index] as Element;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new Exception(string.Format("ObjectListItem: index {0} is out of range for this list ({1} items, last index is {2})", index, list.Count, list.Count - 1), ex);
            }
        }

        public Element GetObject(string name)
        {
            return TryGetElement(ElementType.Object, name);
        }

        public Element GetTimer(string name)
        {
            return TryGetElement(ElementType.Timer, name);
        }

        private Element TryGetElement(ElementType type, string name)
        {
            Element result;
            m_worldModel.Elements.TryGetValue(type, name, out result);
            return result;
        }

        public string TypeOf(Element obj, string attribute)
        {
            object value = obj.Fields.Get(attribute);
            return TypeOf(value);
        }

        public string TypeOf(object value)
        {
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

        public string SafeXML(string input)
        {
            return Utility.SafeXML(input);
        }

        public bool IsRegexMatch(string regexPattern, string input)
        {
            return Utility.IsRegexMatch(regexPattern, input);
        }

        public bool IsRegexMatch(string regexPattern, string input, string cacheID)
        {
            return Utility.IsRegexMatch(regexPattern, input, m_worldModel.RegexCache, cacheID);
        }

        public int GetMatchStrength(string regexPattern, string input)
        {
            return Utility.GetMatchStrength(regexPattern, input);
        }

        public int GetMatchStrength(string regexPattern, string input, string cacheID)
        {
            return Utility.GetMatchStrength(regexPattern, input, m_worldModel.RegexCache, cacheID);
        }

        public QuestDictionary<string> Populate(string regexPattern, string input)
        {
            return Utility.Populate(regexPattern, input);
        }

        public QuestDictionary<string> Populate(string regexPattern, string input, string cacheID)
        {
            return Utility.Populate(regexPattern, input, m_worldModel.RegexCache, cacheID);
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

        public IScript ScriptDictionaryItem(IDictionary dictionary, string key)
        {
            return dictionary[key] as IScript;
        }

        public string ShowMenu(string caption, QuestDictionary<string> options, bool allowCancel)
        {
            if (m_worldModel.Version >= WorldModelVersion.v540)
            {
                throw new Exception("The 'ShowMenu' function is not supported for games written for Quest 5.4 or later. Use the 'show menu' script command instead.");
            }
            return m_worldModel.DisplayMenu(caption, options, allowCancel, false);
        }

        public string ShowMenu(string caption, QuestList<string> options, bool allowCancel)
        {
            if (m_worldModel.Version >= WorldModelVersion.v540)
            {
                throw new Exception("The 'ShowMenu' function is not supported for games written for Quest 5.4 or later. Use the 'show menu' script command instead.");
            }
            return m_worldModel.DisplayMenu(caption, options, allowCancel, false);
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
            return int.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        public double ToDouble(string number)
        {
            return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToString(int number)
        {
            return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public string ToString(double number)
        {
            return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public bool IsInt(string number)
        {
            int result;
            return int.TryParse(number, out result);
        }

        public bool IsDouble(string number)
        {
            double result;
            return double.TryParse(number, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        public string GetInput()
        {
            if (m_worldModel.Version >= WorldModelVersion.v540)
            {
                throw new Exception("The 'GetInput' function is not supported for games written for Quest 5.4 or later. Use the 'get input' script command instead.");
            }
            return m_worldModel.GetNextCommandInput(false);
        }

        public string GetFileURL(string filename)
        {
            if (filename.Contains("..")) throw new ArgumentOutOfRangeException("Invalid filename");
            return m_worldModel.GetExternalURL(filename);
        }

        public string GetUniqueElementName(string name)
        {
            return m_worldModel.GetUniqueElementName(name);
        }

        public bool Ask(string caption)
        {
            if (m_worldModel.Version >= WorldModelVersion.v540)
            {
                throw new Exception("The 'Ask' function is not supported for games written for Quest 5.4 or later. Use the 'ask' script command instead.");
            }
            return m_worldModel.ShowQuestion(caption);
        }

        public int GetRandomInt(int min, int max)
        {
            // The .net implementation of Random.Next defines the minValue as the
            // inclusive lower bound, but maxValue as the Exclusive upper bound.
            // It makes a bit more sense for maxValue to be inclusive, so we add 1.
            return m_random.Next(min, max + 1);
        }

        public double GetRandomDouble()
        {
            return m_random.NextDouble();
        }

        public object Eval(string expression)
        {
            return Eval(expression, null);
        }

        public object Eval(string expression, IDictionary parameters)
        {
            ExpressionGeneric expr = new ExpressionGeneric(expression, new ScriptContext(m_worldModel));
            Context context = new Context();
            if (parameters != null) context.Parameters = new Parameters(parameters);
            return expr.Execute(context);
        }

        public Element Clone(Element element)
        {
            Element newElement = element.Clone();
            return newElement;
        }

        public bool DoesInherit(Element element, string typeName)
        {
            Element type = m_worldModel.Elements.Get(ElementType.ObjectType, typeName);
            return element.Fields.InheritsTypeRecursive(type);
        }

        public QuestList<string> ListCombine(QuestList<string> list1, QuestList<string> list2)
        {
            return ListCombine<string>(list1, list2);
        }

        public QuestList<Element> ListCombine(QuestList<Element> list1, QuestList<Element> list2)
        {
            return ListCombine<Element>(list1, list2);
        }

        private QuestList<T> ListCombine<T>(QuestList<T> list1, QuestList<T> list2)
        {
            if (list1 == null) return new QuestList<T>(list2);
            return list1.MergeLists(list2);
        }

        public QuestList<string> ListExclude(QuestList<string> list, string str)
        {
            return list.Exclude(str);
        }

        public QuestList<Element> ListExclude(QuestList<Element> list, Element element)
        {
            return list.Exclude(element);
        }

        public QuestList<string> ListExclude(QuestList<string> list, QuestList<string> excludeList)
        {
            return list.Exclude(excludeList);
        }

        public QuestList<Element> ListExclude(QuestList<Element> list, QuestList<Element> excludeList)
        {
            return list.Exclude(excludeList);
        }

        public QuestList<Element> GetAllChildObjects(Element element)
        {
            return GetAllChildren(element, ObjectType.Object);
        }

        private QuestList<Element> GetAllChildren(Element element, ObjectType type)
        {
            QuestList<Element> result = new QuestList<Element>();
            foreach (Element child in m_worldModel.Elements.GetDirectChildren(element).Where(e => e.ElemType == ElementType.Object && e.Type == type))
            {
                result.Add(child);
                result.AddRange(GetAllChildren(child, type));
            }
            return result;
        }

        public QuestList<Element> GetDirectChildren(Element element)
        {
            return new QuestList<Element>(
                m_worldModel.Elements.GetDirectChildren(element)
                .Where(e => e.ElemType == ElementType.Object && e.Type == ObjectType.Object));
        }

        public bool IsGameRunning()
        {
            return m_worldModel.State != GameState.Finished;
        }

        public QuestList<Element> ObjectListSort(QuestList<Element> list, params string[] attribute)
        {
            IOrderedEnumerable<Element> result = list.OrderBy(e => e.Fields.Get(attribute[0]));
            for (int i = 1; i < attribute.Length; i++)
            {
                int idx = i;    // need a local copy of i for the lambda
                result = result.ThenBy(e => e.Fields.Get(attribute[idx]));
            }
            return new QuestList<Element>(result);
        }

        public QuestList<Element> ObjectListSortDescending(QuestList<Element> list, params string[] attribute)
        {
            return new QuestList<Element>(ObjectListSort(list, attribute).Reverse());
        }

        public QuestList<string> StringListSort(QuestList<string> list)
        {
            return new QuestList<string>(list.OrderBy(item => item));
        }

        public QuestList<string> StringListSortDescending(QuestList<string> list)
        {
            return new QuestList<string>(StringListSort(list).Reverse());
        }
    }
}
