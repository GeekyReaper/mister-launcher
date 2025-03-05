import { FieldSort } from "./field-sort";

export interface VideogameSearchRequest {
  name?: string;
  regex?: string;
  systems?: string[];
  nbPlayers?: string;
  year?: number;
  yearMax?: number;
  yearMin?: number;
  allowUnknowYear?: boolean;
  allowUnRated?: boolean;
  minRating?: number;
  systemCategory?: string;
  playlist?: string;
  maxRating?: number;
  editor?: string;
  developname?: string;
  gameType?: string[];
  pagesize?: number;
  page?: number;
  limit?: number;
  gameTypeExcluded?: string[];
  collection?: string;
  core?: string;
  gamesExcluded?: string[];
  systemsExcluded?: string[];
  sortFields?: FieldSort[];
  playedhitMin?: number;
  playedhitMax?: number;
  playedlastMin?: Date;
  playedlastMax?: Date;

}
