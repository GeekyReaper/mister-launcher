import { Injectable, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, map } from 'rxjs';
import { GameSearchResult } from './models/game-search-result';
import { GameSearch } from './models/game-search';
import { GameAction } from './models/game-action';
import { ManagerCache } from './models/manager-cache';
import { ManagerHealth } from './models/manager-health';
import { ModuleHealthcheck } from './models/module-healthcheck';
import { ManagerStats } from './models/manager-stats';
import { GameResult } from './models/game-result';
import { SystemSearchResult } from './models/system-search-result';
import { state } from '@angular/animations';
import { SystemSearchRequest } from './models/system-search-request';
import { SystemInfo } from './models/system-info';
import { VideogameSearchRequest } from './models/videogame-search-request';
import { VideogameSearchResult } from './models/videogame-search-result';
import { VideogameDb } from './models/videogame-db';
import { Subscription, timer, interval } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { CoreSavestate } from './models/core-savestate';
import { ModuleSetting } from './models/module-setting';
import { RomInfo } from './models/rom-info';
import { ItemCount } from './models/item-count';
import { JobRomscan } from './models/job-romscan';
import { VideogameSearchFilter } from './models/videogame-search-filter';
import { SystemDb } from './models/system-db';
import { ScriptsResult } from './models/scripts-result';


@Injectable(
  {
    providedIn: 'root'
  }
)
export class QuerygamesService implements OnInit, OnDestroy {

  subscriptionFilter?: Subscription;

  constructor(private http: HttpClient) { }

  currentVideoGameSearchRequest: VideogameSearchRequest = { name: "" };
  defaultSearchVideoGameFilter: VideogameSearchFilter = {
    systemCategory: [{ count: 0, label: "Arcade", value: "Arcade" }, { count: 0, label: "Console", value: "Console" }],
    systems: [],
    gametype: [],
    players: [],
    playlist: [{ count: 0, label: "Play Later", value: "playlater" }, { count: 0, label: "Favorite", value: "favorite" }, { count: 0, label: "Issue", value: "issue" }]

  }
  cacheallsystems: SystemDb[] = []
  subCacheSystem?: Subscription;

  ngOnInit(): void {
    //console.log("CACHE INIT")
    this.RefreshCacheAllSystems();

  }

  ngOnDestroy(): void {
    this.subscriptionFilter?.unsubscribe();
    this.subCacheSystem?.unsubscribe();
  }

  getCurrentVideoGameSearchRequest(): VideogameSearchRequest {
    return this.currentVideoGameSearchRequest;
  }
  setCurrentVideoGameSearchRequest(searchRequest: VideogameSearchRequest): void {
    this.currentVideoGameSearchRequest = searchRequest;
  }

  getDefaultSearchVideoGameFilter(): VideogameSearchFilter {
    return this.defaultSearchVideoGameFilter;
  }

  refreshDefaultSearchVideoGameFilter(): void {
    this.subscriptionFilter = this.GetVideoGameSearchFilter().subscribe((v: VideogameSearchFilter) => { this.defaultSearchVideoGameFilter = v });
  }


  getGames(gamesearch: GameSearch): Observable<GameSearchResult> {

    var payload = JSON.stringify(gamesearch, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<GameSearchResult>(`api/Game/search`, payload, { headers: headers });


  }

  getGameDetail(_id: string): Observable<GameResult> {

    var payload = `{"id" : "${_id}" }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<GameResult>(`api/Game/id`, payload, { headers: headers });

  }

  SetPlaylist(gameid: string, playlist: string, add: boolean): Observable<GameResult> {

    var payload = `{"GameId" : "${gameid}", "Playlist" : "${playlist}", "Add" : ${add} }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<GameResult>(`api/Game/playlist`, payload, { headers: headers });
  }


  getVideoGames(videogamesearch: VideogameSearchRequest): Observable<VideogameSearchResult> {
    var payload = JSON.stringify(videogamesearch, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<VideogameSearchResult>(`api/VideoGame/search`, payload, { headers: headers });
  }

  getVideoGameDetail(_id: string): Observable<VideogameDb> {

    var payload = `{"id" : "${_id}" }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/id`, payload, { headers: headers });

  }

  SetVideoGamePlaylist(id: string, playlist: string, add: boolean): Observable<VideogameDb> {

    var payload = `{"GameId" : "${id}", "Playlist" : "${playlist}", "Add" : ${add} }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/playlist`, payload, { headers: headers });
  }

