using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class EditableDataWrapper<TSource, TWrapped>
    {
        private Dictionary<TSource, TWrapped> s_instances = new Dictionary<TSource, TWrapped>();
        private Func<EditorController, TSource, TWrapped> GetNewWrappedInstance;

        public EditableDataWrapper(Func<EditorController, TSource, TWrapped> instanceCreator)
        {
            GetNewWrappedInstance = instanceCreator;
        }

        public TWrapped GetInstance(EditorController controller, TSource source)
        {
            TWrapped instance;
            if (s_instances.TryGetValue(source, out instance))
            {
                return instance;
            }

            instance = GetNewWrappedInstance(controller, source);
            s_instances.Add(source, instance);
            return instance;
        }

        internal void Clear()
        {
            s_instances.Clear();
        }
    }
}
