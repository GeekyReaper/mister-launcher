import { ItemCount } from "./item-count"
import { SystemDb } from "./system-db"

export interface FilterOption {
  gameTypes: string[]
  nbPlayers: string[]
  systemDbs: SystemDb[]
  minYear: number
  maxYear: number
  systemCategory: string[]
  collections: string[]
  editors: string[]
  developers : string[]
}
