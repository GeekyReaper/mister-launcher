export interface SystemInfo {
 _id: string;
  name: string;
  gamepath: string;
  category: string;
  startyear: number;
  endyear: number;
  generation: number;
  abreviation: string;
  alternative_name: string;
  family: string;
  extensions: string;
  excluderompaterns: string;
  unofficalpathrompaterns: string;
  company: string;
  systemtype: string;
  supporttype: string;
  romtype: string;
  name_eu: string;
  name_jp: string;
  name_us: string;
  match: boolean;
  startdate:string;
  enddate: string;
  media_logomonochrome?: string;
  media_photo?: string;
  media_illustration?: string;
  media_controller?: string;
  media_wheel?: string;
  media_video?: string;
  media_BoitierConsole3D?: string;
  statvideogame: number;
  statromfound: number;
  statrommatch: number;
  allowSaveStates: boolean;
  allowSaveMemory: boolean;
  core: string;
}
