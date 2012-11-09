using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public interface IElementFactory
    {
        event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;
        ElementType CreateElementType { get; }
        Element Create(string name, bool addToUndoLog);
        Element Create(string name);
        Element Create();
        void DestroyElement(string elementName);
        void DestroyElementSilent(string elementName);
        Element CloneElement(Element elementToClone, string newElementName);
        WorldModel WorldModel { set; }
    }

    public abstract class ElementFactoryBase : IElementFactory
    {
        public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;

        public abstract ElementType CreateElementType { get; }

        public virtual Element Create(string name)
        {
            return Create(name, true);
        }

        public virtual Element Create(string name, bool addToUndoLog)
        {
            return CreateInternal(name, addToUndoLog, null);
        }

        protected Element CreateInternal(string name, bool addToUndoLog, Action<Element> extraInitialisation, IList<string> initialTypes = null, IDictionary<string, object> initialFields = null, Element elementToClone = null)
        {
            Element newElement;

            if (elementToClone == null)
            {
                newElement = new Element(WorldModel);
            }
            else
            {
                newElement = new Element(WorldModel, elementToClone);
            }

            if (addToUndoLog)
            {
                WorldModel.UndoLogger.AddUndoAction(new CreateDestroyLogEntry(name, CreateElementType, newElement, true, NotifyAddedElement, NotifyRemovedElement));
            }

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

            if (elementToClone != null && CreateElementType == ElementType.Object)
            {
                newElement.Type = elementToClone.Type;
            }

            if (extraInitialisation != null)
            {
                extraInitialisation.Invoke(newElement);
            }

            if (initialTypes != null)
            {
                foreach (string type in initialTypes)
                {
                    newElement.Fields.AddTypeUndoable(WorldModel.Elements.Get(ElementType.ObjectType, type));
                }
            }

            if (initialFields != null)
            {
                foreach (var field in initialFields)
                {
                    newElement.Fields.Set(field.Key, field.Value);
                }
            }

            newElement.FinishedInitialisation();

            WorldModel.UpdateElementSortOrder(newElement);

            NotifyAddedElement(name);

            return newElement;
        }

        public virtual Element Create()
        {
            string id = WorldModel.GetUniqueID();
            return Create(id);
        }

        public Element CloneElement(Element elementToClone, string newElementName)
        {
            return CreateInternal(newElementName, true, null, elementToClone: elementToClone);
        }

        protected string FriendlyElementTypeName
        {
            get
            {
                return ((ElementTypeInfo)(typeof(ElementType).GetField(CreateElementType.ToString()).GetCustomAttributes(typeof(ElementTypeInfo), false)[0])).Name;
            }
        }

        public WorldModel WorldModel { get; set; }

        protected void NotifyAddedElement(string elementName)
        {
            if (ObjectsUpdated != null) ObjectsUpdated(this, new ObjectsUpdatedEventArgs { Added = elementName });
        }

        protected void NotifyRemovedElement(string elementName)
        {
            if (ObjectsUpdated != null) ObjectsUpdated(this, new ObjectsUpdatedEventArgs { Removed = elementName });
        }

        public void DestroyElement(string elementName)
        {
            DestroyElement(elementName, false);
        }

        public void DestroyElementSilent(string elementName)
        {
            DestroyElement(elementName, true);
        }

        private void DestroyElement(string elementName, bool silent)
        {
            try
            {
                Element destroy = WorldModel.Elements.Get(elementName);

                // get all child elements and delete them too, so they'll be correctly recreated
                // by an undo. We call .ToList() so we get the full list before iterating through it,
                // as the list will change as we destroy child elements.
                IEnumerable<Element> childElements = WorldModel.Elements.GetElements().Where(e => e.Parent == destroy).ToList();

                foreach (Element child in childElements)
                {
                    DestroyElement(child.Name, silent);
                }

                RemoveReferences(destroy);

                if (!silent) AddDestroyToUndoLog(destroy, destroy.Type);
                WorldModel.RemoveElement(CreateElementType, elementName);
                NotifyRemovedElement(elementName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot destroy element '{0}': {1}", elementName, e.Message), e);
            }
        }

        protected virtual void RemoveReferences(Element destroyedElement)
        {
            foreach (Element e in WorldModel.Elements.GetElements())
            {
                e.Fields.RemoveReferencesTo(destroyedElement);
            }
        }

        private void AddDestroyToUndoLog(Element appliesTo, ObjectType type)
        {
            WorldModel.UndoLogger.AddUndoAction(new CreateDestroyLogEntry(appliesTo.Name, appliesTo.ElemType, appliesTo, false, NotifyAddedElement, NotifyRemovedElement));
        }

        protected class CreateDestroyLogEntry : TextAdventures.Quest.UndoLogger.IUndoAction
        {
            private string m_name;
            private ElementType m_type;
            private bool m_create;
            private Element m_element;
            private Action<string> m_notifyAdded;
            private Action<string> m_notifyRemoved;

            public CreateDestroyLogEntry(string name, ElementType type, Element element, bool create, Action<string> notifyAdded, Action<string> notifyRemoved)
            {
                m_name = name;
                m_type = type;
                m_create = create;
                m_element = element;
                m_notifyAdded = notifyAdded;
                m_notifyRemoved = notifyRemoved;
            }

            private void CreateElement(WorldModel worldModel)
            {
                worldModel.Elements.Add(m_type, m_name, m_element);
                m_notifyAdded(m_name);
            }

            private void DestroyElement(WorldModel worldModel)
            {
                worldModel.Elements.Remove(m_type, m_name);
                m_notifyRemoved(m_name);
            }

            public void DoUndo(WorldModel worldModel)
            {
                if (m_create)
                {
                    DestroyElement(worldModel);
                }
                else
                {
                    CreateElement(worldModel);
                }
            }

            public void DoRedo(WorldModel worldModel)
            {
                if (m_create)
                {
                    CreateElement(worldModel);
                }
                else
                {
                    DestroyElement(worldModel);
                }
            }
        }
    }

    public class ObjectFactory : ElementFactoryBase
    {
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

        public Element CreateObject(ObjectType type, IList<string> initialTypes, IDictionary<string, object> initialFields)
        {
            string id = (type == ObjectType.Exit) ? WorldModel.GetUniqueID("exit") : WorldModel.GetUniqueID();
            return CreateObject(id, type, true, initialTypes, initialFields);
        }

        internal Element CreateObject(string objectName, ObjectType type, bool addToUndoLog, IList<string> initialTypes = null, IDictionary<string, object> initialFields = null)
        {
            string defaultType = WorldModel.DefaultTypeNames[type];

            if (WorldModel.State == GameState.Running)
            {
                if (WorldModel.Elements.ContainsKey(ElementType.ObjectType, defaultType))
                {
                    if (initialTypes == null) initialTypes = new List<string>();
                    initialTypes.Insert(0, defaultType);
                }
            }

            Element newObject = base.CreateInternal(objectName, true, newElement => newElement.Type = type, initialTypes, initialFields);

            if (WorldModel.State != GameState.Running)
            {
                newObject.Fields.LazyFields.AddDefaultType(defaultType);
            }

            return newObject;
        }

        public Element CreateObject(string objectName, Element parent)
        {
            Element newObject = CreateObject(objectName);
            newObject.Parent = parent;
            return newObject;
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

        public Element CreateTurnScript(string id, Element parent)
        {
            bool anonymous = false;
            if (string.IsNullOrEmpty(id))
            {
                anonymous = true;
                id = WorldModel.GetUniqueID();
            }
            Element newTurnScript = CreateObject(id, ObjectType.TurnScript);
            newTurnScript.Type = ObjectType.TurnScript;
            newTurnScript.Parent = parent;
            if (anonymous)
            {
                newTurnScript.Fields[FieldDefinitions.Anonymous] = true;
            }
            return newTurnScript;
        }

        public Element CreateExit(string exitName, Element fromRoom, Element toRoom, string initialType)
        {
            return CreateExit(null, exitName, fromRoom, toRoom, initialType);
        }

        public Element CreateExit(string exitID, string exitName, Element fromRoom, Element toRoom, string initialType)
        {
            bool anonymous = false;
            if (string.IsNullOrEmpty(exitID))
            {
                exitID = WorldModel.GetUniqueID("exit");
                if (WorldModel.ObjectExists(exitID)) exitID = WorldModel.GetUniqueID(exitID);
                anonymous = true;
            }
            if (string.IsNullOrEmpty(initialType)) initialType = null;
            Element newExit = CreateObject(exitID, ObjectType.Exit, true, initialType == null ? null : new List<string> { initialType });
            newExit.Fields[FieldDefinitions.Alias] = exitName;
            newExit.Parent = fromRoom;
            newExit.Fields[FieldDefinitions.To] = toRoom;
            newExit.Type = ObjectType.Exit;
            if (anonymous)
            {
                newExit.Fields[FieldDefinitions.Anonymous] = true;
            }
            return newExit;
        }

        public Element CreateExitLazy(string exitName, Element fromRoom, string toRoom)
        {
            Element newExit = CreateExit(exitName, fromRoom, null, null);
            InitLazyExit(newExit, toRoom);
            return newExit;
        }

        public Element CreateExitLazy(string exitID, string exitName, Element fromRoom, string toRoom)
        {
            Element newExit = CreateExit(exitID, exitName, fromRoom, null, null);
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
            if (!WorldModel.EditMode)
            {
                newType.Fields.MutableFieldsLocked = true;
            }
            return newType;
        }

        protected override void RemoveReferences(Element destroyedElement)
        {
            // When deleting an object type, we must also remove references to the deleted type
            // from other elements.

            foreach (Element e in WorldModel.Elements.GetElements())
            {
                if (e.Fields.InheritsType(destroyedElement))
                {
                    e.Fields.RemoveTypeUndoable(destroyedElement);
                }
            }

            base.RemoveReferences(destroyedElement);
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
            if (WorldModel.Elements.Count(CreateElementType) >= 1)
            {
                throw new InvalidOperationException(string.Format("There can only be one '{0}' element", FriendlyElementTypeName));
            }
            return base.Create(name);
        }
    }

    internal class WalkthroughFactory : ElementFactoryBase
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

    internal class TimerFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Timer; }
        }
    }

    internal class ResourceFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Resource; }
        }
    }

    internal class OutputFactory : ElementFactoryBase
    {
        public override ElementType CreateElementType
        {
            get { return ElementType.Output; }
        }
    }
}
