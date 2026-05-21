export interface TreeNode {
  key: string
  text: string
  parent: string | null
  nodeIcon: string | null
  nodeType: string
}

export interface ControlOption {
  value: string
  label: string
}

export interface TextProcessorCommand {
  command: string
  info: string
  insertBefore: string
  insertAfter: string
}

export interface ControlInfo {
  attribute: string | null
  controlType: string
  caption: string | null
  options: ControlOption[] | null
  subEditors: ControlOption[] | null
  subAttribute: string | null
  textProcessorCommands: TextProcessorCommand[] | null
}

export interface TabInfo {
  caption: string | null
  controls: ControlInfo[]
}

export interface EditorDataResponse {
  attributes: Record<string, string | null>
  tabs: TabInfo[]
  controls: ControlInfo[]
}

export interface ScriptControlData {
  controlType: string
  caption: string | null
  attribute: string | null
  value: string | null
  simpleEditor: string | null
  simpleLabel: string | null
  options: ControlOption[] | null
  scripts: ScriptNodeData[] | null
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
}

export interface ScriptCategoryInfo {
  name: string
  commands: ScriptCommandInfo[]
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
