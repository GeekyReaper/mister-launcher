import { GameInfo } from "./game-info";
import { GamefilterResult } from "./gamefilter-result";

export interface GamesResult {
  games: GameInfo[];
  filter: GamefilterResult;
}
