using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class Elements
    {
        private Dictionary<string, Element> m_allElements = new Dictionary<string, Element>();
        private Dictionary<ElementType, Dictionary<string, Element>> m_elements = new Dictionary<ElementType, Dictionary<string, Element>>();
        private Dictionary<ElementType, List<Element>> m_elementsLists = new Dictionary<ElementType, List<Element>>();

        public event EventHandler<NameChangedEventArgs> ElementRenamed;

        internal Elements()
        {
            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                m_elements.Add(t, new Dictionary<string, Element>());
                m_elementsLists.Add(t, new List<Element>());
            }
        }

        public void Add(ElementType t, string key, Element e)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Invalid object name");
            }

            if (m_allElements.ContainsKey(key))
            {
                // An element with this name already exists. This is OK if the new element
                // is of the same type - then it will just override the previous element.

                if (!m_elements[t].ContainsKey(key))
                {
                    throw new Exception(string.Format(
                        "Element '{0}' of type '{1}' cannot override the existing element of type '{2}'",
                        key,
                        t,
                        m_allElements[key].ElemType));
                }

                // element is being overridden, so detach the event handler
                m_allElements[key].Fields.NameChanged -= ElementNameChanged;

                // remove old element from ordered elements list
                m_elementsLists[t].Remove(m_elements[t][key]);
            }

            m_allElements[key] = e;
            m_elements[t][key] = e;
            m_elementsLists[t].Add(e);

            e.Fields.NameChanged += ElementNameChanged;
        }

        void ElementNameChanged(object sender, NameChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Element.Name))
            {
                throw new ArgumentException("Invalid object name");
            }

            m_allElements.Remove(e.OldName);
            m_elements[e.Element.ElemType].Remove(e.OldName);

            m_allElements.Add(e.Element.Name, e.Element);
            m_elements[e.Element.ElemType].Add(e.Element.Name, e.Element);

            if (ElementRenamed != null) ElementRenamed(this, e);
        }

        internal void Remove(ElementType t, string key)
        {
            m_allElements.Remove(key);
            m_elementsLists[t].Remove(m_elements[t][key]);
            m_elements[t].Remove(key);
        }

        public Element Get(string key)
        {
            try
            {
                return m_allElements[key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("Element not found: '{0}'", key), ex);
            }
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
            foreach (Element e in m_elementsLists[t]) yield return e;
        }

        public IEnumerable<Element> GetElements()
        {
            foreach (Element e in m_allElements.Values) yield return e;
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
            try
            {
                return m_elements[t][key];
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No element '{0}' of type '{1}'", key, t), ex);
            }
        }

        public bool TryGetValue(ElementType t, string key, out Element element)
        {
            return m_elements[t].TryGetValue(key, out element);
        }

        public int Count(ElementType t)
        {
            return m_elements[t].Count;
        }

        public int Count()
        {
            return m_allElements.Count;
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

        public IEnumerable<Element> GetDirectChildren(Element parent)
        {
            return from element in m_allElements.Values where element.Parent == parent select element;
        }
    }
}
