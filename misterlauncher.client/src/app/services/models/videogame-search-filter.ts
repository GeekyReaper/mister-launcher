import { ItemCount } from "./item-count";

export interface VideogameSearchFilter {
  systemCategory: ItemCount[];
  systems: ItemCount[];
  playlist: ItemCount[];
  gametype: ItemCount[];
  players: ItemCount[];
}
