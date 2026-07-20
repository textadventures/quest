using QuestViva.Engine;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression test: OrderedDictionary<TKey,TValue>'s Keys/Values/GetEnumerator used to read from
// the internal hash-based Dictionary<TKey,TValue> instead of the insertion-ordered List, even
// though preserving insertion order is the whole point of the class. Dictionary<TKey,TValue>
// happens to enumerate in insertion order as long as nothing is ever removed, which hid the bug -
// but a remove followed by an add reuses the freed slot, and enumeration order then diverges from
// the true append order. Any aslx script doing "dictionary remove" then "dictionary add" then
// "foreach" over the same dictionary would silently see the wrong order.
[TestClass]
public class QuestDictionaryTests
{
    [TestMethod]
    public void Foreach_AfterRemoveAndReAdd_PreservesTrueInsertionOrder()
    {
        var dict = new QuestDictionary<string>();
        dict.Add("a", "1");
        dict.Add("b", "2");
        dict.Add("c", "3");
        dict.Add("d", "4");
        dict.Remove("b");
        dict.Add("e", "5");

        dict.Keys.ShouldBe(["a", "c", "d", "e"]);

        var viaForeach = new List<string>();
        foreach (var kvp in dict)
        {
            viaForeach.Add(kvp.Key);
        }

        viaForeach.ShouldBe(["a", "c", "d", "e"]);
    }
}
