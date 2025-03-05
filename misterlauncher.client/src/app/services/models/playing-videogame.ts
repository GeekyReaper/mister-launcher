import { RomDb } from "./rom-db";
import { SystemDb } from "./system-db";
import { VideogameDb } from "./videogame-db";

export interface PlayingVideogame {
  currentVideogame?: VideogameDb
  playingRomdb?: RomDb;
  systemDb?: SystemDb;
  isPlaying: Boolean;
}
