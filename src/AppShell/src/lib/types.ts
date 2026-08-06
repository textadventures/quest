export interface TreeNode {
  key: string
  text: string
  parent: string | null
  nodeIcon: string | null
  nodeType: string
  isLibrary: boolean
  // Authoritative — mirrors EditorController.CanDelete exactly (covers "game", the gamebook
  // player object, and any built-in library such as Core.aslx or a language file), so the UI
  // never needs its own guess at what's deletable.
  canDelete: boolean
}

export interface ControlOption {
  value: string
  label: string
  // Optional sticky-header grouping for Combobox.svelte, e.g. "Game functions" vs "Library
  // functions" for the Call function picker. Consumers that don't set it get the old flat list.
  group?: string
}

export interface TextProcessorCommand {
  command: string
  info: string
  insertBefore: string
  insertAfter: string
}

export interface ExpressionFunctionInfo {
  name: string
  parameters: string[]
  isUserDefined: boolean
  // True for aslx Function elements that came from a library (Core/English or an added one)
  // rather than the game itself. Only meaningful when isUserDefined is true.
  isLibrary: boolean
}

export interface ControlInfo {
  attribute: string | null
  controlType: string
  caption: string | null
  options: ControlOption[] | null
  subEditors: ControlOption[] | null
  subAttribute: string | null
  textProcessorCommands: TextProcessorCommand[] | null
  addPrompt: string | null
  elementType?: string | null
  objectType?: string | null
  listFilter?: string | null
  source?: string | null
  advanced: boolean
  keyPrompt?: string | null
  valuePrompt?: string | null
  sourceExclude?: string | null
  checkboxCaption?: string | null
  isWalkthrough?: boolean
  href?: string | null
  newFile?: string | null
  lockedAfterCreate?: boolean
}

export interface TabInfo {
  caption: string | null
  controls: ControlInfo[]
}

export interface EditorDataResponse {
  attributes: Record<string, string | null>
  tabs: TabInfo[]
  controls: ControlInfo[]
  isLibraryElement: boolean
  filename: string | null
}

export interface CompassDirectionInfo {
  direction: string
  typeKey: string
  inverseDirection: string
  inverseTypeKey: string
  exitKey: string | null
  to: string | null
  lookOnly: boolean
}

export interface ExitRowInfo {
  key: string
  alias: string | null
  to: string | null
  lookOnly: boolean
}

export interface ExitsData {
  compass: CompassDirectionInfo[]
  allExits: ExitRowInfo[]
  objects: ControlOption[]
}

export interface VerbInfo {
  attribute: string
  displayPattern: string
}

export interface ScriptControlData {
  controlType: string
  caption: string | null
  attribute: string | null
  value: string | null
  simpleEditor: string | null
  simpleLabel: string | null
  source: string | null
  options: ControlOption[] | null
  scripts: ScriptNodeData[] | null
  objectType?: string | null
  isFunctionPicker?: boolean
  isFunctionParams?: boolean
}

export interface ElseIfClauseData {
  id: string
  expression: string
  scripts: ScriptNodeData[]
}

export interface ScriptNodeData {
  id: string
  type: "normal" | "if"
  displayString?: string
  controls?: ScriptControlData[]
  expression?: string
  thenScripts?: ScriptNodeData[]
  elseIfClauses?: ElseIfClauseData[]
  elseScripts?: ScriptNodeData[] | null
}

export interface ScriptBlockData {
  scripts: ScriptNodeData[]
}

export interface ScriptCommandInfo {
  keyword: string
  display: string
  add: string
  createString: string
  advanced: boolean
}

export interface ScriptCategoryInfo {
  name: string
  commands: ScriptCommandInfo[]
  advanced: boolean
}

export interface ScriptCommandCategoriesData {
  categories: ScriptCategoryInfo[]
}

export interface ExpressionTemplateControlData {
  name: string
  value: string | null
  simpleEditor: string | null
  simpleLabel: string | null
  options: ControlOption[] | null
}

export interface IfExpressionTemplateData {
  templateName: string
  originalPattern: string
  controls: ExpressionTemplateControlData[]
}

export interface IfExpressionTemplate {
  name: string
  createExpression: string
}

export interface AttributeDataItem {
  name: string
  value: string | null
  isInherited: boolean
  source: string
  isDefaultType: boolean
  type: string
}

export interface FullAttributeData {
  attributes: AttributeDataItem[]
  inheritedTypes: AttributeDataItem[]
}
