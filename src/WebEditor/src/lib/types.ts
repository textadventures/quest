export interface TreeNode {
  key: string
  text: string
  parent: string | null
}

export type ElementAttributes = Record<string, string | null>
