import { FieldSort } from "./field-sort";

export interface GameSearch {
  name?: string;
  regex?: string;
  systemId?: string;

  nbPlayers?: string;
       year?: number;
       yearMax?: number;
       yearMin?: number;
        allowUnknowYear?: boolean;
        allowUnRated?: boolean;
       minRating?: number;
systemCategory?: string;
  playlist?: string;
        matchscreenscraper?: boolean;
       maxRating?: number;
editor?: string;
developname?: string;

gameType?: string[];

  limit?: number;

  gameTypeExcluded?: string[];
  collectionId?: number;
  core?: string;

  gamesExcluded?: string[];
  systemsExcluded?: string[];


  sortFields?: FieldSort[];
}
