using Microsoft.VisualBasic;

namespace QuestViva.Legacy
{
    internal class ChangeLog
    {
        // NOTE: We only actually use the Object change log at the moment, as that is the only
        // one that has properties and actions.

        public enum AppliesTo
        {
            Object,
            Room
        }

        public AppliesTo AppliesToType;
        public Dictionary<string, string> Changes = new Dictionary<string, string>();

        // appliesTo = room or object name
        // element = the thing that's changed, e.g. an action or property name
        // changeData = the actual change info
        public void AddItem(ref string appliesTo, ref string element, ref string changeData)
        {
            // the first four characters of the changeData will be "prop" or "acti", so we add this to the
            // key so that actions and properties don't collide.

            string key = appliesTo + "#" + Strings.Left(changeData, 4) + "~" + element;
            if (Changes.ContainsKey(key))
            {
                Changes.Remove(key);
            }

            Changes.Add(key, changeData);
        }
    }
}