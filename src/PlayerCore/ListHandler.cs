#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using QuestViva.Common;
using TextAdventures.Quest;

namespace QuestViva.PlayerCore;

public class ListHandler(Action<string, object?[]?> addJavaScriptToBuffer)
{
    private Action<string, object?[]?> AddJavaScriptToBuffer { get; } = addJavaScriptToBuffer;
    private readonly Dictionary<ListType, List<ListData>> _lists = new();
    private readonly ListDataComparer _comparer = new();

    private class ListDataComparer : IEqualityComparer<ListData>
    {
        public bool Equals(ListData? x, ListData? y)
        {
            if (x == null || y == null) return false;
            if (x.ElementId != y.ElementId) return false;
            if (x.ElementName != y.ElementName) return false;
            if (x.Text != y.Text) return false;
            return PlayerHelper.VerbString(x.Verbs) == PlayerHelper.VerbString(y.Verbs);
        }

        public int GetHashCode(ListData obj)
        {
            return obj.GetHashCode();
        }
    }

    public void UpdateList(ListType listType, List<ListData> items)
    {
        bool listChanged;
        if (!_lists.TryGetValue(listType, out var list))
        {
            listChanged = true;
        }
        else
        {
            listChanged = !list.SequenceEqual(items, _comparer);
        }

        if (!listChanged) return;
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
}