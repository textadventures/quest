export interface WasmBridge {
  AddAdjacentFile(filename: string, data: Uint8Array): void
  Initialise(bytes: Uint8Array, filename: string): Promise<string>
  GetTreeNodes(): string
  SetShowLibraryElements(show: boolean): void
  MakeElementLocal(elementKey: string): string
  GetEditorData(key: string): Promise<string | null>
  SetAttribute(elementKey: string, attribute: string, controlType: string, value: string): string
  SetMultiType(elementKey: string, attribute: string, newType: string): string
  SetObjectReference(elementKey: string, attribute: string, objectName: string): string
  SetDropdownType(elementKey: string, controlId: string, selectedType: string): string
  SetSelectedFilter(elementKey: string, filterGroupName: string, filterValue: string): string
  Save(): string
  IsDirty(): boolean
  GetGameXml(): string
  SetGameXml(xml: string): Promise<string>
  GetGameId(): string
  IsGamebook(): boolean
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
  GetExitNames(): string
  GetExpressionTemplates(expressionType: string): string
  GetExpressionTemplateData(expression: string, expressionType: string): string | null
  // List editor API
  AddListItem(elementKey: string, attribute: string, value: string): string
  RemoveListItem(elementKey: string, attribute: string, key: string): string
  UpdateListItem(elementKey: string, attribute: string, key: string, value: string): string
  // Script parameter-list API (e.g. Call function's parameters)
  AddScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, value: string): string
  RemoveScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, key: string): string
  UpdateScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, key: string, value: string): string
  SetScriptListItemCount(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, count: number): string
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
  CreatePage(name: string, parent: string): string
  GetPageNames(): string
  CreateFunction(name: string): string
  CreateTimer(name: string): string
  CreateExit(parent: string): string
  GetExitsData(roomKey: string): string
  CreateExitInDirection(roomKey: string, direction: string, to: string, createInverse: boolean): string
  CreateLookExitInDirection(roomKey: string, direction: string): string
  // Verbs editor API
  GetVerbAttributesInfo(): string
  GetExpressionFunctions(): string
  AddVerb(elementKey: string, verbPattern: string): string
  CreateTurnScript(parent: string): string
  CreateCommand(parent: string): string
  CreateVerb(parent: string): string
  CreateWalkthrough(name: string, parent: string): string
  RecordWalkthroughSteps(name: string, stepsJson: string): string
  CreateTemplate(name: string): string
  CreateDynamicTemplate(name: string): string
  CreateObjectType(name: string): string
  CreateIncludedLibrary(filename: string): string
  CreateJavascript(src: string): string
  DeleteElement(key: string): void
  DeleteElements(keysJson: string): void
  SwapElements(key1: string, key2: string): string
  // Move / cut / copy / paste
  CanMoveElement(elementKey: string): boolean
  GetMovePossibleParents(elementKey: string): string
  MoveElement(elementKey: string, newParentKey: string): string
  CopyElements(keysJson: string): void
  CutElements(keysJson: string): void
  CanPasteElements(parentKey: string): boolean
  PasteElements(parentKey: string): string
  // New game
  GetGameTemplates(): string
  CreateGameFromTemplate(templateId: string, gameName: string): string
  // Play tab "Recently played" cover art — see WasmEditorBridge.ResolveLocalCover. Return
  // value is a JSON-encoded LocalCoverResult ({ name, dataUrl }) or null, not a bare string —
  // multiple of these can be in flight at once (one per recent-local-play card), so unlike
  // most of the "returns a name, fetch the rest via a second synchronous call" pairs above,
  // this can't stash its result in shared state between the two calls without racing.
  ResolveLocalCover(gameFileBytes: Uint8Array, filename: string): Promise<string | null>
}

let _bridge: WasmBridge | null = null;
// The in-flight load itself, not just its result — dotnet.js's runtime module can only be
// created once per page (a second concurrent dotnet.create() throws "Runtime module already
// loaded", and _bridge never gets set, permanently wedging every later caller too). A plain
// `if (_bridge) return` guard only protects callers that arrive after the first load has
// already finished; two callers arriving before then (e.g. several LocalFileRecentCard
// instances resolving cover art on the same Play-tab render) would each see _bridge still
// null and race to call create() themselves. Caching the promise makes every concurrent
// caller await the same single load instead.
let _loading: Promise<WasmBridge> | null = null;

export async function loadWasm(): Promise<WasmBridge> {
    if (_bridge) return _bridge;
    if (_loading) return _loading;

    _loading = (async () => {
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
    })();

    try {
        return await _loading;
    } finally {
        _loading = null;
    }
}
