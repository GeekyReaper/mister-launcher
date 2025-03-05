import { GameAction } from "./game-action"
import { GameDb } from "./game-db"

export interface GameResult {
  gameDb: GameDb
  gameActions: GameAction[]
}
