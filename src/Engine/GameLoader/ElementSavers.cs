using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedType.Local

namespace QuestViva.Engine.GameLoader;

internal partial class GameSaver
{
    private class FunctionSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Function;

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
    }

    private class ObjectTypeSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.ObjectType;

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
        public override ElementType AppliesTo => ElementType.Delegate;

        public override void Save(GameXmlWriter writer, Element e)
        {
            // only save the delegate definition, not the individual implementations - they are just fields on objects
            if (e.MetaFields[MetaFieldDefinitions.DelegateImplementation])
            {
                return;
            }

            writer.WriteStartElement("delegate");
            writer.WriteAttributeString("name", e.Name);
            writer.WriteAttributeString("parameters", string.Join(", ", e.Fields[FieldDefinitions.ParamNames].ToArray()));
            writer.WriteAttributeString("type", e.Fields[FieldDefinitions.ReturnType]);
            writer.WriteEndElement();
        }
    }

    private class TemplateSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Template;

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
        public override ElementType AppliesTo => ElementType.DynamicTemplate;

        public override void Save(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("dynamictemplate");
            writer.WriteAttributeString("name", e.Name);

            writer.WriteString(GameSaver._worldModel.EditMode
                ? e.Fields[FieldDefinitions.Text]
                : e.Fields[FieldDefinitions.Function].Save());
            writer.WriteEndElement();
        }
    }

    private class EditorsSaver : IElementsSaver
    {
        private readonly EditorSaver _editorSaver = new();
        private readonly EditorTabSaver _tabSaver = new();
        private readonly EditorControlSaver _controlSaver = new();

        public ElementType AppliesTo => ElementType.Editor;

        public void Save(GameXmlWriter writer, WorldModel worldModel)
        {
            var allEditors = worldModel.Elements.GetElements(ElementType.Editor);
            var allTabs = worldModel.Elements.GetElements(ElementType.EditorTab).ToArray();
            var allControls = worldModel.Elements.GetElements(ElementType.EditorControl).ToArray();

            foreach (var editor in allEditors.Where(e => !e.MetaFields[MetaFieldDefinitions.Library]))
            {
                _editorSaver.StartSave(writer, editor);
                foreach (var tab in allTabs.Where(t => t.Parent == editor).OrderBy(t => t.MetaFields[MetaFieldDefinitions.SortIndex]))
                {
                    _tabSaver.StartSave(writer, tab);
                    foreach (var control in allControls.Where(c => c.Parent == tab).OrderBy(c => c.MetaFields[MetaFieldDefinitions.SortIndex]))
                    {
                        _controlSaver.Save(writer, control);
                    }
                    _tabSaver.EndSave(writer);
                }
                _editorSaver.EndSave(writer);
            }
        }

        public GameSaver GameSaver
        {
            set
            {
                _editorSaver.GameSaver = value;
                _tabSaver.GameSaver = value;
                _controlSaver.GameSaver = value;
            }
        }
    }

    private class EditorSaver : ElementSaverBase
    {
        public EditorSaver()
        {
            AddIgnoreField("parent");
        }

        public override ElementType AppliesTo => ElementType.Editor;

        public override void Save(GameXmlWriter writer, Element e)
        {
            throw new NotImplementedException();
        }

        public void StartSave(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("editor");
            SaveFields(writer, e);
        }

        public void EndSave(GameXmlWriter writer)
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

        public override ElementType AppliesTo => ElementType.EditorTab;

        public void StartSave(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("tab");
            SaveFields(writer, e);
        }

        public void EndSave(GameXmlWriter writer)
        {
            writer.WriteEndElement();
        }

        public override void Save(GameXmlWriter writer, Element e)
        {
            throw new NotImplementedException();
        }

        public override bool AutoSave => false;
    }

    private class EditorControlSaver : ElementSaverBase
    {
        public EditorControlSaver()
        {
            AddIgnoreField("parent");
        }

        public override ElementType AppliesTo => ElementType.EditorControl;

        public override void Save(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("control");
            SaveFields(writer, e);
            writer.WriteEndElement();
        }

        public override bool AutoSave => false;
    }

    private class WalkthroughsSaver : IElementsSaver
    {
        private readonly WalkthroughSaver _walkthroughSaver = new();

        public ElementType AppliesTo => ElementType.Walkthrough;

        public void Save(GameXmlWriter writer, WorldModel worldModel)
        {
            if (!writer.Options.IncludeWalkthrough) return;
            foreach (var walkThrough in worldModel.Elements.GetElements(ElementType.Walkthrough)
                         .Where(e => e.Parent == null))
            {
                SaveElementAndChildren(writer, worldModel, walkThrough);
            }
        }

        private void SaveElementAndChildren(GameXmlWriter writer, WorldModel worldModel, Element walkThrough)
        {
            _walkthroughSaver.StartSave(writer, walkThrough);

            IEnumerable<Element> orderedChildren = from child in worldModel.Elements.GetElements(ElementType.Walkthrough)
                where child.Parent == walkThrough
                orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
                select child;

            foreach (var child in orderedChildren)
            {
                SaveElementAndChildren(writer, worldModel, child);
            }

            _walkthroughSaver.EndSave(writer);
        }

        public GameSaver GameSaver
        {
            set => _walkthroughSaver.GameSaver = value;
        }
    }

    private class WalkthroughSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Walkthrough;

        public override void Save(GameXmlWriter writer, Element e)
        {
            StartSave(writer, e);
            EndSave(writer);
        }

        public void StartSave(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("walkthrough");
            writer.WriteAttributeString("name", e.Name);

            var steps = e.Fields[FieldDefinitions.Steps];
            if (steps is not {Count: > 0})
            {
                return;
            }

            var result = string.Empty;
            var indent = Utility.GetIndentChars(writer.IndentLevel + 1, GameXmlWriter.IndentChars);

            result = steps.Aggregate(result, (current, step) => current + Environment.NewLine + indent + step);
            result += Environment.NewLine;

            writer.WriteStartElement("steps");
            writer.WriteAttributeString("type", GameSaver.Version <= WorldModelVersion.v530 ? "list" : "simplestringlist");
            writer.WriteString(result);
            writer.WriteEndElement();
        }

        public void EndSave(GameXmlWriter writer)
        {
            writer.WriteEndElement();
        }
    }

    private class IncludeSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.IncludedLibrary;

        public override void Save(GameXmlWriter writer, Element e)
        {
            var filename = e.Fields[FieldDefinitions.Filename];
            if (string.IsNullOrEmpty(filename)) return;
            writer.WriteStartElement("include");
            writer.WriteAttributeString("ref", filename);
            writer.WriteEndElement();
        }
    }

    private class ImpliedTypeSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.ImpliedType;

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
        public override ElementType AppliesTo => ElementType.Javascript;

        public override void Save(GameXmlWriter writer, Element e)
        {
            var filename = e.Fields[FieldDefinitions.Src];
            if (string.IsNullOrEmpty(filename)) return;
            writer.WriteStartElement("javascript");
            writer.WriteAttributeString("src", filename);
            writer.WriteEndElement();
        }
    }

    private class TimerSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Timer;

        public override void Save(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("timer");
            writer.WriteAttributeString("name", e.Name);
            SaveFields(writer, e);
            writer.WriteEndElement();
        }
    }

    private class ResourceSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Resource;

        public override void Save(GameXmlWriter writer, Element e)
        {
            // Resource elements should never need to be saved. They can only
            // be defined by the Core library and are there for information only
            // (specifying which additional files to include in a .quest file)
        }
    }

    private class OutputSaver : ElementSaverBase
    {
        public override ElementType AppliesTo => ElementType.Output;

        public override void Save(GameXmlWriter writer, Element e)
        {
            writer.WriteStartElement("output");
            SaveFields(writer, e);
            writer.WriteEndElement();
        }
    }
}