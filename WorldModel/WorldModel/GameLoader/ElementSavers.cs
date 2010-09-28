using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AxeSoftware.Quest
{
    internal partial class GameSaver
    {
        private class FunctionSaver : ElementSaverBase
        {
            #region IElementSaver Members

            public override ElementType AppliesTo
            {
                get { return ElementType.Function; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("function");
                writer.WriteAttributeString("name", e.Name);
                writer.WriteAttributeString("parameters", string.Join(", ", e.Fields[FieldDefinitions.ParamNames].ToArray()));
                writer.WriteAttributeString("type", e.Fields[FieldDefinitions.ReturnType]);
                writer.WriteString(GameSaver.SaveScript(writer, e.Fields[FieldDefinitions.Script], 0));
                writer.WriteEndElement();
            }

            #endregion
        }

        private class ObjectTypeSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.ObjectType; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("type");
                writer.WriteAttributeString("name", e.Name);
                SaveFields(writer, e);
                writer.WriteEndElement();
            }
        }

        private class DelegateSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Delegate; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                // only save the delegate definition, not the individual implementations - they are just fields on objects
                if (!e.MetaFields[MetaFieldDefinitions.DelegateImplementation])
                {
                    writer.WriteStartElement("delegate");
                    writer.WriteAttributeString("name", e.Name);
                    writer.WriteAttributeString("parameters", string.Join(", ", e.Fields[FieldDefinitions.ParamNames].ToArray()));
                    writer.WriteAttributeString("type", e.Fields[FieldDefinitions.ReturnType]);
                    writer.WriteEndElement();
                }
            }
        }

        private class TemplateSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Template; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("template");
                // TO DO: This is a bit of a kludge. Here we get rid of "template!" prefix
                writer.WriteAttributeString("name", e.Name.Substring(9));
                writer.WriteString(e.Fields[FieldDefinitions.Text]);
                writer.WriteEndElement();
            }
        }

        private class DynamicTemplateSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.DynamicTemplate; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("dynamictemplate");
                writer.WriteAttributeString("name", e.Name);
                writer.WriteString(e.Fields[FieldDefinitions.Function].Save());
                writer.WriteEndElement();
            }
        }

        private class InterfaceSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Interface; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("interface");
                writer.WriteAttributeString("src", e.Fields[FieldDefinitions.Filename]);
                writer.WriteEndElement();
            }
        }

        private class EditorsSaver : IElementsSaver
        {
            private EditorSaver m_editorSaver = new EditorSaver();
            private EditorTabSaver m_tabSaver = new EditorTabSaver();
            private EditorControlSaver m_controlSaver = new EditorControlSaver();
            private GameSaver m_gameSaver;

            #region IElementsSaver Members

            public ElementType AppliesTo
            {
                get { return ElementType.Editor; }
            }

            public void Save(GameXmlWriter writer, WorldModel worldModel)
            {
                IEnumerable<Element> allEditors = worldModel.Elements.GetElements(ElementType.Editor);
                IEnumerable<Element> allTabs = worldModel.Elements.GetElements(ElementType.EditorTab);
                IEnumerable<Element> allControls = worldModel.Elements.GetElements(ElementType.EditorControl);

                foreach (Element editor in allEditors.Where(e => !e.MetaFields[MetaFieldDefinitions.Library]))
                {
                    m_editorSaver.StartSave(writer, editor);
                    foreach (Element tab in allTabs.Where(t => t.Parent == editor))
                    {
                        m_tabSaver.StartSave(writer, tab);
                        foreach (Element control in allControls.Where(c => c.Parent == tab))
                        {
                            m_controlSaver.Save(writer, control);
                        }
                        m_tabSaver.EndSave(writer, tab);
                    }
                    m_editorSaver.EndSave(writer, editor);
                }
            }

            public GameSaver GameSaver
            {
                get
                {
                    return m_gameSaver;
                }
                set
                {
                    m_gameSaver = value;
                    m_editorSaver.GameSaver = m_gameSaver;
                    m_tabSaver.GameSaver = m_gameSaver;
                    m_controlSaver.GameSaver = m_gameSaver;
                }
            }

            #endregion
        }

        private class EditorSaver : ElementSaverBase
        {
            public EditorSaver()
            {
                AddIgnoreField("parent");
            }

            public override ElementType AppliesTo
            {
                get { return ElementType.Editor; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                throw new NotImplementedException();
            }

            public void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("editor");
                base.SaveFields(writer, e);
            }

            public void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }

        private class EditorTabSaver : ElementSaverBase
        {
            public EditorTabSaver()
            {
                AddIgnoreField("parent");
            }

            public override ElementType AppliesTo
            {
                get { return ElementType.EditorTab; }
            }

            public void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("tab");
                base.SaveFields(writer, e);
            }

            public void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                throw new NotImplementedException();
            }

            public override bool AutoSave
            {
                get { return false; }
            }
        }

        private class EditorControlSaver : ElementSaverBase
        {
            public EditorControlSaver()
            {
                AddIgnoreField("parent");
            }

            public override ElementType AppliesTo
            {
                get { return ElementType.EditorControl; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("control");
                base.SaveFields(writer, e);
                writer.WriteEndElement();
            }

            public override bool AutoSave
            {
                get { return false; }
            }
        }

        private class WalkthroughSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Walkthrough; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                QuestList<string> steps = e.Fields[FieldDefinitions.Steps];
                if (steps == null || steps.Count == 0) return;

                string result = string.Empty;
                string indent = GameSaver.GetIndentChars(writer.IndentLevel + 1, writer.IndentChars);

                foreach (string step in steps)
                {
                    result += Environment.NewLine + indent + step;
                }
                result += Environment.NewLine;

                writer.WriteStartElement("walkthrough");
                writer.WriteString(result);
                writer.WriteEndElement();
            }
        }

        private class IncludeSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.IncludedLibrary; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("include");
                writer.WriteAttributeString("ref", e.Fields[FieldDefinitions.Filename]);
                writer.WriteEndElement();
            }
        }

        private class ImpliedTypeSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.ImpliedType; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("implied");
                writer.WriteAttributeString("element", e.Fields[FieldDefinitions.Element]);
                writer.WriteAttributeString("property", e.Fields[FieldDefinitions.Property]);
                writer.WriteAttributeString("type", e.Fields[FieldDefinitions.Type]);
                writer.WriteEndElement();
            }
        }
    }
}