  LaunchVideoGame(videogameid: string, romid?: string): Observable<VideogameDb> {
    var payload = `{"videoGameId" : "${videogameid}", "romId" : "${romid}"}`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    //setTimeout(() => {
    //  this.RefreshCacheInfo()
    //}, 10000);
    return this.http.post<VideogameDb>(`api/VideoGame/launch`, payload, { headers: headers });
  }

  UnlinkRomForVideogame(videogameid: string, romid?: string): Observable<VideogameDb> {
    var payload = `{"videogameid" : "${videogameid}", "romid" : "${romid}"}`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/unlinkrom`, payload, { headers: headers });
  }

  LinkRomForVideogame(videogameid: string, romid?: string): Observable<VideogameDb> {
    var payload = `{"videogameid" : "${videogameid}", "romid" : "${romid}"}`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/linkrom`, payload, { headers: headers });
  }

  SetPrimaryRomForVideogame(videogameid: string, romid?: string): Observable<VideogameDb> {
    var payload = `{"videogameid" : "${videogameid}", "romid" : "${romid}"}`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/setprimaryrom`, payload, { headers: headers });
  }


  getSystems(systemsearch: SystemSearchRequest): Observable<SystemSearchResult> {

    var payload = JSON.stringify(systemsearch, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<SystemSearchResult>(`api/System/search`, payload, { headers: headers });
  }

  GetAllSystemFromCache(): SystemDb[] {
    if (this.cacheallsystems.length == 0) {
      this.RefreshCacheAllSystems();
    }
    return this.cacheallsystems;
  }

  RefreshCacheAllSystems():void {

   
    let systemsearch: SystemSearchRequest = { limit: 500, sortFields: [{ field:"name", isAscending : true}] };
    var payload = JSON.stringify(systemsearch, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    this.subCacheSystem = this.http.post<SystemSearchResult>(`api/System/search`, payload, { headers: headers }).subscribe((r: SystemSearchResult) => {
      this.cacheallsystems = r.systems;
      //console.log(`CACHE System updated : ${this.cacheallsystems.length}`)
    });

  }

  getSystemDetail(_id: string): Observable<SystemInfo> {

    var payload = `{"id" : "${_id}" }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<SystemInfo>(`api/System/id`, payload, { headers: headers });
  }

  sendMisterCommand(cmd: string, israw : boolean = false): Observable<Boolean> {

    var payload = `
    {
      "cmds" : ["${cmd}"],
      "raw" : ${israw},
      "delay" : 0
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<Boolean>(`api/Core/keyboardcmd`, payload, { headers: headers });
  }

  sendMisterCommands(cmds: string[], delay : number): Observable<Boolean> {

    var payload = `
    {
      "cmds" : ${JSON.stringify(cmds, null, 4)},
      "raw" : false,
      "delay" : ${delay}
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<Boolean>(`api/Core/keyboardcmd`, payload, { headers: headers });
  }

  


  checkAction(gameaction: GameAction):boolean {
    var payload = JSON.stringify(gameaction.parameters, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    //alert(payload);
    var httpstatus: number;
    httpstatus = 0;

  

    this.http.post(`api/game/launch`, payload, { headers: headers, observe: 'response' }).subscribe(res => {
      httpstatus = res.status;
    });
    
      
    
    //alert(`HTTPSTATUS ${httpstatus}`)
      return (httpstatus == 0);


  }

  

  getSaveStates(videogameid?: string, romid?: string): Observable<CoreSavestate[]> {
    var payload = `
    {
      "videogameid" : "${videogameid}",
      "romid" : "${romid}"
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<CoreSavestate[]>(`api/Core/getallsavetates`, payload, { headers: headers });
  }

  SavestateCmdSave(videogameid?: string, romid?: string, slot? : number): Observable<CoreSavestate[]> {
    var payload = `
    {
      "videogameid" : "${videogameid}",
      "romid" : "${romid}",
      "slot" : ${slot}
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<CoreSavestate[]>(`api/Core/savestatecmdsave`, payload, { headers: headers });
  }


  SavestateCmdLoad(videogameid?: string, romid?: string, slot?: number): Observable<CoreSavestate[]> {
    var payload = `
    {
      "videogameid" : "${videogameid}",
      "romid" : "${romid}",
      "slot" : ${slot}
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');


    return this.http.post<CoreSavestate[]>(`api/Core/savestatecmdload`, payload, { headers: headers });
  }

  UpdateSystemSettings(system : SystemInfo ): Observable<SystemInfo> {
    var payload = JSON.stringify(system, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<SystemInfo>(`api/system/updatesettings`, payload, { headers: headers });
  }

  GetModuleSettings(modulename: string): Observable<ModuleSetting[]> {
    var payload = `
    {
      "modulename" : "${modulename}"      
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<ModuleSetting[]>(`api/core/getmodulesettings`, payload, { headers: headers });
  }

  SetModuleSettings(settings: ModuleSetting[]): Observable<Boolean> {
    var payload = `
    {
      "settings" : ${JSON.stringify(settings, null, 4)}      
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/core/setmodulesettings`, payload, { headers: headers });
  }

  CheckModuleSettings(settings: ModuleSetting[]): Observable<ModuleHealthcheck> {
    var payload = `
    {
      "settings" : ${JSON.stringify(settings, null, 4)}      
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<ModuleHealthcheck>(`api/core/checkmodulesettings`, payload, { headers: headers });
  }

  DeleteVideogame(id: string): Observable<Boolean> {
    var payload = `
    {
      "id" : "${id}"      
    }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/VideoGame/delete`, payload, { headers: headers });
  }

  UpdateVideogameSettings(item: VideogameDb): Observable<VideogameDb> {
    var payload = JSON.stringify(item, null, 4);
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/updatesettings`, payload, { headers: headers });
  }


  ScanArcadeRom(): Observable<Boolean> {
   let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<Boolean>(`api/Core/scanarcaderom`, { headers: headers });
  }

  ScanConsoleRom(systemid : string): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "systemid" : "${systemid}"      
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Core/scanconsolerom`,payload, { headers: headers });
  }


  GetRomFromId(id: string): Observable<RomInfo> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "id" : "${id}"      
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<RomInfo>(`api/Rom/id`, payload, { headers: headers });
  }

  GetUnmatchRoms(category: string, systemid: string): Observable<RomInfo[]> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "category" : "${category}",
      "systemid" : "${systemid}" 
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<RomInfo[]>(`api/Rom/unmatchroms`, payload, { headers: headers });
  }


  SearchVideogameFromScrapper(searchName: string, systemid?: string): Observable<VideogameDb[]> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "searchName" : "${searchName}",
      "systemid" : "${systemid}" 
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb[]>(`api/VideoGame/searchvideogamefromscrapper`, payload, { headers: headers }); 

  }

  

  LinkRomToScrapperVideogame(romid: string, scrapperVideogame: number, childroms: string[]): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "romid" : "${romid}",
      "scrapperVideogame" : ${scrapperVideogame},
      "childroms" : ${JSON.stringify(childroms, null, 4)}
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Rom/linkromtoscrappervideogame`, payload, { headers: headers }); 
  }

  SystemCountFilter(filter: string): Observable<ItemCount[]> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "filter" : "${filter}"
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<ItemCount[]>(`api/system/countfilter`, payload, { headers: headers });
  }

  AutomaticMatchRomJob(systemid: string, filterresultcode : number[]): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "systemid" : "${systemid}",
      "filterresultcode" :  ${JSON.stringify(filterresultcode,null,4)}
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Core/automaticmatchrom`, payload, { headers: headers });
  }

  LaunchJobScanRom(systemid: string): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "systemid" : "${systemid}"      
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Rom/launchjobscanrom`, payload, { headers: headers });
  }

  GetCurrentJob(): Observable<JobRomscan> {
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<JobRomscan>('api/Core/currentjob', { headers: headers });
  }

  LaunchJobScanSystem(): Observable<Boolean> {
    let headers = new HttpHeaders();
    
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<Boolean>(`api/system/scan`, { headers: headers });
  }


  GetScripts(): Observable<ScriptsResult> {
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<ScriptsResult>('api/Core/scripts', { headers: headers });
  }

  ExecuteScript(name :string, force: boolean): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "name" : "${name}",
      "force" : ${force}
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Core/executescript`, payload, { headers: headers });
  }


  DeleteRom(id: string, deletefile : Boolean): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "id" : "${id}",
      "deletefile" : ${deletefile}
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Rom/delete`, payload, { headers: headers });
  }

  SearchVideogameFromRomId(romid: string): Observable<VideogameDb> {
    let headers = new HttpHeaders();
    var payload = `
    {
      "id" : "${romid}"
    }`;
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<VideogameDb>(`api/VideoGame/searchvideogamefromromid`, payload, { headers: headers });

  }

  GetVideoGameSearchFilter(): Observable<VideogameSearchFilter> {
    let headers = new HttpHeaders();

    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<VideogameSearchFilter>(`api/videogame/getsearchvideogamefilter`, { headers: headers });
  }

}
