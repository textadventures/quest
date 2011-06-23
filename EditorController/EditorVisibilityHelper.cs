using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest
{
    internal class EditorVisibilityHelper
    {
        private WorldModel m_worldModel;
        private bool m_alwaysVisible = true;
        private string m_relatedAttribute = null;
        private string m_visibleIfRelatedAttributeIsType = null;
        private string m_visibleIfElementInheritsType = null;
        private string m_notVisibleIfElementInheritsType = null;
        private Element m_visibleIfElementInheritsTypeElement = null;
        private Element m_notVisibleIfElementInheritsTypeElement = null;
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
            m_visibleIfElementInheritsType = source.Fields.GetString("mustinherit");
            m_notVisibleIfElementInheritsType = source.Fields.GetString("mustnotinherit");
            if (m_visibleIfElementInheritsType != null || m_notVisibleIfElementInheritsType != null) m_alwaysVisible = false;
            m_filterGroup = source.Fields.GetString("filtergroup");
            m_filter = source.Fields.GetString("filter");
            if (m_filter != null) m_alwaysVisible = false;

            string expression = source.Fields.GetString("onlydisplayif");
            if (expression != null)
            {
                m_visibilityExpression = new Expression<bool>(Utility.ConvertDottedPropertiesToVariable(expression), worldModel);
                m_alwaysVisible = false;
            }
        }

        public bool IsVisible(IEditorData data)
        {
            if (m_alwaysVisible) return true;

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
                    m_visibleIfElementInheritsTypeElement = m_worldModel.Elements.Get(ElementType.ObjectType, m_visibleIfElementInheritsType);
                }
                return m_worldModel.Elements.Get(data.Name).Fields.InheritsType(m_visibleIfElementInheritsTypeElement);
            }

            if (m_notVisibleIfElementInheritsType != null)
            {
                if (m_notVisibleIfElementInheritsTypeElement == null)
                {
                    m_notVisibleIfElementInheritsTypeElement = m_worldModel.Elements.Get(ElementType.ObjectType, m_notVisibleIfElementInheritsType);
                }
                return !m_worldModel.Elements.Get(data.Name).Fields.InheritsType(m_notVisibleIfElementInheritsTypeElement);
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

            if (m_visibilityExpression != null)
            {
                // evaluate <onlydisplayif> expression, with "this" as the current element
                Scripts.Context context = new Scripts.Context();
                context.Parameters = new Scripts.Parameters("this", m_worldModel.Elements.Get(data.Name));
                return m_visibilityExpression.Execute(context);
            }

            return false;
        }
    }
}
