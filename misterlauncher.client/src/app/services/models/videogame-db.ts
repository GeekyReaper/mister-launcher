import { RegionAttribute } from "./region-attribute"
import { RomDb } from "./rom-db";

export interface VideogameDb {
  _id: string;
  screenscraperId?: number;
  name: string;
  name_region: string;
  year: number;
  systemcategory: string;
  date_region: string;
  desc: string;
  desc_lang: string;
  systemid: string;
  systemname: string;
  editorname: string;
  developname: string;
  nbplayers: string;
  media_fanart: string;
  media_video: string;
  media_manuel: string;
  media_screenshot: string;
  media_title: string;
  gamedate: Date;
  playlist?: string[];
  gametype: string[];
  rating: number;  
  names?: RegionAttribute[];
  dates?: RegionAttribute[];
  collection?: string;
  collectionId?: number;
  core?: string;
  roms: RomDb[];
  romscount: number;
  namesearch: string;
}
