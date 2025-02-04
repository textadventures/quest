using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest
{
    internal class EditorVisibilityHelper
    {
        private WorldModel m_worldModel;
        private bool m_alwaysVisible = true;
        private string m_relatedAttribute;
        private string m_visibleIfRelatedAttributeIsType;
        private IList<string> m_visibleIfElementInheritsType;
        private IList<string> m_notVisibleIfElementInheritsType;
        private List<Element> m_visibleIfElementInheritsTypeElement;
        private List<Element> m_notVisibleIfElementInheritsTypeElement;
        private string m_filterGroup;
        private string m_filter;
        private Expression<bool> m_visibilityExpression;
        private EditorDefinition m_parent;

        public EditorVisibilityHelper(EditorDefinition parent, WorldModel worldModel, Element source)
        {
            m_parent = parent;
            m_worldModel = worldModel;
            m_relatedAttribute = source.Fields.GetString("relatedattribute");
            if (m_relatedAttribute != null) m_alwaysVisible = false;
            m_visibleIfRelatedAttributeIsType = source.Fields.GetString("relatedattributedisplaytype");
            m_visibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustinherit");
            m_notVisibleIfElementInheritsType = source.Fields.GetAsType<QuestList<string>>("mustnotinherit");
            if (m_visibleIfElementInheritsType != null || m_notVisibleIfElementInheritsType != null) m_alwaysVisible = false;
            m_filterGroup = source.Fields.GetString("filtergroup");
            m_filter = source.Fields.GetString("filter");
            if (m_filter != null) m_alwaysVisible = false;

            string expression = source.Fields.GetString("onlydisplayif");
            if (expression != null)
            {
                m_visibilityExpression = new Expression<bool>(Utility.ConvertVariablesToFleeFormat(expression), new TextAdventures.Quest.Scripts.ScriptContext(worldModel, true));
                m_alwaysVisible = false;
            }
        }

        public bool IsVisible(IEditorData data)
        {
            if (m_alwaysVisible) return true;

            if (m_visibilityExpression != null)
            {
                // evaluate <onlydisplayif> expression, with "this" as the current element
                Scripts.Context context = new Scripts.Context();
                context.Parameters = new Scripts.Parameters("this", m_worldModel.Elements.Get(data.Name));
                bool result = false;
                try
                {
                    result = m_visibilityExpression.Execute(context);
                }
                catch
                {
                    // ignore any exceptions which may occur, for example if the element is being deleted
                }
                if (!result) return false;
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

                Element element = m_worldModel.Elements.Get(data.Name);

                foreach (Element forbiddenType in m_notVisibleIfElementInheritsTypeElement)
                {
                    if (element.Fields.InheritsTypeRecursive(forbiddenType))
                    {
                        return false;
                    }
                }
            }

            if (m_relatedAttribute != null)
            {
                object relatedAttributeValue = data.GetAttribute(m_relatedAttribute);
                if (relatedAttributeValue is IDataWrapper) relatedAttributeValue = ((IDataWrapper)relatedAttributeValue).GetUnderlyingValue();

                string relatedAttributeType = relatedAttributeValue == null ? "null" : WorldModel.ConvertTypeToTypeName(relatedAttributeValue.GetType());
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

                Element element = m_worldModel.Elements.Get(data.Name);

                foreach (Element type in m_visibleIfElementInheritsTypeElement)
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
                string selectedFilter = data.GetSelectedFilter(m_filterGroup);

                // Or, if the named filtergroup's current filter selection is not set, infer the current filter value
                // based on which attribute is populated for this data.
                if (selectedFilter == null) selectedFilter = m_parent.GetDefaultFilterName(m_filterGroup, data);

                return (selectedFilter == m_filter);
            }

            return true;
        }
    }
}
