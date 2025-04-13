using System;
using System.Collections.Generic;
using System.Linq;
using QuestViva.Utility;
// ReSharper disable UnusedType.Local

namespace QuestViva.Engine.GameLoader;

internal partial class GameSaver
{
    private class ObjectsSaver : IElementsSaver
    {
        public ElementType AppliesTo => ElementType.Object;

        public void Save(GameXmlWriter writer, WorldModel worldModel)
        {
            var elementSaver = new ObjectSaver
            {
                GameSaver = GameSaver
            };

            var allObjects = worldModel.Elements.GetElements(ElementType.Object)
                .OrderBy(o => o.MetaFields[MetaFieldDefinitions.SortIndex]).ToArray();

            foreach (var e in allObjects.Where(e => e.Parent == null && GameSaver.CanSave(e)))
            {
                SaveObjectAndChildren(writer, allObjects, e, elementSaver);
            }
        }

        public required GameSaver GameSaver { get; set; }

        private void SaveObjectAndChildren(GameXmlWriter writer, Element[] allObjects, Element e, ObjectSaver saver)
        {
            saver.StartSave(writer, e);
            var orderedChildren = from child in allObjects
                where child.Parent == e
                orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
                select child;

            foreach (var child in orderedChildren)
            {
                SaveObjectAndChildren(writer, allObjects, child, saver);
            }

            saver.EndSave(writer, e);
        }
    }

    private class ObjectSaver : ElementSaverBase
    {
        private readonly Dictionary<ObjectType, IObjectSaver> _savers = new();

        public ObjectSaver()
        {
            AddIgnoreField("type");
            AddIgnoreField("parent");
        }

        protected override void Initialise()
        {
            // Use Reflection to create instances of all IObjectSavers
            foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                         typeof(IObjectSaver)))
            {
                AddSaver((IObjectSaver)Activator.CreateInstance(t)!);
            }
        }

        private void AddSaver(IObjectSaver saver)
        {
            saver.ObjectSaver = this;
            _savers.Add(saver.AppliesTo, saver);
        }

        public override ElementType AppliesTo => ElementType.Object;

        public override void Save(GameXmlWriter writer, Element e)
        {
            if (!GetSaver(e, out var saver))
            {
                return;
            }

            saver.StartSave(writer, e);
            saver.EndSave(writer, e);
        }

        private bool GetSaver(Element e, out IObjectSaver saver)
        {
            if (!_savers.TryGetValue(e.Type, out saver!))
            {
                throw new Exception("ERROR: No ObjectSaver for type " + e.Type.ToString());
            }
            return true;
        }

        public void StartSave(GameXmlWriter writer, Element e)
        {
            if (GetSaver(e, out var saver))
            {
                saver.StartSave(writer, e);
            }
        }

        public void EndSave(GameXmlWriter writer, Element e)
        {
            if (GetSaver(e, out var saver))
            {
                saver.EndSave(writer, e);
            }
        }

        protected override bool CanSaveAttribute(string attribute, Element e)
        {
            return base.CanSaveAttribute(attribute, e) && _savers[e.Type].CanSaveAttribute(attribute);
        }

        protected override bool CanSaveTypeName(GameXmlWriter writer, string type, Element e)
        {
            if (!base.CanSaveTypeName(writer, type, e)) return false;
            return e.Type != ObjectType.Command || !e.Fields[FieldDefinitions.IsVerb] || type != "defaultverb";
        }

        private interface IObjectSaver
        {
            ObjectType AppliesTo { get; }
            void StartSave(GameXmlWriter writer, Element e);
            void EndSave(GameXmlWriter writer, Element e);
            ObjectSaver ObjectSaver { set; }
            bool CanSaveAttribute(string attribute);
        }

        private abstract class ObjectSaverBase : IObjectSaver
        {
            private readonly List<string> _ignoreAttributes = [];

            protected void AddIgnoreField(string field)
            {
                _ignoreAttributes.Add(field);
            }

            public abstract ObjectType AppliesTo { get; }

            public abstract void StartSave(GameXmlWriter writer, Element e);
            public abstract void EndSave(GameXmlWriter writer, Element e);

            public required ObjectSaver ObjectSaver
            {
                get;
                set;
            }

            public bool CanSaveAttribute(string attribute)
            {
                return !_ignoreAttributes.Contains(attribute);
            }
        }

        private class ObjectElementSaver : ObjectSaverBase
        {
            public override ObjectType AppliesTo => ObjectType.Object;

            public override void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("object");
                writer.WriteAttributeString("name", e.Name);
                ObjectSaver.SaveFields(writer, e);
            }

            public override void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }

        private class GameElementSaver : ObjectSaverBase
        {
            public GameElementSaver()
            {
                AddIgnoreField("gamename");
            }

            public override ObjectType AppliesTo => ObjectType.Game;

            public override void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("game");
                writer.WriteAttributeString("name", e.Fields.GetString("gamename"));
                ObjectSaver.SaveFields(writer, e);
            }

            public override void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }

        private class CommandSaver : ObjectSaverBase
        {
            public CommandSaver()
            {
                AddIgnoreField("isverb");
                AddIgnoreField("anonymous");
            }

            public override ObjectType AppliesTo => ObjectType.Command;

            public override void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement(e.Fields[FieldDefinitions.IsVerb] ? "verb" : "command");
                if (writer.Mode == SaveMode.SavedGame || !e.Fields[FieldDefinitions.Anonymous])
                {
                    writer.WriteAttributeString("name", e.Name);
                }
                ObjectSaver.SaveFields(writer, e);
            }

            public override void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }

        private class ExitElementSaver : ObjectSaverBase
        {
            public ExitElementSaver()
            {
                AddIgnoreField("to");
                AddIgnoreField("alias");
                AddIgnoreField("anonymous");
            }

            public override ObjectType AppliesTo => ObjectType.Exit;

            public override void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("exit");
                if (writer.Mode == SaveMode.SavedGame || !e.Fields[FieldDefinitions.Anonymous])
                {
                    writer.WriteAttributeString("name", e.Name);
                }
                if (!string.IsNullOrEmpty(e.Fields[FieldDefinitions.Alias]))
                {
                    writer.WriteAttributeString("alias", e.Fields[FieldDefinitions.Alias]);
                }
                if (e.Fields[FieldDefinitions.To] != null)
                {
                    writer.WriteAttributeString("to", e.Fields[FieldDefinitions.To].Name);
                }
                ObjectSaver.SaveFields(writer, e);
            }

            public override void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }

        private class TurnScriptElementSaver : ObjectSaverBase
        {
            public TurnScriptElementSaver()
            {
                AddIgnoreField("anonymous");
            }

            public override ObjectType AppliesTo => ObjectType.TurnScript;

            public override void StartSave(GameXmlWriter writer, Element e)
            {
                writer.WriteStartElement("turnscript");
                if (writer.Mode == SaveMode.SavedGame || !e.Fields[FieldDefinitions.Anonymous])
                {
                    writer.WriteAttributeString("name", e.Name);
                }
                ObjectSaver.SaveFields(writer, e);
            }

            public override void EndSave(GameXmlWriter writer, Element e)
            {
                writer.WriteEndElement();
            }
        }
    }
}