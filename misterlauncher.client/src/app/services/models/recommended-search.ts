import { GameSearchRequest } from "./game-search-request"

export interface RecommendedSearch {
  name: string
  category: string
  search: GameSearchRequest
}
