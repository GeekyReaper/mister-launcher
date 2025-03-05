import { SortField } from "./sort-field"

export interface GameSearchRequest {
  id?: string
  name?: string
  regex?: string
  systemId?: string
  nbPlayers?: string
  year?: number
  yearMax?: number
  yearMin?: number
  allowUnknowYear?: boolean
  allowUnRated: boolean
  minRating: number
  systemCategory: string
  matchscreenscraper: boolean
  maxRating: number
  editor: string
  developname: string
  gameType: string[]
  limit: number
  sortFields: SortField[]
}
