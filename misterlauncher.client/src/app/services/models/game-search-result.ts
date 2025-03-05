import { FilterOption } from "./filter-option"
import { GameResult } from "./game-result"
import { RecommendedGameSearchRequest } from "./recommended-gamesearchrequest"

export interface GameSearchResult {
  games: GameResult[]
  filterOption: FilterOption
  recommendedGameSearchRequests: RecommendedGameSearchRequest[]
}
