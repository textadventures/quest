export interface WasmBridge {
  Initialise(bytes: Uint8Array, filename: string): Promise<boolean>
  GetTreeNodes(): string
  GetEditorData(key: string): Promise<string | null>
  SetAttribute(elementKey: string, attribute: string, controlType: string, value: string): string
  SetMultiType(elementKey: string, attribute: string, newType: string): string
  SetObjectReference(elementKey: string, attribute: string, objectName: string): string
  SetDropdownType(elementKey: string, controlId: string, selectedType: string): string
  Save(): string
  IsDirty(): boolean
  GetGameId(): string
  AddPublishAsset(filename: string, data: Uint8Array): void
  CreatePublishPackage(includeWalkthrough: boolean): Uint8Array
  CanUndo(): boolean
  CanRedo(): boolean
  Undo(): Promise<void>
  Redo(): void
  // Script editor API
  GetScriptCode(elementKey: string, attribute: string): string
  SetScriptCode(elementKey: string, attribute: string, code: string): string
  CopyScripts(elementKey: string, attribute: string, containerPath: string, indicesJson: string): string
  CutScripts(elementKey: string, attribute: string, containerPath: string, indicesJson: string): string
  DeleteScripts(elementKey: string, attribute: string, containerPath: string, indicesJson: string): string
  PasteScripts(elementKey: string, attribute: string, containerPath: string): string
  CanPasteScript(): boolean
  GetScriptData(elementKey: string, attribute: string): string | null
  SetScriptParameter(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramName: string, value: string): string
  SetIfExpression(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, expression: string): string
  SetElseIfExpression(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, elseIfIndex: number, expression: string): string
  AddScript(elementKey: string, attribute: string, containerPath: string, keyword: string): string
  DeleteScript(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string
  MoveScript(elementKey: string, attribute: string, containerPath: string, index1: number, index2: number): string
  AddElse(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string
  AddElseIf(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string
  RemoveElse(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string
  RemoveElseIf(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, elseIfIndex: number): string
  GetScriptCommandCategories(): Promise<string>
  GetObjectNames(): string
  GetIfExpressionTemplates(): string
  GetIfExpressionTemplateData(expression: string): string | null
  // List editor API
  AddListItem(elementKey: string, attribute: string, value: string): string
  RemoveListItem(elementKey: string, attribute: string, key: string): string
  UpdateListItem(elementKey: string, attribute: string, key: string, value: string): string
  // Attributes editor API
  GetFullAttributeData(elementKey: string): string | null
  RemoveAttribute(elementKey: string, attribute: string): string
  AddInheritedType(elementKey: string, typeName: string): string
  RemoveInheritedType(elementKey: string, typeName: string): string
  GetTypeNames(): string
  AddDictionaryItem(elementKey: string, attribute: string, key: string, value: string): string
  RemoveDictionaryItem(elementKey: string, attribute: string, key: string): string
  UpdateDictionaryItem(elementKey: string, attribute: string, key: string, value: string): string
  MakeScriptEditable(elementKey: string, attribute: string): string
  MakeScriptDictEditable(elementKey: string, attribute: string): string
  AddScriptDictionaryItem(elementKey: string, attribute: string, key: string): string
  RemoveScriptDictionaryItem(elementKey: string, attribute: string, key: string): string
  ChangeAttributeType(elementKey: string, attribute: string, newType: string): string
  SetPatternAttribute(elementKey: string, attribute: string, pattern: string): string
  // Element creation / deletion
  ValidateName(name: string): string
  GetUniqueName(baseName: string): string
  CreateRoom(name: string, parent: string): string
  CreateObject(name: string, parent: string): string
  CreateFunction(name: string): string
  CreateTimer(name: string): string
  CreateExit(parent: string): string
  CreateTurnScript(parent: string): string
  CreateCommand(parent: string): string
  CreateVerb(parent: string): string
  CreateWalkthrough(name: string, parent: string): string
  CreateTemplate(name: string): string
  CreateDynamicTemplate(name: string): string
  CreateObjectType(name: string): string
  CreateIncludedLibrary(): string
  CreateJavascript(): string
  DeleteElement(key: string): void
  SwapElements(key1: string, key2: string): string
  // New game
  GetGameTemplates(): string
  CreateGameFromTemplate(templateId: string, gameName: string): string
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
