import { FilterOption } from "./filter-option"
import { VideogameDb } from "./videogame-db"

export interface VideogameSearchResult {
  videogames: VideogameDb[]
  filterOption: FilterOption
  count: number
  page: number
  pagesize : number
}
