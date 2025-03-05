import { GameSearchRequest } from "./game-search-request"

export interface RecommendedGameSearchRequest {
  name: string
  category: string
  gameSearchRequest: GameSearchRequest
}
