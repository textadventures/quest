using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

// WASM entry point — runs once when the module loads via runMain().
// [JSExport] methods on WasmPlayerBridge are available to JS after this returns.
return;
