using System;
using System.Collections.Generic;
using AxeSoftware.Quest.Functions;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public class Template
    {
        private WorldModel m_worldModel;
        // TO DO: This is nasty
        private const string k_namePrefix = "template!";

        public Template(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        internal Element AddTemplate(string t, string text)
        {
            Element template;
            t = k_namePrefix + t;
            if (m_worldModel.Elements.ContainsKey(ElementType.Template, t))
            {
                template = m_worldModel.Elements.Get(ElementType.Template, t);
            }
            else
            {
                template = m_worldModel.GetElementFactory(ElementType.Template).Create(t);
            }
            template.Fields[FieldDefinitions.Text] = text;

            return template;
        }

        public string GetText(string t)
        {
            t = k_namePrefix + t;
            if (!m_worldModel.Elements.ContainsKey(ElementType.Template, t)) throw new Exception(string.Format("No template named '{0}'", t));
            return m_worldModel.Elements.Get(ElementType.Template, t).Fields[FieldDefinitions.Text];
        }

        public string GetDynamicText(string t, Element obj)
        {
            return GetDynamicTextInternal(t, new Parameters("object", obj));
        }

        public string GetDynamicText(string t, string text)
        {
            return GetDynamicTextInternal(t, new Parameters("text", text));
        }

        private string GetDynamicTextInternal(string t, Parameters parameters)
        {
            // if there is no dynamictemplate of this name, return the "ordinary" template instead.
            if (!m_worldModel.Elements.ContainsKey(ElementType.DynamicTemplate, t)) return GetText(t);
            Context c = new Context();
            c.Parameters = parameters;
            Element template = m_worldModel.Elements.Get(ElementType.DynamicTemplate, t);
            return template.Fields[FieldDefinitions.Function].Execute(c);
        }

        internal Element AddVerbTemplate(string c, string text)
        {
            Element template;

            if (!m_worldModel.Elements.ContainsKey(ElementType.Template, k_namePrefix + c))
            {
                template = AddTemplate(c, "");
            }
            else
            {
                template = m_worldModel.Elements.Get(ElementType.Template, k_namePrefix + c);
                template.Fields[FieldDefinitions.Text] += "|";
            }
            template.Fields[FieldDefinitions.Text] += "^" + text + " (?<object>.*)$";
            return template;
        }

        internal Element AddDynamicTemplate(string t, string expression)
        {
            Element template;
            if (m_worldModel.Elements.ContainsKey(ElementType.DynamicTemplate, t))
            {
                template = m_worldModel.Elements.Get(ElementType.DynamicTemplate, t);
            }
            else
            {
                template = m_worldModel.GetElementFactory(ElementType.DynamicTemplate).Create(t);
            }
            template.Fields[FieldDefinitions.Function] = new Expression<string>(expression);
            return template;
        }
    }
}
