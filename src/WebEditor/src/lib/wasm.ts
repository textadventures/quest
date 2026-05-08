export interface WasmBridge {
  Initialise(bytes: Uint8Array, filename: string): Promise<boolean>
  GetTreeNodes(): string
  GetEditorData(key: string): string | null
  Save(): string
  Undo(): void
  Redo(): void
}

let _bridge: WasmBridge | null = null

export async function loadWasm(): Promise<WasmBridge> {
  if (_bridge) return _bridge

  // dotnet.js is served at runtime from the WasmEditor AppBundle via the Vite dev
  // server middleware (vite.config.ts). Vite-ignore prevents Vite trying to bundle it.
  // @ts-expect-error -- no type declarations for the .NET WASM runtime module
  const { dotnet } = await import(/* @vite-ignore */ '/AppBundle/dotnet.js')

  const { getAssemblyExports, getConfig, runMain } = await dotnet
    .withDiagnosticTracing(false)
    .create()

  await runMain()

  const config = getConfig()
  const exports = await getAssemblyExports(config.mainAssemblyName)
  _bridge = exports['QuestViva.WasmEditor.WasmEditorBridge'] as WasmBridge
  return _bridge
}
