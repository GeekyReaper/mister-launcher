import { RegionAttribute } from "./region-attribute"

export interface GameDb {
  _id: string
  matchscreenscraper: boolean
  name: string
  name_region: string
  year: number
  romsize: number
  romname: string
  fullpath: string
  systemcategory: string
  date_region: string
  desc: string
  desc_lang: string
  systemid: string
  systemname: string
  editorname: string
  developname: string
  nbplayers: string
  media_fanart: string
  media_video: string
  media_manuel: string
  media_screenshot: string
  media_title: string
  romdate: string
  gamedate: string
  playlist?: string[]
  gametype: string[]
  rating: number
  screenscraperId?: number;
  names?: RegionAttribute[];
  dates?: RegionAttribute[];
  collection?: string;
  collectionId?: number;
  core?: string;
}
