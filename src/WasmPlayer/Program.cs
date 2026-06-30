using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

// Root script-dispatch types so the IL trimmer preserves their public members.
// See ScriptDispatchRoots.cs for details and how to extend the list.
ScriptDispatchRoots.EnsureRooted();

// WASM entry point — runs once when the module loads via runMain().
// [JSExport] methods on WasmPlayerBridge are available to JS after this returns.
return;
