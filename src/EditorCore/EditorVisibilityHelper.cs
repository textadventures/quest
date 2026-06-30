using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

internal class EditorVisibilityHelper
{
    private readonly bool m_alwaysVisible = true;
    private readonly string m_filter;
    private readonly string m_filterGroup;
    private readonly IList<string> m_notVisibleIfElementInheritsType;
    private readonly EditorDefinition m_parent;
    private readonly string m_relatedAttribute;
    private readonly Expression<bool> m_visibilityExpression;
    private readonly IList<string> m_visibleIfElementInheritsType;
    private readonly string m_visibleIfRelatedAttributeIsType;
    private readonly WorldModel m_worldModel;
    private List<Element> m_notVisibleIfElementInheritsTypeElement;
    private List<Element> m_visibleIfElementInheritsTypeElement;

    public EditorVisibilityHelper(EditorDefinition parent, WorldModel worldModel, Element source)
    {
        m_parent = parent;
        m_worldModel = worldModel;
        m_relatedAttribute = source.Fields.GetString("relatedattribute");
        if (m_relatedAttribute != null)
        {
            m_alwaysVisible = false;
        }

        m_visibleIfRelatedAttributeIsType = source.Fields.GetString("relatedattributedisplaytype");
        m_visibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustinherit");
        m_notVisibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustnotinherit");
        if (m_visibleIfElementInheritsType != null || m_notVisibleIfElementInheritsType != null)
        {
            m_alwaysVisible = false;
        }

        m_filterGroup = source.Fields.GetString("filtergroup");
        m_filter = source.Fields.GetString("filter");
        if (m_filter != null)
        {
            m_alwaysVisible = false;
        }

        var expression = source.Fields.GetString("onlydisplayif");
        if (expression != null)
        {
            m_visibilityExpression = new Expression<bool>(Engine.Utility.EncodeIdentifierSpaces(expression),
                new ScriptContext(worldModel, true));
            m_alwaysVisible = false;
        }
    }

    public async Task<bool> IsVisible(IEditorData data)
    {
        if (m_alwaysVisible)
        {
            return true;
        }

        if (m_visibilityExpression != null)
        {
            // evaluate <onlydisplayif> expression, with "this" as the current element
            var context = new Context();
            context.Parameters = new Parameters("this", m_worldModel.Elements.Get(data.Name));
            var result = false;
            try
            {
                result = await m_visibilityExpression.ExecuteAsync(context);
            }
            catch
            {
                // ignore any exceptions which may occur, for example if the element is being deleted
            }

            if (!result)
            {
                return false;
            }
        }

        if (m_notVisibleIfElementInheritsType != null)
        {
            if (m_notVisibleIfElementInheritsTypeElement == null)
            {
                // convert "mustnotinherit" type names list into a list of type elements
                m_notVisibleIfElementInheritsTypeElement = new List<Element>(
                    m_notVisibleIfElementInheritsType.Select(t => m_worldModel.Elements.Get(ElementType.ObjectType, t))
                );
            }

            // if the element does inherit any of the "forbidden" types, then this control is not visible

            var element = m_worldModel.Elements.Get(data.Name);

            foreach (var forbiddenType in m_notVisibleIfElementInheritsTypeElement)
            {
                if (element.Fields.InheritsTypeRecursive(forbiddenType))
                {
                    return false;
                }
            }
        }

        if (m_relatedAttribute != null)
        {
            var relatedAttributeValue = data.GetAttribute(m_relatedAttribute);
            if (relatedAttributeValue is IDataWrapper)
            {
                relatedAttributeValue = ((IDataWrapper) relatedAttributeValue).GetUnderlyingValue();
            }

            var relatedAttributeType = relatedAttributeValue == null
                ? "null"
                : WorldModel.ConvertTypeToTypeName(relatedAttributeValue.GetType());
            return relatedAttributeType == m_visibleIfRelatedAttributeIsType;
        }

        if (m_visibleIfElementInheritsType != null)
        {
            if (m_visibleIfElementInheritsTypeElement == null)
            {
                // convert "mustinherit" type names list into a list of type elements
                m_visibleIfElementInheritsTypeElement = new List<Element>(
                    m_visibleIfElementInheritsType.Select(t => m_worldModel.Elements.Get(ElementType.ObjectType, t))
                );
            }

            // if the element does inherit any of the types, then this control is visible

            var element = m_worldModel.Elements.Get(data.Name);

            foreach (var type in m_visibleIfElementInheritsTypeElement)
            {
                if (element.Fields.InheritsTypeRecursive(type))
                {
                    return true;
                }
            }

            return false;
        }

        if (m_filterGroup != null)
        {
            // This control is visible if the named filtergroup's current filter selection is this control's filter.
            var selectedFilter = data.GetSelectedFilter(m_filterGroup);

            // Or, if the named filtergroup's current filter selection is not set, infer the current filter value
            // based on which attribute is populated for this data.
            if (selectedFilter == null)
            {
                selectedFilter = m_parent.GetDefaultFilterName(m_filterGroup, data);
            }

            return selectedFilter == m_filter;
        }

        return true;
    }
}