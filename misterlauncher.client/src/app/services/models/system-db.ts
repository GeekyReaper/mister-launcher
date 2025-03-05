export interface SystemDb {
  _id: string
  name: string
  gamepath: string
  category: string
  startyear: number
  endyear: number
  generation: number
  abreviation: string
  alternative_name: string
  family: string
  extensions: string
  company: string
  systemtype: any
  supporttype: string
  romtype: string
  name_eu: string
  name_jp: string
  name_us: string
  match: boolean
  startdate: string
  enddate: string
  statvideogame: number
  allowSaveStates: boolean
  allowSaveMemory : boolean
}
