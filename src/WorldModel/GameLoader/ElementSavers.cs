using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TextAdventures.Quest
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
                if (e.Fields[FieldDefinitions.ParamNames] != null && e.Fields[FieldDefinitions.ParamNames].Count > 0)
                {
                    writer.WriteAttributeString("parameters", string.Join(", ", e.Fields[FieldDefinitions.ParamNames].ToArray()));
                }
                if (!string.IsNullOrEmpty(e.Fields[FieldDefinitions.ReturnType]))
                {
                    writer.WriteAttributeString("type", e.Fields[FieldDefinitions.ReturnType]);
                }
                if (e.Fields[FieldDefinitions.Script] != null)
                {
                    writer.WriteString(GameSaver.SaveScript(writer, e.Fields[FieldDefinitions.Script], 0));
                }
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
                writer.WriteAttributeString("name", e.Fields[FieldDefinitions.TemplateName]);
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

                if (!GameSaver.m_worldModel.EditMode)
                {
                    writer.WriteString(e.Fields[FieldDefinitions.Function].Save());
                }
                else
                {
                    writer.WriteString(e.Fields[FieldDefinitions.Text]);
                }
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
                    foreach (Element tab in allTabs.Where(t => t.Parent == editor).OrderBy(t => t.MetaFields[MetaFieldDefinitions.SortIndex]))
                    {
                        m_tabSaver.StartSave(writer, tab);
                        foreach (Element control in allControls.Where(c => c.Parent == tab).OrderBy(c => c.MetaFields[MetaFieldDefinitions.SortIndex]))
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

        private class WalkthroughsSaver : IElementsSaver
        {
            private WalkthroughSaver m_walkthroughSaver = new WalkthroughSaver();

            public ElementType AppliesTo
            {
                get { return ElementType.Walkthrough; }
            }

            public void Save(GameXmlWriter writer, WorldModel worldModel)
            {
                if (!writer.Options.IncludeWalkthrough) return;
                foreach (Element walkThrough in worldModel.Elements.GetElements(ElementType.Walkthrough).Where(e => e.Parent == null))
                {
                    SaveElementAndChildren(writer, worldModel, walkThrough);
                }
            }

            private void SaveElementAndChildren(GameXmlWriter writer, WorldModel worldModel, Element walkThrough)
            {
                m_walkthroughSaver.StartSave(writer, walkThrough);

                IEnumerable<Element> orderedChildren = from child in worldModel.Elements.GetElements(ElementType.Walkthrough)
                                                       where child.Parent == walkThrough
                                                       orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
                                                       select child;

                foreach (Element child in orderedChildren)
                {
                    SaveElementAndChildren(writer, worldModel, child);
                }

                m_walkthroughSaver.EndSave(writer, walkThrough);
            }

            public GameSaver GameSaver
            {
                set
                {
                    m_walkthroughSaver.GameSaver = value;
                }
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
                StartSave(writer, e);
                EndSave(writer, e);
            }

            public void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("walkthrough");
                writer.WriteAttributeString("name", e.Name);

                QuestList<string> steps = e.Fields[FieldDefinitions.Steps];
                if (steps != null && steps.Count > 0)
                {
                    string result = string.Empty;
                    string indent = Utility.GetIndentChars(writer.IndentLevel + 1, writer.IndentChars);

                    foreach (string step in steps)
                    {
                        result += Environment.NewLine + indent + step;
                    }
                    result += Environment.NewLine;

                    writer.WriteStartElement("steps");
                    writer.WriteAttributeString("type", GameSaver.Version <= WorldModelVersion.v530 ? "list" : "simplestringlist");
                    writer.WriteString(result);
                    writer.WriteEndElement();
                }
            }

            public void EndSave(GameXmlWriter writer, Element e)
            {
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
                string filename = e.Fields[FieldDefinitions.Filename];
                if (string.IsNullOrEmpty(filename)) return;
                writer.WriteStartElement("include");
                writer.WriteAttributeString("ref", filename);
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

        private class JavascriptSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Javascript; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                string filename = e.Fields[FieldDefinitions.Src];
                if (string.IsNullOrEmpty(filename)) return;
                writer.WriteStartElement("javascript");
                writer.WriteAttributeString("src", filename);
                writer.WriteEndElement();
            }
        }

        private class TimerSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Timer; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("timer");
                writer.WriteAttributeString("name", e.Name);
                base.SaveFields(writer, e);
                writer.WriteEndElement();
            }
        }

        private class ResourceSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Resource; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                // Resource elements should never need to be saved. They can only
                // be defined by the Core library and are there for information only
                // (specifying which additional files to include in a .quest file)
            }
        }

        private class OutputSaver : ElementSaverBase
        {
            public override ElementType AppliesTo
            {
                get { return ElementType.Output; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("output");
                base.SaveFields(writer, e);
                writer.WriteEndElement();
            }
        }
    }
}
