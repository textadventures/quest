using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class Elements
    {
        private Dictionary<string, Element> m_allElements = new Dictionary<string, Element>();
        private Dictionary<ElementType, Dictionary<string, Element>> m_elements = new Dictionary<ElementType, Dictionary<string, Element>>();

        internal Elements()
        {
            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                m_elements.Add(t, new Dictionary<string, Element>());
            }
        }

        public void Add(ElementType t, string key, Element e)
        {
            m_allElements.Add(key, e);
            m_elements[t].Add(key, e);
        }

        public void Remove(ElementType t, string key)
        {
            m_allElements.Remove(key);
            m_elements[t].Remove(key);
        }

        public Element Get(string key)
        {
            return m_allElements[key];
        }

        public IEnumerable<Element> Objects
        {
            get { return GetElements(ElementType.Object); }
        }

        public IEnumerable<Element> ObjectsFiltered(Func<Element, bool> filter)
        {
            return GetElements(ElementType.Object).Where(filter);
        }

        public IEnumerable<Element> GetElements(ElementType t)
        {
            foreach (Element e in m_elements[t].Values) yield return e;
        }

        public bool ContainsKey(ElementType t, string key)
        {
            return m_elements[t].ContainsKey(key);
        }

        public bool ContainsKey(string key)
        {
            return m_allElements.ContainsKey(key);
        }

        public Element Get(ElementType t, string key)
        {
            return m_elements[t][key];
        }

        public bool TryGetValue(ElementType t, string key, out Element element)
        {
            return m_elements[t].TryGetValue(key, out element);
        }

        public int Count(ElementType t)
        {
            return m_elements[t].Count;
        }

        public Element GetSingle(ElementType t)
        {
            foreach (Element e in m_elements[t].Values)
            {
                return e;
            }
            return null;
        }

        public IEnumerable<Element> GetChildElements(Element parent)
        {
            foreach (Element e in m_allElements.Values)
            {
                if (e.Parent == parent)
                {
                    if (e == parent) throw new InvalidOperationException("Object's parent is set to self");
                    yield return e;
                    foreach (Element child in GetChildElements(e))
                    {
                        if (child == parent) throw new InvalidOperationException("Circular parent");
                        yield return child;
                    }
                }
            }
        }
    }
}
