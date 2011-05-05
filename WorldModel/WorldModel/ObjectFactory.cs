using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IElementFactory
    {
        ElementType CreateElementType { get; }
        Element Create(string name);
        Element Create();
        WorldModel WorldModel { set; }
    }

    public abstract class ElementFactoryBase : IElementFactory
    {
        public abstract ElementType CreateElementType { get; }

        public virtual Element Create(string name)
        {
            Element newElement = new Element(WorldModel);
            
            try
            {
                WorldModel.Elements.Add(CreateElementType, name, newElement);
            }
            catch (ArgumentException e)
            {
                throw new Exception(string.Format("Cannot add {0} '{1}': {2}", FriendlyElementTypeName, name, e.Message), e);
            }

            newElement.Name = name;
            newElement.ElemType = CreateElementType;
            return newElement;
        }

        public virtual Element Create()
        {
            string id = WorldModel.GetUniqueID();
            return Create(id);
        }

        protected string FriendlyElementTypeName
        {
            get
            {
                return ((ElementTypeInfo)(typeof(ElementType).GetField(CreateElementType.ToString()).GetCustomAttributes(typeof(ElementTypeInfo), false)[0])).Name;
            }
        }

        public WorldModel WorldModel { get; set; }
    }

    public class ObjectFactory : ElementFactoryBase
    {
        public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;

        public override ElementType CreateElementType { get { return ElementType.Object; } }

        public override Element Create(string name)
        {
            return CreateObject(name);
        }

        public Element CreateObject(string objectName)
        {
            return CreateObject(objectName, ObjectType.Object);
        }

        public Element CreateObject(string objectName, ObjectType type)
        {
            return CreateObject(objectName, type, true);
        }

        public Element CreateObject(ObjectType type)
        {
            return CreateObject(WorldModel.GetUniqueID(), type);
        }

        internal Element CreateObject(string objectName, ObjectType type, bool addToUndoLog)
        {
            WorldModel.UndoLogger.AddUndoAction(new CreateDestroyLogEntry(objectName, true, type));
            Element newObject = base.Create(objectName);

            newObject.Type = type;

            string defaultType = WorldModel.DefaultTypeNames[type];

            if (WorldModel.State == GameState.Running)
            {
                if (WorldModel.Elements.ContainsKey(ElementType.ObjectType, defaultType))
                {
                    newObject.AddType(WorldModel.GetObjectType(defaultType));
                }
            }
            else
            {
                newObject.Fields.LazyFields.AddDefaultType(defaultType);
            }

            if (ObjectsUpdated != null) ObjectsUpdated(this, new ObjectsUpdatedEventArgs { Added = objectName });

            return newObject;
        }

        public Element CreateObject(string objectName, Element parent)
        {
            Element newObject = CreateObject(objectName);
            newObject.Parent = parent;
            return newObject;
        }

        public void DestroyObject(string objectName)
        {
            DestroyObject(objectName, false);
        }

        internal void DestroyObjectSilent(string objectName)
        {
            DestroyObject(objectName, true);
        }

        private void DestroyObject(string objectName, bool silent)
        {
            try
            {
                Element destroy = WorldModel.Object(objectName);
                if (!silent) AddDestroyToUndoLog(destroy, destroy.Type);
                if (ObjectsUpdated != null) ObjectsUpdated(this, new ObjectsUpdatedEventArgs { Removed = objectName });
                WorldModel.RemoveObject(objectName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot destroy object '{0}': {1}", objectName, e.Message), e);
            }
        }

        public Element CreateCommand()
        {
            string id = WorldModel.GetUniqueID();
            return CreateCommand(id);
        }

        public Element CreateCommand(string id)
        {
            Element newCommand = CreateObject(id, ObjectType.Command);
            newCommand.Type = ObjectType.Command;
            return newCommand;
        }

        public Element CreateExit(string exitName, Element fromRoom, Element toRoom)
        {
            string exitID = fromRoom.Name + "." + exitName;
            if (WorldModel.ObjectExists(exitID)) exitID = WorldModel.GetUniqueID(exitID);
            Element newExit = CreateExit(exitID, exitName, fromRoom, toRoom);
            newExit.Fields[FieldDefinitions.Anonymous] = true;
            return newExit;
        }

        public Element CreateExit(string exitID, string exitName, Element fromRoom, Element toRoom)
        {
            Element newExit = CreateObject(exitID, ObjectType.Exit);
            newExit.Fields[FieldDefinitions.Alias] = exitName;
            newExit.Parent = fromRoom;
            newExit.Fields[FieldDefinitions.To] = toRoom;
            newExit.Type = ObjectType.Exit;
            return newExit;
        }

        public Element CreateExitLazy(string exitName, Element fromRoom, string toRoom)
        {
            Element newExit = CreateExit(exitName, fromRoom, null);
            InitLazyExit(newExit, toRoom);
            return newExit;
        }

        public Element CreateExitLazy(string exitID, string exitName, Element fromRoom, string toRoom)
        {
            Element newExit = CreateExit(exitID, exitName, fromRoom, null);
            InitLazyExit(newExit, toRoom);
            return newExit;
        }

        private void InitLazyExit(Element exit, string toRoom)
        {
            if (toRoom != null)
            {
                exit.Fields.LazyFields.AddObjectField("to", toRoom);
            }
        }

        private void AddDestroyToUndoLog(Element appliesTo, ObjectType type)
        {
            Fields fields = appliesTo.Fields;

            Dictionary<string, object> allAttributes = fields.GetAllAttributes();

            foreach (string attr in allAttributes.Keys)
            {
                WorldModel.UndoLogger.AddUndoAction(new UndoFieldSet(WorldModel, appliesTo.Name, attr, allAttributes[attr], null, false));
            }

            WorldModel.UndoLogger.AddUndoAction(new CreateDestroyLogEntry(appliesTo.Name, false, type));
        }

        private class CreateDestroyLogEntry : AxeSoftware.Quest.UndoLogger.IUndoAction
        {
            private bool m_create;
            private ObjectType m_type;
            private string m_name;

            public CreateDestroyLogEntry(string name)
                : this(name, true, ObjectType.Object)
            {
            }

            public CreateDestroyLogEntry(string name, bool create, ObjectType type)
            {
                m_create = create;
                m_name = name;
                m_type = type;
            }

            public void DoUndo(WorldModel worldModel)
            {
                if (m_create)
                {
                    worldModel.ObjectFactory.DestroyObjectSilent(m_name);
                }
                else
                {
                    worldModel.ObjectFactory.CreateObject(m_name, m_type, false);
                }
            }

            public void DoRedo(WorldModel worldModel)
            {
                if (m_create)
                {
                    worldModel.ObjectFactory.CreateObject(m_name, m_type, false);
                }
                else
                {
                    worldModel.ObjectFactory.DestroyObjectSilent(m_name);
                }
            }
        }
    }

    internal class ObjectTypeFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.ObjectType; }
        }

        public override Element Create(string name)
        {
            Element newType = base.Create(name);
            newType.Fields.MutableFieldsLocked = true;
            return newType;
        }
    }

    internal class EditorFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Editor; }
        }
    }

    internal class EditorTabFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.EditorTab; }
        }
    }

    internal class EditorControlFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.EditorControl; }
        }
    }

    internal class FunctionFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Function; }
        }
    }

    internal class DelegateFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Delegate; }
        }
    }

    internal class TemplateFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Template; }
        }
    }

    internal class DynamicTemplateFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.DynamicTemplate; }
        }
    }

    internal abstract class SingleElementFactory : ElementFactoryBase
    {
        public override Element Create(string name)
        {
            if (WorldModel.Elements.Count(CreateElementType) > 1)
            {
                throw new InvalidOperationException(string.Format("There can only be one '{0}' element", FriendlyElementTypeName));
            }
            return base.Create(name);
        }
    }

    internal class WalkthroughFactory : SingleElementFactory
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Walkthrough; }
        }
    }

    internal class IncludedLibraryFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.IncludedLibrary; }
        }
    }

    internal class ImpliedTypeFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.ImpliedType; }
        }
    }

    internal class JavascriptReferenceFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Javascript; }
        }
    }
}
