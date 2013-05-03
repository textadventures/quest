using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    public class Template
    {
        private WorldModel m_worldModel;
        private Dictionary<string, Element> m_templateLookup = new Dictionary<string, Element>();

        public Template(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        internal Element AddTemplate(string templateName, string text, bool isCommandTemplate, bool isBaseTemplate = false)
        {
            // if a template is marked as IsBaseTemplate, it's from the base .aslx file so shouldn't be overwritten
            // by an equivalent library definition
            Element existingTemplate;
            if (m_templateLookup.TryGetValue(templateName, out existingTemplate))
            {
                if (existingTemplate.Fields[FieldDefinitions.IsBaseTemplate])
                {
                    return null;
                }
            }

            string elementName = m_worldModel.GetUniqueID("template");

            Element template = m_worldModel.GetElementFactory(ElementType.Template).Create(elementName);
            template.Fields[FieldDefinitions.TemplateName] = templateName;
            template.Fields[FieldDefinitions.Text] = text;
            template.Fields[FieldDefinitions.Anonymous] = true;
            template.Fields[FieldDefinitions.IsBaseTemplate] = isBaseTemplate;

            if (isCommandTemplate)
            {
                template.Fields[FieldDefinitions.IsVerb] = true;
            }

            m_templateLookup[templateName] = template;

            return template;
        }

        public string GetText(string t, bool throwException = true)
        {
            if (!m_templateLookup.ContainsKey(t))
            {
                if (m_worldModel.EditMode && throwException)
                {
                    return string.Format("{{UNKNOWN TEMPLATE: {0}}}", t);
                }
                if (throwException)
                {
                    throw new Exception(string.Format("No template named '{0}'", t));
                }
                return null;
            }
            return m_templateLookup[t].Fields[FieldDefinitions.Text];
        }

        public string GetDynamicText(string t, params Element[] obj)
        {
            Parameters parameters;

            if (obj.Length == 1)
            {
                parameters = new Parameters("object", obj[0]);
            }
            else
            {
                parameters = new Parameters();
                for (int i = 0; i < obj.Length; i++)
                {
                    parameters.Add("object" + (i + 1).ToString(), obj[i]);
                }
            }

            return GetDynamicTextInternal(t, parameters);
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

        internal Element AddVerbTemplate(string c, string text, string filename)
        {
            Element template;

            if (!m_templateLookup.ContainsKey(c))
            {
                template = AddTemplate(c, "", true);
            }
            else
            {
                template = m_templateLookup[c];

                if (m_worldModel.Version >= WorldModelVersion.v530 && template.MetaFields[MetaFieldDefinitions.Filename] != filename)
                {
                    // As of Quest 5.3, if the existing verbtemplate was defined in a different file, clear it out.
                    // This means that language libraries can "reset" the verbtemplates defined in English.aslx.

                    template.Fields[FieldDefinitions.Text] = "";
                }
                else
                {
                    template.Fields[FieldDefinitions.Text] += "; ";
                }
            }

            template.Fields[FieldDefinitions.Text] += text;
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

            if (!m_worldModel.EditMode)
            {
                template.Fields[FieldDefinitions.Function] = new Expression<string>(expression, new ScriptContext(m_worldModel));
            }
            else
            {
                template.Fields[FieldDefinitions.Text] = expression;
            }

            return template;
        }

        public bool TemplateExists(string name)
        {
            return m_templateLookup.ContainsKey(name);
        }

        public bool DynamicTemplateExists(string name)
        {
            return m_worldModel.Elements.ContainsKey(ElementType.DynamicTemplate, name);
        }

        internal Element GetTemplateElement(string name)
        {
            return m_templateLookup[name];
        }

        private Regex m_templateRegex = new Regex(@"\[(?<name>.*?)\]");

        public string ReplaceTemplateText(string text)
        {
            if (text == null) return null;

            int start = 0;

            while (m_templateRegex.IsMatch(text, start))
            {
                var match = m_templateRegex.Match(text, start);
                var templateName = match.Groups["name"].Value;
                var templateValue = GetText(templateName, false) ?? "[" + templateName + "]";
                text = m_templateRegex.Replace(text, templateValue, 1, start);
                start = match.Index + templateValue.Length;
                if (start > text.Length) break;
            }
            return text;
        }
    }
}

