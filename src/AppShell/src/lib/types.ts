export interface TreeNode {
  key: string
  text: string
  parent: string | null
  nodeType: string
  isLibrary: boolean
  // Authoritative — mirrors EditorController.CanDelete exactly (covers "game", the gamebook
  // player object, and any built-in library such as Core.aslx or a language file), so the UI
  // never needs its own guess at what's deletable.
  canDelete: boolean
  // The .aslx file this element was loaded from (e.g. "Core.aslx", "CoreTimers.aslx", or the
  // game's own file). Only meaningful for grouping when isLibrary is true.
  filename: string | null
  // User-assigned organisational folder name, currently only settable on Function elements via
  // "Move to folder" — purely a display grouping, like filename above, but author-controlled.
  folder: string | null
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
  sourceType?: string | null
  checkboxCaption?: string | null
  isWalkthrough?: boolean
  href?: string | null
  newFile?: string | null
  lockedAfterCreate?: boolean
  // <keyname> - overrides a "multi" control's scriptdictionary sub-editor's generic "key"
  // label (e.g. "Object" for useon/selfuseon/give/giveto, whose keys are object names).
  keyName?: string | null
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
  breakBefore?: boolean
  // The expressionType (e.g. "set", "foreach") this control's <usetemplates> declares - see
  // ExpressionField.svelte, which renders a template picker instead of a plain ExpressionInput
  // when this is set and templates exist for that type.
  useTemplates?: string | null
  // Pre-fetched nested script trees for a "scriptdictionary" control (e.g. switch's "cases"),
  // one per dictionary key - lets the case-list editor render each case's script from initial
  // data without a further round trip, mirroring how "scripts" does this for a "script" control.
  cases?: CaseScriptData[] | null
  // <multiline/> - the "textbox" simple editor should render as a resizable textarea that
  // keeps embedded newlines instead of a single-line input.
  multiline?: boolean
  // <expand/> - this control should grow to fill the remaining width of its row instead of
  // being capped to a fixed max-width.
  expand?: boolean
}

export interface ElseIfClauseData {
  id: string
  expression: string
  scripts: ScriptNodeData[]
}

export interface CaseScriptData {
  key: string
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
  common: string | null
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

export interface ExpressionTemplateData {
  templateName: string
  originalPattern: string
  controls: ExpressionTemplateControlData[]
}

export interface ExpressionTemplate {
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
