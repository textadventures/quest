export interface WasmBridge {
  Initialise(bytes: Uint8Array, filename: string): Promise<boolean>
  GetTreeNodes(): string
  GetEditorData(key: string): string | null
  SetAttribute(elementKey: string, attribute: string, controlType: string, value: string): string
  SetDropdownType(elementKey: string, controlId: string, selectedType: string): string
  Save(): string
  CanUndo(): boolean
  CanRedo(): boolean
  Undo(): void
  Redo(): void
}

let _bridge: WasmBridge | null = null;

export async function loadWasm(): Promise<WasmBridge> {
    if (_bridge) return _bridge;

    // dotnet.js is served at runtime by the Vite AppBundle middleware (vite.config.ts).
    // Use new Function to prevent Vite's import-analysis plugin from trying to resolve
    // the URL at build time — it only exists as a runtime-served file.
    const loadModule = new Function("url", "return import(url)");
    const { dotnet } = (await loadModule("/AppBundle/_framework/dotnet.js")) as { dotnet: any };

    const { getAssemblyExports, getConfig, runMain } = await dotnet
        .withDiagnosticTracing(false)
        .create();

    await runMain();

    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName);
    _bridge = exports.QuestViva.WasmEditor.WasmEditorBridge as WasmBridge;
    return _bridge;
}
