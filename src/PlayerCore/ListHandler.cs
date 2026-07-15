#nullable enable
using QuestViva.Common;

namespace QuestViva.PlayerCore;

public class ListHandler(Action<string, object?[]?> addJavaScriptToBuffer)
{
    private readonly ListDataComparer _comparer = new();
    private readonly Dictionary<ListType, List<ListData>> _lists = new();
    private Action<string, object?[]?> AddJavaScriptToBuffer { get; } = addJavaScriptToBuffer;

    public void UpdateList(ListType listType, List<ListData> items)
    {
        // ElementMenuVerbs isn't cached/deduplicated like the other lists: it exists to sync
        // data-verbs onto whichever inline {object:} links currently exist in the DOM, and new
        // links can appear (e.g. after a "wait" block reveals text) even when the underlying
        // verb data hasn't changed since the last push - a no-op push would then wrongly get
        // suppressed and the newly-added link would never receive its verbs.
        if (listType == ListType.ElementMenuVerbs)
        {
            _lists[listType] = items;
            SendUpdatedList(listType);
            return;
        }

        bool listChanged;
        if (!_lists.TryGetValue(listType, out var list))
        {
            listChanged = true;
        }
        else
        {
            listChanged = !list.SequenceEqual(items, _comparer);
        }

        if (!listChanged)
        {
            return;
        }

        _lists[listType] = items;
        SendUpdatedList(listType);
    }

    private void SendUpdatedList(ListType listType)
    {
        if (listType == ListType.ExitsList)
        {
            SendCompassList(_lists[ListType.ExitsList]);
            return;
        }

        if (listType == ListType.ElementMenuVerbs)
        {
            SendElementMenuVerbs(_lists[ListType.ElementMenuVerbs]);
            return;
        }

        var listName = listType switch
        {
            ListType.InventoryList => "inventory",
            ListType.ObjectsList => "placesobjects",
            _ => null
        };

        if (listName != null)
        {
            AddJavaScriptToBuffer("updateList", [
                listName, PlayerHelper.ListDataParameter(_lists[listType])
            ]);
        }
    }

    private void SendCompassList(List<ListData> list)
    {
        var data = string.Join("/", list.Select(l => l.Text));
        AddJavaScriptToBuffer("updateCompass", [data]);
    }

    private void SendElementMenuVerbs(List<ListData> list)
    {
        var data = new Dictionary<string, string>();
        foreach (var item in list)
        {
            if (item.ElementId != null)
            {
                data[item.ElementId] = PlayerHelper.VerbString(item.Verbs);
            }
        }

        AddJavaScriptToBuffer("updateObjectLinks", [data]);
    }

    private class ListDataComparer : IEqualityComparer<ListData>
    {
        public bool Equals(ListData? x, ListData? y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (x.ElementId != y.ElementId)
            {
                return false;
            }

            if (x.ElementName != y.ElementName)
            {
                return false;
            }

            if (x.Text != y.Text)
            {
                return false;
            }

            return PlayerHelper.VerbString(x.Verbs) == PlayerHelper.VerbString(y.Verbs);
        }

        public int GetHashCode(ListData obj)
        {
            return obj.GetHashCode();
        }
    }
}