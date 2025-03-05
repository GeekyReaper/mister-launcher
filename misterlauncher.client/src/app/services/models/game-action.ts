export interface Dictionary<T> {
  [Key: string]: T;
}

export interface GameAction {
  name: string
  category: string
  parameters: Dictionary<string>
  path: string
  method: string
}
