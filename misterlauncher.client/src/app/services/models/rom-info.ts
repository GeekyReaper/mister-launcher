import { RomMra } from "./rom-mra";

export interface RomInfo {
  _id: string;  
  fullpath: string;  
  systemCategory: string;          
  isMatch: boolean;
  size: number;
  extension: string;  

  official : boolean;
  name: string;  
  fullname: string;  
  systemid : string;
  version : string;
  region : string;
  language : string;
  supporttype : string;

  core : string;
  mraInfo?: RomMra;
  date: Date;
  lastscandate: Date;
  firstscandate: Date;

  parsingexception : string;
  responsetime: number;

  checksum_md5: string;
  checksum_crc: string;

  scrapperResult: number;

}
