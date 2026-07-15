using System.Diagnostics.CodeAnalysis;
using QuestViva.Engine;

// [DynamicDependency] on a reachable method tells the IL trimmer to preserve the specified
// members even though they're only accessed via reflection at runtime.
//
// NcalcExpressionEvaluator dispatches method calls and property accesses from game scripts
// via reflection — e.g. list.Count, list.Add("item"), dict.ContainsKey("key"). The trimmer
// can't trace these paths statically, so we root the public members here.
//
// If a game breaks with MissingMethodException or MissingMemberException on a Quest type,
// add the type below with the appropriate DynamicallyAccessedMemberTypes flags.
internal static class ScriptDispatchRoots
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties, typeof(QuestList<string>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties, typeof(QuestDictionary<string>))]
    internal static void EnsureRooted() { }
}
