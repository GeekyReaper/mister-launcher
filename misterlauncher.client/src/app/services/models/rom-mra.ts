export interface RomMra {
  name: string;
  region: string;
  homebrew: Boolean;
  bootleg: Boolean;
  version: string;
  alternative: string;
  platform: string;
  series: string;
  year: number;
  manufacturer: string;
  category: string;
  setname: string;
  parent: string;
  mameversion: string;
  rbf: string;
  about: string;
  resolution: string;
  rotation: string;
  flip: string;
  players: string;
  joystick: string;
  date: Date;  
  romshierarchy: string[];        
  md5: string;
}
