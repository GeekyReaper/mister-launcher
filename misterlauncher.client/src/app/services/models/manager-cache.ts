import { ManagerHealth } from "./manager-health"
import { ManagerPlayinggame } from "./manager-playinggame";
import { ManagerStats } from "./manager-stats"

export interface ManagerCache {
  health: ManagerHealth
  stats: ManagerStats
  playingVideoGame: ManagerPlayinggame;
  lastUpdate: Date
}
