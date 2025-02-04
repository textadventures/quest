using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TextAdventures.Quest
{
    internal partial class GameSaver
    {
        private class ObjectsSaver : IElementsSaver
        {
            #region IElementsSaver Members

            public ElementType AppliesTo
            {
                get { return ElementType.Object; }
            }

            public void Save(GameXmlWriter writer, WorldModel worldModel)
            {
                ObjectSaver elementSaver = new ObjectSaver();
                elementSaver.GameSaver = GameSaver;

                IEnumerable<Element> allObjects = worldModel.Elements.GetElements(ElementType.Object).OrderBy(o => o.MetaFields[MetaFieldDefinitions.SortIndex]);

                foreach (Element e in allObjects.Where(e => e.Parent == null && GameSaver.CanSave(e)))
                {
                    SaveObjectAndChildren(writer, allObjects, e, elementSaver);
                }
            }

            public GameSaver GameSaver { get; set; }

            #endregion

            private void SaveObjectAndChildren(GameXmlWriter writer, IEnumerable<Element> allObjects, Element e, ObjectSaver saver)
            {
                saver.StartSave(writer, e);
                IEnumerable<Element> orderedChildren = from child in allObjects
                                                       where child.Parent == e
                                                       orderby child.MetaFields[MetaFieldDefinitions.SortIndex]
                                                       select child;

                foreach (Element child in orderedChildren)
                {
                    SaveObjectAndChildren(writer, allObjects, child, saver);
                }

                saver.EndSave(writer, e);
            }
        }

        private class ObjectSaver : ElementSaverBase
        {
            private Dictionary<ObjectType, IObjectSaver> m_savers = new Dictionary<ObjectType, IObjectSaver>();

            public ObjectSaver()
            {
                AddIgnoreField("type");
                AddIgnoreField("parent");
            }

            protected override void Initialise()
            {
                // Use Reflection to create instances of all IObjectSavers
                foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                    typeof(IObjectSaver)))
                {
                    AddSaver((IObjectSaver)Activator.CreateInstance(t));
                }
            }

            private void AddSaver(IObjectSaver saver)
            {
                saver.ObjectSaver = this;
                m_savers.Add(saver.AppliesTo, saver);
            }

            #region IElementSaver Members

            public override ElementType AppliesTo
            {
                get { return ElementType.Object; }
            }

            public override void Save(GameXmlWriter writer, Element e)
            {
                IObjectSaver saver;
                if (GetSaver(writer, e, out saver))
                {
                    saver.StartSave(writer, e);
                    saver.EndSave(writer, e);
                }
            }

            #endregion

            private bool GetSaver(GameXmlWriter writer, Element e, out IObjectSaver saver)
            {
                if (!m_savers.TryGetValue(e.Type, out saver))
                {
                    throw new Exception("ERROR: No ObjectSaver for type " + e.Type.ToString());
                }
                return true;
            }

            public void StartSave(GameXmlWriter writer, Element e)
            {
                IObjectSaver saver;
                if (GetSaver(writer, e, out saver))
                {
                    saver.StartSave(writer, e);
                }
            }

            public void EndSave(GameXmlWriter writer, Element e)
            {
                IObjectSaver saver;
                if (GetSaver(writer, e, out saver))
                {
                    saver.EndSave(writer, e);
                }
            }

            protected override bool CanSaveAttribute(string attribute, Element e)
            {
                if (!base.CanSaveAttribute(attribute, e)) return false;
                return m_savers[e.Type].CanSaveAttribute(attribute);
            }

            protected override bool CanSaveTypeName(GameXmlWriter writer, string type, Element e)
            {
                if (!base.CanSaveTypeName(writer, type, e)) return false;
                if (e.Type == ObjectType.Command && e.Fields[FieldDefinitions.IsVerb] && type == "defaultverb") return false;
                return true;
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
                private List<string> m_ignoreAttributes = new List<string>();

                protected void AddIgnoreField(string field)
                {
                    m_ignoreAttributes.Add(field);
                }

                #region IObjectSaver Members

                public abstract ObjectType AppliesTo { get; }

                public abstract void StartSave(GameXmlWriter writer, Element e);
                public abstract void EndSave(GameXmlWriter writer, Element e);

                public ObjectSaver ObjectSaver
                {
                    get;
                    set;
                }

                public bool CanSaveAttribute(string attribute)
                {
                    if (m_ignoreAttributes.Contains(attribute)) return false;
                    return true;
                }

                #endregion
            }

            private class ObjectElementSaver : ObjectSaverBase
            {
                #region IObjectSaver Members

                public override ObjectType AppliesTo
                {
                    get { return ObjectType.Object; }
                }

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

                #endregion
            }

            private class GameElementSaver : ObjectSaverBase
            {
                public GameElementSaver()
                {
                    AddIgnoreField("gamename");
                }

                #region IObjectSaver Members

                public override ObjectType AppliesTo
                {
                    get { return ObjectType.Game; }
                }

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

                #endregion
            }

            private class CommandSaver : ObjectSaverBase
            {
                public CommandSaver()
                {
                    AddIgnoreField("isverb");
                    AddIgnoreField("anonymous");
                }

                #region IObjectSaver Members

                public override ObjectType AppliesTo
                {
                    get { return ObjectType.Command; }
                }

                public override void StartSave(GameXmlWriter writer, Element e)
                {
                    if (e.Fields[FieldDefinitions.IsVerb])
                    {
                        writer.WriteStartElement("verb");
                    }
                    else
                    {
                        writer.WriteStartElement("command");
                    }
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

                #endregion
            }

            private class ExitElementSaver : ObjectSaverBase
            {
                public ExitElementSaver()
                {
                    AddIgnoreField("to");
                    AddIgnoreField("alias");
                    AddIgnoreField("anonymous");
                }

                public override ObjectType AppliesTo
                {
                    get { return ObjectType.Exit; }
                }

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

                public override ObjectType AppliesTo
                {
                    get { return ObjectType.TurnScript; }
                }

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
}
