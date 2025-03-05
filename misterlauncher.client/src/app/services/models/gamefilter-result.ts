import { SystemInfo } from "./system-info";

export interface GamefilterResult {
  GameTypes: string[];
  NbPlayers: string[];
  GameSystems: SystemInfo[]
  MinYear: number;
  MaxYear: number;
  SystemCategory: string[];
}
