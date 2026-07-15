#nullable disable
namespace QuestViva.Engine;

public class Elements
{
    private readonly Dictionary<string, Element> m_allElements = new();
    private readonly Dictionary<ElementType, Dictionary<string, Element>> m_elements = new();
    private readonly Dictionary<ElementType, List<Element>> m_elementsLists = new();

    // Index of parent -> direct children, kept in sync by Add/Remove/UpdateParentIndex so that
    // GetDirectChildren doesn't need to do a full scan of every element in the game.
    private readonly List<Element> m_rootElements = new();
    private readonly Dictionary<Element, List<Element>> m_childrenByParent = new();

    // Elements that have completed at least one Add() call, and so are eligible to have their
    // parent-index entry moved by UpdateParentIndex. Elements being cloned have "parent" copied
    // onto them (via Fields.Clone) before they've been added, so that early parent assignment
    // must not touch the index - Add() below performs the baseline registration instead.
    private readonly HashSet<Element> m_indexedElements = new();

    // Per-parent counters backing UpdateElementSortOrder, so assigning a new child's SortIndex
    // doesn't need to scan all existing siblings to find the current max. SortIndex is only ever
    // used for relative ordering (OrderBy) or pairwise swapping of two already-issued values, so
    // a monotonically increasing counter per parent is safe even though it doesn't reclaim gaps
    // left by removed elements.
    private readonly Dictionary<Element, int> m_nextSortIndexByParent = new();
    private int m_nextRootSortIndex;

    internal Elements()
    {
        foreach (ElementType t in Enum.GetValues<ElementType>())
        {
            m_elements.Add(t, new Dictionary<string, Element>());
            m_elementsLists.Add(t, new List<Element>());
        }
    }

    public IEnumerable<Element> Objects => GetElements(ElementType.Object);

    public event EventHandler<NameChangedEventArgs> ElementRenamed;

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
            var oldElement = m_allElements[key];
            oldElement.Fields.NameChanged -= ElementNameChanged;

            // remove old element from ordered elements list
            m_elementsLists[t].Remove(m_elements[t][key]);

            // and from the parent index, since it's being displaced from the live element graph
            RemoveFromParentIndex(oldElement, oldElement.Parent);
            m_indexedElements.Remove(oldElement);
        }

        m_allElements[key] = e;
        m_elements[t][key] = e;
        m_elementsLists[t].Add(e);

        // Baseline parent-index registration. Covers both fresh elements (Parent is still null
        // at this point) and clones (Parent may already be set, copied from the clone source by
        // Fields.Clone before Add() ran) and undo/redo re-creation of a previously-removed element.
        AddToParentIndex(e, e.Parent);
        m_indexedElements.Add(e);

        e.Fields.NameChanged += ElementNameChanged;
    }

    private void ElementNameChanged(object sender, NameChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Element.Name))
        {
            throw new ArgumentException("Invalid object name");
        }

        m_allElements.Remove(e.OldName);
        m_elements[e.Element.ElemType].Remove(e.OldName);

        m_allElements.Add(e.Element.Name, e.Element);
        m_elements[e.Element.ElemType].Add(e.Element.Name, e.Element);

        if (ElementRenamed != null)
        {
            ElementRenamed(this, e);
        }
    }

    internal void Remove(ElementType t, string key)
    {
        if (m_allElements.TryGetValue(key, out var element))
        {
            RemoveFromParentIndex(element, element.Parent);
            m_indexedElements.Remove(element);
        }

        m_allElements.Remove(key);
        m_elementsLists[t].Remove(m_elements[t][key]);
        m_elements[t].Remove(key);
    }

    // Called by Element.SetParentFromFields whenever an element's parent changes, so the
    // parent -> children index stays in sync without needing a full rescan of all elements.
    internal void UpdateParentIndex(Element child, Element oldParent, Element newParent)
    {
        if (!m_indexedElements.Contains(child))
        {
            // Still under construction (e.g. Fields.Clone copying "parent" before the clone has
            // been added) - Add() will perform the baseline registration once it's added.
            return;
        }

        RemoveFromParentIndex(child, oldParent);
        AddToParentIndex(child, newParent);
    }

    private void AddToParentIndex(Element child, Element parent)
    {
        if (parent == null)
        {
            m_rootElements.Add(child);
            return;
        }

        if (!m_childrenByParent.TryGetValue(parent, out var children))
        {
            children = new List<Element>();
            m_childrenByParent[parent] = children;
        }

        children.Add(child);
    }

    private void RemoveFromParentIndex(Element child, Element parent)
    {
        if (parent == null)
        {
            m_rootElements.Remove(child);
            return;
        }

        if (m_childrenByParent.TryGetValue(parent, out var children))
        {
            children.Remove(child);
        }
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

    public IEnumerable<Element> ObjectsFiltered(Func<Element, bool> filter)
    {
        return GetElements(ElementType.Object).Where(filter);
    }

    public IEnumerable<Element> GetElements(ElementType t)
    {
        foreach (var e in m_elementsLists[t])
        {
            yield return e;
        }
    }

    public IEnumerable<Element> GetElements()
    {
        foreach (var e in m_allElements.Values)
        {
            yield return e;
        }
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
        foreach (var e in m_elements[t].Values)
        {
            return e;
        }

        return null;
    }

    public IEnumerable<Element> GetChildElements(Element parent)
    {
        var visited = new HashSet<Element> { parent };
        return CollectChildElements(parent, visited);
    }

    private IEnumerable<Element> CollectChildElements(Element parent, HashSet<Element> visited)
    {
        foreach (var e in GetDirectChildren(parent))
        {
            if (!visited.Add(e)) continue;
            yield return e;
            foreach (var child in CollectChildElements(e, visited))
            {
                yield return child;
            }
        }
    }

    public IEnumerable<Element> GetDirectChildren(Element parent)
    {
        if (parent == null)
        {
            return m_rootElements;
        }

        return m_childrenByParent.TryGetValue(parent, out var children) ? children : [];
    }

    internal int GetNextSortIndex(Element parent)
    {
        if (parent == null)
        {
            return m_nextRootSortIndex++;
        }

        var next = m_nextSortIndexByParent.GetValueOrDefault(parent);
        m_nextSortIndexByParent[parent] = next + 1;
        return next;
    }
}