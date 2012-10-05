using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    // This class is used to create an IEditableDictionary wrapper where the values in the dictionary are
    // themselves wrapped.
    //      For example: TSource=IScript, TWrapped=IEditableScripts
    //      Then we can expose an IEditableDictionary<IEditableScripts> which wraps a QuestDictionary<IScript>
    public class EditableWrappedItemDictionary<TSource, TWrapped> : IEditableDictionary<TWrapped>, IDataWrapper
        where TWrapped : IDataWrapper
        where TSource : class
    {
        public event EventHandler<EditableListUpdatedEventArgs<TWrapped>> Added;
        public event EventHandler<EditableListUpdatedEventArgs<TWrapped>> Removed;

        // currently unused
        public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
        {
            add { }
            remove { }
        }

        public event EventHandler<EditableListUpdatedEventArgs<TWrapped>> Updated;

        #region Static DataWrapper
        private static EditableDataWrapper<QuestDictionary<TSource>, EditableWrappedItemDictionary<TSource, TWrapped>> s_wrapper;

        static EditableWrappedItemDictionary()
        {
            s_wrapper = new EditableDataWrapper<QuestDictionary<TSource>, EditableWrappedItemDictionary<TSource, TWrapped>>(GetNewInstance);
        }

        public static EditableWrappedItemDictionary<TSource, TWrapped> GetInstance(EditorController controller, QuestDictionary<TSource> list)
        {
            return s_wrapper.GetInstance(controller, list);
        }

        private static EditableWrappedItemDictionary<TSource, TWrapped> GetNewInstance(EditorController controller, QuestDictionary<TSource> list)
        {
            return new EditableWrappedItemDictionary<TSource, TWrapped>(controller, list);
        }

        public static void Clear()
        {
            s_wrapper.Clear();
        }
        #endregion

        private QuestDictionary<TSource> m_source;
        private Dictionary<string, IEditableListItem<TWrapped>> m_wrappedItems = new Dictionary<string, IEditableListItem<TWrapped>>();
        private Dictionary<TWrapped, IEditableListItem<TWrapped>> m_wrappedItemsLookup = new Dictionary<TWrapped, IEditableListItem<TWrapped>>();
        private EditorController m_controller;
        private readonly string m_id;

        private static int s_count = 0;

        public EditableWrappedItemDictionary(EditorController controller, QuestDictionary<TSource> source)
        {
            s_count++;
            m_id = "wrapdictionary" + s_count;

            m_controller = controller;
            m_source = source;
            m_source.Added += m_source_Added;
            m_source.Removed += m_source_Removed;
            PopulateWrappedItems();
        }

        private void PopulateWrappedItems()
        {
            m_wrappedItems.Clear();
            int index = 0;

            foreach (var item in m_source)
            {
                AddWrappedItem(item.Key, WrapValue(item.Value), EditorUpdateSource.System, index);
                index++;
            }
        }

        private TWrapped WrapValue(TSource source)
        {
            return (TWrapped)m_controller.WrapValue((TSource)source);
        }

        private TSource UnwrapValue(TWrapped wrapped)
        {
            if (wrapped == null) return null;
            return (TSource)wrapped.GetUnderlyingValue();
        }

        public object GetUnderlyingValue()
        {
            return m_source;
        }

        public IDictionary<string, IEditableListItem<TWrapped>> Items
        {
            get { return m_wrappedItems; }
        }

        private void AddWrappedItem(string key, TWrapped value, EditorUpdateSource source, int index)
        {
            IEditableListItem<TWrapped> wrappedValue = new EditableListItem<TWrapped>(key, value);
            m_wrappedItems.Add(key, wrappedValue);
            m_wrappedItemsLookup.Add(value, wrappedValue);
            value.UnderlyingValueUpdated += WrappedUnderlyingValueUpdated;

            if (Added != null) Added(this, new EditableListUpdatedEventArgs<TWrapped> { UpdatedItem = wrappedValue, Index = index, Source = source });
        }

        private void RemoveWrappedItem(IEditableListItem<TWrapped> item, EditorUpdateSource source, int index)
        {
            m_wrappedItems[item.Key].Value.UnderlyingValueUpdated -= WrappedUnderlyingValueUpdated;
            m_wrappedItemsLookup.Remove(m_wrappedItems[item.Key].Value);
            m_wrappedItems.Remove(item.Key);
            if (Removed != null) Removed(this, new EditableListUpdatedEventArgs<TWrapped> { UpdatedItem = item, Index = index, Source = source });
        }

        void WrappedUnderlyingValueUpdated(object sender, DataWrapperUpdatedEventArgs e)
        {
            if (Updated != null)
            {
                // sender will be the underlying wrapped value that has been updated. e.g. an IEditableScripts item
                TWrapped updatedItem = (TWrapped)sender;

                Updated(this, new EditableListUpdatedEventArgs<TWrapped>
                {
                    UpdatedItem = m_wrappedItemsLookup[updatedItem],
                    Index = m_source.IndexOfKey(m_wrappedItemsLookup[updatedItem].Key)
                });
            }
        }

        void m_source_Added(object sender, QuestDictionaryUpdatedEventArgs<TSource> e)
        {
            AddWrappedItem(e.Key, WrapValue(e.Item), (EditorUpdateSource)e.Source, e.Index);
        }

        void m_source_Removed(object sender, QuestDictionaryUpdatedEventArgs<TSource> e)
        {
            RemoveWrappedItem(m_wrappedItems[e.Key], (EditorUpdateSource)e.Source, e.Index);
        }

        public IEnumerable<KeyValuePair<string, string>> DisplayItems
        {
            get
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                foreach (var item in m_wrappedItems)
                {
                    result.Add(item.Key, item.Value.Value.DisplayString());
                }

                return result;
            }
        }

        // TO DO: Public methods shouldn't be starting/ending transactions here - that should be up to the caller

        public void Add(string key, TWrapped value)
        {
            string undoEntry = null;
            undoEntry = string.Format("Add '{0}={1}'", key, value == null ? string.Empty : value.DisplayString());

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
            m_source.Add(key, UnwrapValue(value), UpdateSource.User);
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        public void Remove(params string[] keys)
        {
            string undoEntry = null;
            undoEntry = string.Format("Remove '{0}'", string.Join(",", keys));

            m_controller.WorldModel.UndoLogger.StartTransaction(undoEntry);
            foreach (string key in keys)
            {
                m_source.Remove(key, UpdateSource.User);
            }
            m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        public ValidationResult CanAdd(string key)
        {
            if (m_source.ContainsKey(key))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.ItemAlreadyExists };
            }
            return new ValidationResult { Valid = true };
        }

        public TWrapped this[string key]
        {
            get
            {
                return WrapValue(m_source[key]);
            }
        }

        public void Update(string key, TWrapped value)
        {
            int index = m_source.IndexOfKey(key);
            m_source.Remove(key, UpdateSource.User);
            m_source.Add(key, UnwrapValue(value), UpdateSource.User, index);
        }

        // it is up to the caller of this method to start/end a transaction (this should also be the case eventually
        // for the other public methods)

        public void ChangeKey(string oldKey, string newKey)
        {
            //m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Update key '{0}' to '{1}'", oldKey, newKey));
            int index = m_source.IndexOfKey(oldKey);
            TSource value = m_source[oldKey];
            m_source.Remove(oldKey, UpdateSource.User);
            m_source.Add(newKey, value, UpdateSource.User, index);
            //m_controller.WorldModel.UndoLogger.EndTransaction();
        }

        public string DisplayString()
        {
            return string.Format("(Script Dictionary: {0} items)", m_source.Count);
        }

        public bool Locked
        {
            get { return m_source.Locked; }
        }

        public IEditableDictionary<TWrapped> Clone(string parent, string attribute)
        {
            // TO DO: If this is required, then for example where TWrapped=IScript we will need to clone the scripts too
            throw new NotImplementedException();

            //IEditableDictionary<TWrapped> result;
            //m_controller.WorldModel.UndoLogger.StartTransaction(string.Format("Copy '{0}' {1}", parent, attribute));
            //result = CloneInternal(m_controller.WorldModel.Elements.Get(parent), attribute);
            //m_controller.WorldModel.UndoLogger.EndTransaction();
            //return result;
        }

        //private IEditableDictionary<TWrapped> CloneInternal(Element parent, string attribute)
        //{
        //    QuestDictionary<TSource> newSource = (QuestDictionary<TSource>)m_source.Clone();
        //    newSource.Locked = false;
        //    parent.Fields.Set(attribute, newSource);
        //    newSource = (QuestDictionary<TSource>)parent.Fields.Get(attribute);
        //    return EditableWrappedItemDictionary<TSource, TWrapped>.GetNewInstance(m_controller, newSource);
        //}

        public string Owner
        {
            get
            {
                if (m_source.Owner == null) return null;
                return m_source.Owner.Name;
            }
        }

        public string Id { get { return m_id; } }
    }
}
