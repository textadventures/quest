export interface TreeNode {
  key: string
  text: string
  parent: string | null
}

export interface ControlOption {
  value: string
  label: string
}

export interface ControlInfo {
  attribute: string | null
  controlType: string
  caption: string | null
  options: ControlOption[] | null
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
