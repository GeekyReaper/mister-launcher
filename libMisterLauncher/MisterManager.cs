using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Amazon.Runtime.Internal.Util;
using libMisterLauncher.Entity;
using libMisterLauncher.Service;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using static System.Net.WebRequestMethods;

[assembly: InternalsVisibleTo("MisterLauncher.Test")]
namespace libMisterLauncher.Manager
{
    public delegate void CacheUpdated();
    public delegate void JobRomUpdate(JobMister job);

    public class MisterManager
    {
        //private Dictionary<string, CommandApi> CommandApiList = new Dictionary<string, CommandApi>();

        private Dictionary<Type,IMisterModule> _modules = new Dictionary<Type, IMisterModule>();

        public event CacheUpdated? OnCacheUpdated;
        public event JobRomUpdate? OnJobRomUpdate;


        internal GamedbService gamedbService {  
            get
            {
               return getModule<GamedbService>();
            } }
        internal MisterRemoteService misterremoteService
        {
            get
            {
                var t = getModule<MisterRemoteService>();
                if (t == null)
                    LoadModules();

                return getModule<MisterRemoteService>();
            }
        }

        internal MisterFtpService misterftpService
        {
            get
            {
                var t = getModule<MisterFtpService>();
                if (t == null)
                    LoadModules();

                return getModule<MisterFtpService>();
                
            }
        }

        internal MisterMediaService mistermediaService
        {
            get
            {
                var t = getModule<MisterMediaService>();
                if (t == null)
                    LoadModules();

                return getModule<MisterMediaService>();                
            }
        }

        internal ScrapperScService scrapperService
        {
            get
            {
                var t = getModule<ScrapperScService>();
                if (t == null)
                    LoadModules();

                return getModule<ScrapperScService>();                
            }
        }

        internal MisterAuthService authService
        {
            get
            {
                var t = getModule<MisterAuthService>();
                if (t == null)
                    LoadModules();

                return getModule<MisterAuthService>();
            }
        }

        internal MisterManagerCache _cache = new MisterManagerCache();

        internal JobMister? _currentJob = null;


        public MisterManager ()
        {
                  
        }

        public void Initialize(GameDbSettings gdbSettings, RemoteServiceSettings remoteSettings, FtpServiceSettings ftpSettings,MediaServiceSettings mediaSettings, ScrapperScServiceSettings scSettings, AuthServiceSettings authSettings)
        {
            setModule(new GamedbService(gdbSettings));
            setModule(new MisterRemoteService(remoteSettings));
            setModule(new MisterFtpService(ftpSettings));
            setModule(new MisterMediaService(mediaSettings));
            setModule(new ScrapperScService(scSettings));
            setModule(new MisterAuthService(authSettings));
            RefreshCache();
        }

        public void Initialize(GameDbSettings gdbSettings)
        {
            if (!gdbSettings.isValid())
            {
                return;
            }
            setModule(new GamedbService(gdbSettings));
            _cache.Health.AddModuleHealth(gamedbService.CheckHealth());



            LoadModules();

        }


        public void LoadModules()
        {
            if (gamedbService.CheckConnection())
            {
                loadModule<MisterFtpService, FtpServiceSettings>();
                loadModule<MisterMediaService, MediaServiceSettings>();
                loadModule<MisterRemoteService, RemoteServiceSettings>();
                loadModule<ScrapperScService, ScrapperScServiceSettings>();
                loadModule<MisterAuthService, AuthServiceSettings>();
            
            }
            RefreshCache();
        }

       
        private void MisterremoteService_OnGamePlaying(RemoteServiceCurrentGame currentGame)
        {
           
            var systems = _cache.GetSystems();
            if (systems.Count()==0)
            {
                var s = gamedbService.GetAllSystems().Result;
                foreach (var m in s) { _cache.StoreSystems(m); };
                systems = _cache.GetSystems();
            }
            
            var playinggame = CurrentVideogame(currentGame).Result;
            if (playinggame.currentVideogame!= null)
            {
                var r = gamedbService.IncrementVideogamePlayed(playinggame.currentVideogame._id);
            }
            if (!string.IsNullOrEmpty(currentGame.core) && playinggame.IsPlaying == false)
            { // No match between core and videogame bescause Console CORE are not detected.
                return;
            }
            _cache.playingVideoGame.SetPlayingGame(playinggame);
            _cache.LastUpdate = DateTime.Now;
            if (OnCacheUpdated != null)
                OnCacheUpdated();
        }


        public void checkModuleList()
        { 
            var modulelist = new List<ModuleDefinition>() {
                new ModuleDefinition() { ModuleType = typeof(MisterRemoteService), SettingsType = typeof(RemoteServiceSettings) },
                new ModuleDefinition() { ModuleType = typeof(MisterFtpService), SettingsType = typeof(FtpServiceSettings) },
                new ModuleDefinition() { ModuleType = typeof(MisterMediaService), SettingsType = typeof(MediaServiceSettings) },
                new ModuleDefinition() { ModuleType = typeof(ScrapperScService), SettingsType = typeof(ScrapperScServiceSettings) },
                new ModuleDefinition() { ModuleType = typeof(MisterAuthService), SettingsType = typeof(AuthServiceSettings) }
            };
            
            foreach (var definition in modulelist)
            {
                
                if (!_modules.ContainsKey(definition.ModuleType))
                {
                    var s = Activator.CreateInstance(definition.SettingsType) as IMisterSettings;
                    s.LoadModuleSettings(gamedbService.GetModuleSettings(s.ModuleName).Result);
                    var m = Activator.CreateInstance(definition.ModuleType) as IMisterModule;
                    m.LoadSettings(s);
                    if (s.ModuleName == "MisterRemote")
                    {
                        var remoteservice = (m as MisterRemoteService);
                        if (remoteservice != null)
                            remoteservice.OnGamePlaying += MisterremoteService_OnGamePlaying;
                    }
                    setModule(m);

                }
            }


        }

        private void loadModule<T, K>() where T : class, IMisterModule, new() where K : IMisterSettings, new()
        {
            var settings = new K();
            var dbsettings = gamedbService.GetModuleSettings(settings.ModuleName).Result;
            if (dbsettings.Count == 0)
            {
                bool settingssave = gamedbService.SetModuleSettings(settings.GetModuleSettings()).Result;
            }
            else
            {
                settings.LoadModuleSettings(dbsettings);
            }
            T module = new T();
            module.LoadSettings(settings);
            
            if (settings.ModuleName == "MisterRemote")
            {
                var remoteservice = (module as MisterRemoteService);
                if (remoteservice !=null)
                    remoteservice.OnGamePlaying += MisterremoteService_OnGamePlaying;
            }
            setModule(module);


        }

        public async void RefreshCache()
        {
            foreach (var k in _modules.Keys)
            {
                _cache.Health.AddModuleHealth(_modules[k].CheckHealth());
            }
            if (gamedbService!=null)
            {
                _cache.Stats = await gamedbService.GetStats();
                var s = await gamedbService.GetAllSystems();               
                foreach (var m in s) { _cache.StoreSystems(m); };
            }           
            //RefreshPlayingGame();
            _cache.LastUpdate = DateTime.Now;
            

        }

        public void RefreshPlayingGame(int delay=0)
        {
            if (delay>0)
            {
                Thread.Sleep(delay);
            }
            var playinggame = CurrentVideogame().Result;
            _cache.playingVideoGame.SetPlayingGame(playinggame);
            _cache.LastUpdate = DateTime.Now;
            if (OnCacheUpdated != null)
                OnCacheUpdated();

        }

        public MisterManagerCache Cache()
        {
            return _cache;
        }

        private void setModule<T>(T module) where T : class, IMisterModule
        {
            if (module!=null)
            {
                if (!_modules.ContainsKey(typeof(T)))
                {
                    _modules.Add(typeof(T), module as T);
                }
                else
                {
                    _modules[typeof(T)] = module;
                }
            }
        }
        private T? getModule<T>() where T : class, IMisterModule
        {
            return _modules.ContainsKey(typeof(T)) ? _modules[typeof(T)] as T : null;            
        }
       
       
        public async Task<SearchVideoGameFilter> GetVideoGameSearchFilter ()
        {
            var result = new SearchVideoGameFilter();
            List<string> items = await gamedbService.GetDistinctValues<string>(EnumCollection.VIDEOGAME, "systemCategory");
            result.systemCategory = (from i in items select new ItemCount() { count = 0, label = i, value = i }).ToList();
            if (scrapperService!=null)
                result.gametype = (from i in scrapperService.GetGameType(true) select new ItemCount() { count = 0, label = i, value = i }).ToList();

            items = await gamedbService.GetAllPlaylist();
            result.playlist = (from i in items select new ItemCount() { count = 0, label = i, value = i }).ToList();

            items = await gamedbService.GetDistinctValues<string>(EnumCollection.VIDEOGAME, "nbplayers");
            result.players = (from i in items select new ItemCount() { count = 0, label = i, value = i }).ToList();

            result.systems = (from s in _cache.GetSystems() select new ItemCount() { count = 0, label = s.name, value = s._id }).ToList();
            return result;

        }

        public async Task<SystemSearchResult> GetSystemResult (GameSystemSearch search)
        {
            var result = new SystemSearchResult();

            if (gamedbService!=null)
            {
                result.systems = await gamedbService.GetMatchGameSystem(search);
                result.count = result.systems.Count;
            }
            return result;
        }

        public async Task<SystemDb?> GetSystem(string id)
        {
            var result = new SystemDb();
            if (gamedbService == null)
                return result;
            return await gamedbService.GetSystemDb(id);

        }

       
        #region VideoGame
        public async Task<VideoGameSearchResult> GetVideoGameResultAsync(VideoGameSearchRequest search)
        {
            return await Task.Run(() =>
            {
                return GetVideoGameResult(search);
            });

        }
        private VideoGameSearchResult GetVideoGameResult(VideoGameSearchRequest search)
        {
            var result = new VideoGameSearchResult();
            if (gamedbService == null)
                return result;
            result.videogames = gamedbService.GetMatchVideoGame(search);
            result.count = result.videogames.Count;
            result.filterOption = gamedbService?.GetFilterOption(result.videogames, search);
            
            var limit = search.pagesize != null && search.pagesize > 0 ? (int)search.pagesize : 20;
            result.pagesize = limit;
            result.page = search.page != null && search.page > 0 ? (int)search.page : 1;
            var start = (result.page - 1) * limit;
            result.videogames = result.videogames.Take(new Range (start, limit+start)).ToList();
            return result;
        }

        public async Task<VideoGameDb?> GetVideoGame(string id)
        {
            var result = new VideoGameDb();
            if (gamedbService == null)
                return result;
            return await gamedbService.GetVideoGame(id);
            

        }

        public async Task<VideoGameDb?> SetVideoGamePlaylist(string id, string playlist, bool add = true)
        {
            if (gamedbService == null)
                return new VideoGameDb();
            var result = await gamedbService.SetVideoGamePlaylist(id, playlist, add);
            return await this.GetVideoGame(id);

        }

        public async Task<VideoGameDb?> LaunchVideoGame(string videoGameId, string romId)
        {
            var videoGame = await GetVideoGame(videoGameId);
            
            if (videoGame == null)
                return null;

            var rom = videoGame.roms.Where(r => r.romid == romId).FirstOrDefault();

            if (rom == null)
                return null;

            var system = _cache.GetSystem(videoGame.systemid);

            var result = await misterremoteService.LaunchGame(rom.fullpath, system);

            if (!result)
            {
                return null;
            }
            
            //Task.Run(() => {
            //        RefreshPlayingGame(5000);
            //    });          


            return videoGame;
            

        }

        public async Task<VideoGameDb?> RomActionRomForVideogame(string videogameid, string romid, RomAction action)
        {
            var vg =  await gamedbService.RomActionRomForVideogame(videogameid, romid, action);
            if (vg!=null && vg.romscount == 0)
            {
                var delete = await DeleteVideoGame(videogameid);                
            }
            return vg;
        }

        public async Task<VideoGameDb?> SearhVideogameByRomid(string romid)
        {
            return await gamedbService.SearchVideoGameByRomId(romid);
        }

        public async Task<bool> LinkRomToVideogameId(string romid, int screenscraperVideoGameId, List<string> childroms)
        {
            JsonRequestResult scrapperresult;
            SystemDb? system = null;

            // loadRom
            var rom = await gamedbService.GetRom(romid);
            if (rom == null)
                return false;

            // Search videogam from scrapper
            scrapperresult = await scrapperService.GetVideoGameFromId(screenscraperVideoGameId);
            if (scrapperresult == null || scrapperresult.json == null)
            {
                return false;
            }
            // update Rom information
            rom.scrapperResult = scrapperresult.HttpCode;
            rom.responsetime = scrapperresult.responsetime;
            rom.parsingexception = scrapperresult.parsingException;


           
            
            // Convert to Full Videogame
            var videogame = scrapperService.ConvertVideogameFromJson(scrapperresult.json);
            var isOk = false;

            var existVideoGame = gamedbService.GetVideoGameFomScrapperId(videogame.screenscraperId).Result;
            if (existVideoGame != null)
            {// Add Rom
                bool romislinked = (from r in existVideoGame.roms where r.romid == rom._id select r).Count() > 0;
                if (!romislinked)
                {
                    var romsc = scrapperService.ConvertRomFromJson(scrapperresult.json);
                    romsc.romid = rom._id;
                    romsc.fullpath = rom.fullpath;
                    romsc.name = rom.name;
                    romsc.date = rom.date;
                    // for arcade game
                    romsc.core = rom.core;

                    // synchronize informations
                    rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                    romsc.region = rom.region;

                    rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                    romsc.language = rom.language;

                    rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                    romsc.supportType = rom.supporttype;

                   

                    if (existVideoGame.roms == null)
                        existVideoGame.roms = new List<RomDb>();
                    existVideoGame.roms.Add(romsc);
                    existVideoGame.romscount = existVideoGame.roms.Count();

                    isOk = await gamedbService.UpdateVideogameRoms(existVideoGame);

                }
                videogame._id = existVideoGame._id;
               


            }
            else
            { // Create VideoGame


                // Get System
                if (rom.systemCategory == "Console")
                {
                    system = await GetSystem(rom.systemid);
                    if (system == null)
                    {
                        return false;
                    }                   
                }
                else
                { // Arcade case
                    var scrappersystemid = scrapperService.GetSystemScrapperIdFromJsonVideogame(scrapperresult.json);
                    if (scrappersystemid > 0)
                    {
                        system = _cache.GetSystems().Where(s => s.screenscraperId == scrappersystemid).FirstOrDefault();
                        if (system == null || system.category != "Arcade")
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }




                // Complete SystemInfo
                videogame.systemcategory = system.category;
                videogame.systemid = system._id;
                videogame.systemname = system.name;

                // SetROM systemid
                rom.systemid = system._id;

                // set ID
                videogame.setId();

                // set link rom
                if (videogame.roms == null)
                    videogame.roms = new List<RomDb>();
                var romsc = scrapperService.ConvertRomFromJson(scrapperresult.json);
                romsc.romid = rom._id;
                romsc.fullpath = rom.fullpath;
                romsc.name = rom.name;
                romsc.date = rom.date;

                // arcadde game
                romsc.core = rom.core;

               
                // synchronize informations
                rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                romsc.region = rom.region;

                rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                romsc.language = rom.language;

                rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                romsc.supportType = rom.supporttype;

                // check if this video game already exist - Extremly rare
                var checkvgid = await gamedbService.GetVideoGame(videogame._id);
                if (checkvgid != null)
                {
                    bool romislinked = (from r in checkvgid.roms where r.romid == rom._id select r).Count() > 0;
                    if (!romislinked)
                    {
                        if (checkvgid.roms == null)
                            checkvgid.roms = new List<RomDb>();
                        checkvgid.roms.Add(romsc);
                        checkvgid.romscount = checkvgid.roms.Count();

                        isOk = await gamedbService.UpdateVideogameRoms(checkvgid);                       


                    }
                    //else
                    //{
                    //    var update = await gamedbService.UpdateMatchRom(rom);
                    //    foreach (var r in childroms)
                    //    {
                    //        var t = await RomActionRomForVideogame(videogame._id, r, RomAction.LINK);
                    //    }
                    //    UpdateStatistics();
                    //    return true;
                    //}
                }
                else
                {
                    // set media Reference
                    var pathtostoremedia = string.Format("{0}/{1}/{2}/", videogame.systemcategory, videogame.systemid, videogame.name);
                    videogame.media_fanart = GetMediaReference(videogame.media_fanart, "fanart", pathtostoremedia);
                    videogame.media_video = GetMediaReference(videogame.media_video, "video", pathtostoremedia);
                    videogame.media_manuel = GetMediaReference(videogame.media_manuel, "manuel", pathtostoremedia);
                    videogame.media_title = GetMediaReference(videogame.media_title, "title", pathtostoremedia);
                    videogame.media_screenshot = GetMediaReference(videogame.media_screenshot, "screenshot", pathtostoremedia);

                    videogame.roms.Add(romsc);

                    videogame.romscount = 1;
                    // Insert to database
                    isOk = await gamedbService.InsertVideogameFull(videogame);
                  
                }
            }

            // update rom has matched
            if (!isOk)
            {
                var update = await gamedbService.UpdateMatchRom(rom);
                UpdateStatistics(); 
                return false;
            }

            rom.isMatch = true;

            var updateromok = await gamedbService.UpdateMatchRom(rom);
            foreach (var r in childroms)
            {
                var t = await RomActionRomForVideogame(videogame._id, r, RomAction.LINK);
            }
            
            UpdateStatistics();
            return true;

        }

        public async Task<PlayingGame> CurrentVideogame ()
        {
            var result = new PlayingGame();
            if (gamedbService == null || misterremoteService == null)
                return result;

            var rominfo = await misterremoteService.CurrentGame();
            if (rominfo == null || rominfo.IsEmpty())
                return result;

            if (rominfo.IsArcade())
            {
                var search = new VideoGameSearchRequest()
                {
                    Core = rominfo.core
                };
                var vg = gamedbService.GetMatchVideoGame(search).FirstOrDefault();
                result.SetVideoGame(vg, GamedbService.regexformat(rominfo.core));
                if (vg!=null)
                {
                    result.systemDb = _cache.GetSystem(vg.systemid);
                }
                return result;

            }

            var consolevg = await gamedbService.SearchVideoGameByRom(rominfo.system == "Genesis" ? "MegaDrive" : rominfo.system, rominfo.gameName);
            result.SetVideoGame(consolevg, rominfo.gameName);
            if (consolevg != null)
            {
                result.systemDb = _cache.GetSystem(consolevg.systemid);
            }
            return result;


        }

        public async Task<PlayingGame> CurrentVideogame(RemoteServiceCurrentGame currentGame)
        {
            var result = new PlayingGame();
            if (gamedbService == null || misterremoteService == null)
                return result;

           
            if (currentGame == null || currentGame.IsEmpty())
                return result;

            if (currentGame.IsArcade())
            {
                var search = new VideoGameSearchRequest()
                {
                    Core = currentGame.core
                };
                var vg = gamedbService.GetMatchVideoGame(search).FirstOrDefault();
                result.SetVideoGame(vg, GamedbService.regexformat(currentGame.core));
                if (vg != null)
                {
                    result.systemDb = _cache.GetSystem(vg.systemid);
                }
                return result;

            }

            var consolevg = await gamedbService.SearchVideoGameByRom(currentGame.system == "Genesis" ? "MegaDrive" : currentGame.system, currentGame.gameName);
            result.SetVideoGame(consolevg, currentGame.gameName);
            if (consolevg != null)
            {
                result.systemDb = _cache.GetSystem(consolevg.systemid);
            }
            return result;


        }

        public async Task<List<VideoGameDb>> SearchScrapperVideoGameFromSearchName(string searchname, string systemid)
        {
            var result = new List<VideoGameDb>();
            JsonRequestResult scrapperresult;
            SystemDb? system = null;

            if (systemid == "Arcade")
            {
                scrapperresult = await scrapperService.GetVideoGameFromSearchName(searchname, 75);
            }
            else
            {
                system = await GetSystem(systemid);
                if (system == null)
                {
                    return result;
                }
                scrapperresult = await scrapperService.GetVideoGameFromSearchName(searchname, system.screenscraperId);
            }
            if (scrapperresult == null || scrapperresult.json == null || scrapperresult.json.AsArray() == null || scrapperresult.json.AsArray().Count == 0)
            {
                return result;
            }
            foreach (var jsongame in scrapperresult.json.AsArray())
            {
                var vg = scrapperService.ConvertVideogameFromJson(jsongame).CastToVideoGame();           
                if (system!=null)
                {
                    vg.systemcategory = system.category;
                    vg.systemid = system._id;
                    vg.systemname = system.name;
                }
                else
                {
                    var systemscrapperid = scrapperService.getSystemScrapperId(jsongame);
                    system = await gamedbService.GetSystemDbFromScrapperId(systemscrapperid);
                    if (system != null)
                    {
                        vg.systemcategory = system.category;
                        vg.systemid = system._id;
                        vg.systemname = system.name;
                    }
                }
                if (!string.IsNullOrEmpty(vg.name))
                    result.Add(vg);
            }
            return result;
        }

        internal bool LinkRomToConsoleVideogame (Rom rom, SystemDb system, GbdRomUpdateResult? romupdateresult = null, Action<JobLog>? log = null)
        {
            var response = scrapperService.GetVideoGameFromRom(rom.fullname, rom.size, system.screenscraperId).Result;
            rom.scrapperResult = response.HttpCode;            
            rom.responsetime = response.responsetime;
            rom.parsingexception = response.parsingException;
            if (log != null)
                log(new JobLog("SC", string.Format("Request takes {0}s and return {1}", response.responsetime, response.HttpCode), LogResult.INFO));


            if (!response.IsValid())
            {
                if (log != null)
                    log(new JobLog("SC", string.Format("Response is not valid", response.responsetime, response.HttpCode), LogResult.FAILED));

                var update = gamedbService.UpdateMatchRom(rom).Result;
                return false;
            }

                
            var videogame = scrapperService.ConvertVideogameFromJson(response.json);
            if (log != null)
                log(new JobLog("SC", string.Format("VideoGame found [{0}] {1}", videogame.screenscraperId, videogame.name), LogResult.INFO));


            var isOk = false;

            var existVideoGame = gamedbService.GetVideoGameFomScrapperId(videogame.screenscraperId).Result;
            if (existVideoGame != null)
            {// Add Rom
                bool romislinked = (from r in existVideoGame.roms where r.romid == rom._id select r).Count() > 0;
                if (!romislinked)
                {
                    var romsc = scrapperService.ConvertRomFromJson(response.json);
                    romsc.romid = rom._id;
                    romsc.fullpath = rom.fullpath;
                    romsc.name = rom.name;
                    romsc.date = rom.date;

                    //update rom
                    // synchronize informations
                    rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                    romsc.region = rom.region;

                    rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                    romsc.language = rom.language;

                    rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                    romsc.supportType = rom.supporttype;

                    if (existVideoGame.roms == null)
                        existVideoGame.roms = new List<RomDb>();
                    existVideoGame.roms.Add(romsc);
                    existVideoGame.romscount = existVideoGame.roms.Count();

                    isOk = gamedbService.UpdateVideogameRoms(existVideoGame).Result;
                    if (isOk && romupdateresult != null)
                        romupdateresult.IncVideogameUpdate();
                    if (log != null)
                        log(new JobLog("SC", string.Format("VideoGame already exist [{0}] - Rom {1} link added", existVideoGame._id, rom._id), LogResult.SUCCEED));

                }
                else
                {
                    var update = gamedbService.UpdateMatchRom(rom).Result;
                    if (romupdateresult != null)
                        romupdateresult.IncMatch();
                    if (log != null)
                        log(new JobLog("SC", string.Format("VideoGame and rom link already exist [{0}] - [{1}]", existVideoGame._id, rom._id), LogResult.SUCCEED));

                    return true;
                }


            }
            else
            { // Create VideoGame

                // Complete SystemInfo
                videogame.systemcategory = system.category;
                videogame.systemid = system._id;
                videogame.systemname = system.name;

                // set ID
                videogame.setId();

                // set link rom
                if (videogame.roms == null)
                    videogame.roms = new List<RomDb>();
                var romsc = scrapperService.ConvertRomFromJson(response.json);
                romsc.romid = rom._id;
                romsc.fullpath = rom.fullpath;
                romsc.name = rom.name;
                romsc.date = rom.date;

                //update rom
                // synchronize informations
                rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                romsc.region = rom.region;

                rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                romsc.language = rom.language;

                rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                romsc.supportType = rom.supporttype;

                // check if this video game already exist - Extremly rare
                var checkvgid = gamedbService.GetVideoGame(videogame._id).Result;
                if (checkvgid != null)
                {
                    bool romislinked = (from r in checkvgid.roms where r.romid == rom._id select r).Count() > 0;
                    if (!romislinked)
                    {
                        if (checkvgid.roms == null)
                            checkvgid.roms = new List<RomDb>();
                        checkvgid.roms.Add(romsc);
                        checkvgid.romscount = checkvgid.roms.Count();

                        isOk = gamedbService.UpdateVideogameRoms(checkvgid).Result;
                        if (isOk && romupdateresult != null)
                            romupdateresult.IncVideogameUpdate();
                        if (log != null)
                            log(new JobLog("SC", string.Format("Second check - VideoGame already exist [{0}] - Rom {1} link added", videogame._id, rom._id), LogResult.SUCCEED));

                    }
                    else
                    {
                        var update = gamedbService.UpdateMatchRom(rom).Result;
                        if (romupdateresult != null)
                            romupdateresult.IncMatch();
                        if (log != null)
                            log(new JobLog("SC", string.Format("Second check - VideoGame and rom link already exist[{0}] - [{1}]", videogame._id, rom._id), LogResult.SUCCEED));

                        return true;
                    }
                }
                else
                { 
                    // set media Reference
                    var pathtostoremedia = string.Format("{0}/{1}/{2}/", videogame.systemcategory, videogame.systemid, videogame.name);
                    videogame.media_fanart = GetMediaReference(videogame.media_fanart, "fanart", pathtostoremedia);
                    videogame.media_video = GetMediaReference(videogame.media_video, "video", pathtostoremedia);
                    videogame.media_manuel = GetMediaReference(videogame.media_manuel, "manuel", pathtostoremedia);
                    videogame.media_title = GetMediaReference(videogame.media_title, "title", pathtostoremedia);
                    videogame.media_screenshot = GetMediaReference(videogame.media_screenshot, "screenshot", pathtostoremedia);



                    videogame.roms.Add(romsc);

                    videogame.romscount = 1;
                    // Insert to database
                    isOk = gamedbService.InsertVideogameFull(videogame).Result;
                    if (isOk && romupdateresult != null)
                        romupdateresult.IncVideogameCreate();
                    if (log != null)
                        log(new JobLog("SC", string.Format("Create VideoGame and rom link [{0}]-[{1}]", videogame._id, rom._id), LogResult.SUCCEED));

                }
            }

            // update rom has matched
            if (!isOk)
            {
                var update = gamedbService.UpdateMatchRom(rom).Result;
                return false;
            }

            rom.isMatch = true;

            var updateromok = gamedbService.UpdateMatchRom(rom).Result;            
            if (updateromok && romupdateresult != null)
                romupdateresult.IncMatch();
            return updateromok;
      
        }

        internal bool LinkRomToArcadeVideogame(Rom rom, GbdRomUpdateResult? romupdateresult = null, Action<JobLog>? log = null)
        {
            if (rom.mraInfo == null || rom.mraInfo.romshierarchy.Count == 0)
            {
                if (log != null)
                    log(new JobLog("SC", "No roms hierarchy in MRA file", LogResult.FAILED));
                return false;
            }

            SystemDb? system = null;

            JsonRequestResult response = null;
            int i = rom.mraInfo.romshierarchy.Count-1;
            while ((response==null || !response.IsValid()) && i>=0)
            {
                response = scrapperService.GetVideoGameFromRom(rom.mraInfo.romshierarchy[i], rom.size, 75).Result;  // 75 : Parent screen scrapper system id for Arcade
                rom.scrapperResult = response.HttpCode;
                rom.responsetime = response.responsetime;
                rom.parsingexception = response.parsingException;

                if (log != null)
                    log(new JobLog("SC", string.Format("Request takes {0}s and return {1}", response.responsetime, response.HttpCode), LogResult.INFO));

                // check system
                if (response.IsValid())
                {
                    var scrappersystemid = scrapperService.GetSystemScrapperIdFromJsonVideogame(response.json);
                    if (scrappersystemid > 0)
                    {
                        system = _cache.GetSystems().Where(s => s.screenscraperId == scrappersystemid).FirstOrDefault();
                        if (system == null || system.category != "Arcade")
                        {
                            if (log != null)
                                log(new JobLog("SC", string.Format("Arcade system with screenscrapper id {0} not found", scrappersystemid), LogResult.FAILED));

                            response = null;
                        }
                    }
                    else
                    {
                        if (log != null)
                            log(new JobLog("SC","No system screenscrapper id found ", LogResult.FAILED));

                        response = null;
                    }

                }



                i--;

                
            }   
            
            if (response == null || !response.IsValid())
            { // No Arcade SystemFound
                rom.scrapperResult = 405;
                var update = gamedbService.UpdateMatchRom(rom).Result;
                return false;
            }

            // Update ROM systemid
            if (system==null)
            {
                log(new JobLog("SC", "Unknow case"));

            }
            rom.systemid = system!=null ? system._id : "unknow";

            if (!response.IsValid())
            {
                var update = gamedbService.UpdateMatchRom(rom).Result;
                return false;
            }


            var videogame = scrapperService.ConvertVideogameFromJson(response.json);
            if (log != null)
                log(new JobLog("SC", string.Format("VideoGame found [{0}] {1}", videogame.screenscraperId, videogame.name), LogResult.INFO));
            var isOk = false;

            var existVideoGame = gamedbService.GetVideoGameFomScrapperId(videogame.screenscraperId).Result;
            if (existVideoGame != null)
            {// Add Rom
                bool romislinked = (from r in existVideoGame.roms where r.romid == rom._id select r).Count() > 0;
                if (!romislinked)
                {
                    var romsc = scrapperService.ConvertRomFromJson(response.json);
                    romsc.romid = rom._id;
                    romsc.fullpath = rom.fullpath;
                    romsc.name = rom.name;
                    romsc.date = rom.date;
                    // only for arcade
                    romsc.core = rom.core;

                    // synchronize informations
                    rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                    romsc.region = rom.region;

                    rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                    romsc.language = rom.language;

                    rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                    romsc.supportType = rom.supporttype;

                    if (existVideoGame.roms == null)
                        existVideoGame.roms = new List<RomDb>();
                    existVideoGame.roms.Add(romsc);
                    existVideoGame.romscount = existVideoGame.roms.Count();

                    isOk = gamedbService.UpdateVideogameRoms(existVideoGame).Result;
                    if (isOk && romupdateresult != null)
                        romupdateresult.IncVideogameUpdate();
                    if (log != null)
                        log(new JobLog("SC", string.Format("VideoGame already exist [{0}] - Rom {1} link added", existVideoGame._id, rom._id), LogResult.SUCCEED));

                }
                else
                {
                    if (log != null)
                        log(new JobLog("SC", string.Format("VideoGame and rom link already exist [{0}]-[{1}]", existVideoGame._id, rom._id), LogResult.SUCCEED));

                    var update = gamedbService.UpdateMatchRom(rom).Result;
                    if (romupdateresult != null)
                        romupdateresult.IncMatch();
                    return true;
                }


            }
            else
            { // Create VideoGame

                // Complete SystemInfo
                videogame.systemcategory = system.category;
                videogame.systemid = system._id;
                videogame.systemname = system.name;


                // set ID
                videogame.setId();

                // set link rom
                if (videogame.roms == null)
                    videogame.roms = new List<RomDb>();
                var romsc = scrapperService.ConvertRomFromJson(response.json);
                romsc.romid = rom._id;
                romsc.fullpath = rom.fullpath;
                romsc.name = rom.name;
                romsc.date = rom.date;
                // only for arcade
                romsc.core = rom.core;

                // synchronize informations
                rom.region = string.IsNullOrEmpty(romsc.region) ? rom.region : romsc.region;
                romsc.region = rom.region;

                rom.language = string.IsNullOrEmpty(romsc.language) ? rom.language : romsc.language;
                romsc.language = rom.language;

                rom.supporttype = string.IsNullOrEmpty(romsc.supportType) ? rom.supporttype : romsc.supportType;
                romsc.supportType = rom.supporttype;

                // check if this video game already exist - Extremly rare
                var checkvgid = gamedbService.GetVideoGame(videogame._id).Result;
                if (checkvgid != null)
                {
                    bool romislinked = (from r in checkvgid.roms where r.romid == rom._id select r).Count() > 0;
                    if (!romislinked)
                    {
                        if (checkvgid.roms == null)
                            checkvgid.roms = new List<RomDb>();
                        checkvgid.roms.Add(romsc);
                        checkvgid.romscount = checkvgid.roms.Count();

                        isOk = gamedbService.UpdateVideogameRoms(checkvgid).Result;
                        if (isOk && romupdateresult != null)
                            romupdateresult.IncVideogameUpdate();
                        if (log != null)
                            log(new JobLog("SC", string.Format("Second check - VideoGame already exist [{0}]-[{1}]", videogame._id, rom._id), LogResult.SUCCEED));

                    }
                    else
                    {
                        var update = gamedbService.UpdateMatchRom(rom).Result;
                        if (romupdateresult != null)
                            romupdateresult.IncMatch();
                        if (log != null)
                            log(new JobLog("SC", string.Format("Second check - VideoGame and rom already exist [{0}]-[{1}]", videogame._id, rom._id), LogResult.SUCCEED));

                        return true;
                    }
                }
                else
                {
                    // set media Reference
                    var pathtostoremedia = string.Format("{0}/{1}/{2}/", videogame.systemcategory, videogame.systemid, videogame.name);
                    videogame.media_fanart = GetMediaReference(videogame.media_fanart, "fanart", pathtostoremedia);
                    videogame.media_video = GetMediaReference(videogame.media_video, "video", pathtostoremedia);
                    videogame.media_manuel = GetMediaReference(videogame.media_manuel, "manuel", pathtostoremedia);
                    videogame.media_title = GetMediaReference(videogame.media_title, "title", pathtostoremedia);
                    videogame.media_screenshot = GetMediaReference(videogame.media_screenshot, "screenshot", pathtostoremedia);



                    videogame.roms.Add(romsc);

                    videogame.romscount = 1;
                    // Insert to database
                    isOk = gamedbService.InsertVideogameFull(videogame).Result;
                    if (log != null)
                        log(new JobLog("SC", string.Format("Create VideoGame and rom link [{0}]-[{1}]", videogame._id, rom._id), LogResult.SUCCEED));

                    if (isOk && romupdateresult != null)
                        romupdateresult.IncVideogameCreate();
                }
            }

            // update rom has matched
            if (!isOk)
            {
                var update = gamedbService.UpdateMatchRom(rom).Result;
                return false;
            }

            rom.isMatch = true;

            var updateromok = gamedbService.UpdateMatchRom(rom).Result;
            if (updateromok && romupdateresult != null)
                romupdateresult.IncMatch();
            return updateromok;

        }

        internal string GetMediaReference (string? value, string type, string path)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var mediareference = InsertMedia(new MediaRequest()
            {
                targetpath = path,
                url = value,
                type = type


            }).Result;

            return mediareference;
        }


        public async Task<bool> DeleteVideoGame (string videogameid)
        {
            var videogame = await gamedbService.GetVideoGame(videogameid);
            if (videogame == null)
                return false;
            // unlink rom with videogame
            foreach (var rom in videogame.roms)
            {
               var v = await gamedbService.RomActionRomForVideogame(videogameid, rom.romid, RomAction.UNLINK);
            }
            // delete media
            await DeleteMediaWithFile(videogame.media_fanart);
            await DeleteMediaWithFile(videogame.media_manuel);
            await DeleteMediaWithFile(videogame.media_screenshot);
            await DeleteMediaWithFile(videogame.media_title);
            await DeleteMediaWithFile(videogame.media_video);

            // update system satistics
            gamedbService.videogamestatistics();

            return await gamedbService.DeleteVideoGame(videogameid);
            

        }

        public async Task<VideoGameDb?> UpdateSettingsVideogame(VideoGameDb videogame)
        {
            var result = await gamedbService.UpdateSettingsVideogame(videogame);
            return result; ;
        }

        #endregion

        #region ROM

        public async Task<Rom?> GetRom(string id)
        {
            return await gamedbService.GetRom(id);
        }

        public async Task<bool> DeleteRom(string id, bool deletefile)
        {
            var rom = await gamedbService.GetRom(id);
            if (rom.isMatch == true)
            {// remove link with videogame
                var videogame = await gamedbService.SearchVideoGameByRomId(rom._id);
                if (videogame!=null)
                {
                    var vgupdate = await RomActionRomForVideogame(videogame._id, rom._id, RomAction.UNLINK);                   
                }                
            }
            if (deletefile)
            {
                misterftpService.DeleteFile(rom.fullpath);                
            }
            var result = await gamedbService.DeleteRom(rom._id);
            return result;
        }

        public async Task<List<Rom>> GetUnmatchRoms(string category, string systemid = "")
        {
            var roms = await gamedbService.SelectUnmatchedRom(systemid, category, new List<int>(), 0);
            return roms.OrderBy(r => r.name).ToList();
        }

        public bool AutomaticLinkRomToVideoGame(string systemid, List<int> resultcodefilter)
        {
            if (_currentJob != null)
                return false;
            var t = LinkRomToVideoGame(resultcodefilter, systemid);
            return true;
        }

        internal async Task<JobMister> LinkRomToVideoGame(List<int> resultcodefilter, string systemid = "All")
        {
            var job = new JobMister();
            job.jobName = string.Format("Matching {0} Roms", systemid);
            job.jobType = JobType.MATCHINGROM;
            job.state = JobState.RUNNING;          
            _currentJob = job;
           

            if (!scrapperService.isOk)
            {
                job.AddLog("Initialize", "Screen scrapper is not setup", LogResult.FAILED);
                job.state = JobState.DONE;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;

            }

            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            var systems = new List<SystemDb>();

            if (systemid == "Arcade" || systemid == "All")
            {
                systems.Add(new SystemDb() { _id = "Arcade", name = "Arcade" });
            }
            if (systemid == "All")
            {
                systems.AddRange(_cache.GetSystems().Where(s => s.category == "Console").ToList());
            }
            if (systemid != "All" && systemid != "Arcade")
            {
                var s = _cache.GetSystem(systemid);
                if (s!=null)
                    systems.Add(s);
            }

            job.AddLog("Initialize", string.Format("{0} system{1} found", systems.Count, systems.Count>1? "s" : ""), systems.Count==0 ? LogResult.FAILED : LogResult.INFO);


            if (systems.Count == 0)
            {
                job.state = JobState.DONE;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }

            var roms = new List<Rom>();
            foreach (var system in systems)
            {
                job.AddLog("UnmatchRoms", string.Format("Select unmatch roms for {0}", system.name));
               
                
                if (system._id == "Arcade")
                {
                    roms.AddRange(await gamedbService.SelectUnmatchedRom("", "Arcade", resultcodefilter, 0));
                }
                else
                {
                    roms.AddRange(await gamedbService.SelectUnmatchedRom(system._id, "Console", resultcodefilter, 0));
                }
                job.AddLog("UnmatchRoms", string.Format("Total roms found {0}", roms.Count));

            }

            
            job.result.Initialize(roms.Count);
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);
            foreach (var r in roms)
            {
                var systemname = "Arcade";
                SystemDb? system = null;

                if (r.systemCategory != "Arcade")
                {
                    system = _cache.GetSystems().Where(s => s._id == r.systemid).FirstOrDefault();
                    if (system != null)
                    {
                        systemname = system.name;
                    }
                    else
                    {
                        systemname = "unknow";
                    }
                        
                }
                job.AddLog("SC", string.Format("Search videogame for {0} on {1}", r.name, systemname));

                if (r.systemCategory == "Arcade")
                    LinkRomToArcadeVideogame(r, job.result, job.AddLog);
                else if (system!=null)
                    LinkRomToConsoleVideogame(r, system, job.result, job.AddLog);
                job.result.Newiteration();
                job.UpdateDelay();
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
            }
            
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);
            _currentJob = null;
            UpdateStatistics();
            return job;
        }


        //public bool LaunchLinkRomToConsoleVideoGame(string systemid)
        //{
        //    if (_currentJob != null)
        //        return false;

        //    var j = LinkRomToConsoleVideoGame(systemid);
        //    return true;
        //}

        internal async Task<JobMister> LinkRomToConsoleVideoGame(string? systemid = null)
        {
            var job = new JobMister();
            job.jobName = string.Format("Scan {0} Rom", string.IsNullOrEmpty(systemid) ? "All Console" : systemid);
            job.jobType = JobType.MATCHINGROM;
            _currentJob = job;
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            var systems = new List<SystemDb>();
            if (string.IsNullOrEmpty(systemid))
                systems.AddRange(_cache.GetSystems().Where(s => s.category == "Console").ToList());
            else
            {
                var s = _cache.GetSystem(systemid);
                if (s == null)
                {
                    job.state = JobState.DONE;
                    if (OnJobRomUpdate != null)
                        OnJobRomUpdate(job);
                    _currentJob = null;
                    return job;
                }
                systems.Add(s);
            }

            foreach (var system in systems)
            {
                job.jobName = string.Format("Scan {0} Roms", system.name);
                job.state = JobState.RUNNING;
                _currentJob = job;

                var roms = await gamedbService.SelectUnmatchedRom(system._id, "Console", new List<int>(), 0);
                job.result.Initialize(roms.Count);
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                foreach (var r in roms)
                {
                    LinkRomToConsoleVideogame(r, system, job.result);
                    job.result.Newiteration();
                    job.UpdateDelay();
                    if (OnJobRomUpdate != null)
                        OnJobRomUpdate(job);
                }
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);

            }
          
            job.state = JobState.DONE;
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);
            _currentJob = null;
            gamedbService.videogamestatistics();
            return job;
        }

        public bool AutomaticScanRom(string systemid)
        {
            if (_currentJob != null)
                return false;
            var j = ScanRom(systemid);
            return true;
        }

        internal async Task<JobMister> ScanRom(string systemid = "All")
        {
            var job = new JobMister();
            job.jobName = string.Format("SCAN Roms for {0}", systemid);
            job.jobType = JobType.SCANROM;
            job.state = JobState.RUNNING;
            _currentJob = job;
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            if (!misterftpService.isOk)
            {
                job.AddLog("Initialize", "FTP is not reachable", LogResult.FAILED);
                job.state = JobState.DONE;
                _currentJob = job;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }

            var systems = new List<SystemDb>();

            if (systemid == "Arcade" || systemid == "All")
            {
                systems.Add(new SystemDb() { _id = "Arcade", name = "Arcade" });
            }
            if (systemid == "All")
            {
                systems.AddRange(_cache.GetSystems().Where(s => s.category == "Console").ToList());
            }
            if (systemid != "All" && systemid != "Arcade")
            {
                var s = _cache.GetSystem(systemid);
                if (s != null)
                    systems.Add(s);
            }
            job.AddLog("Initialize", string.Format("{0} system{1} found", systems.Count, systems.Count > 1 ? "s" : ""), systems.Count == 0 ? LogResult.FAILED : LogResult.INFO);

            if (systems.Count == 0)
            {
                job.state = JobState.DONE;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }
            
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            foreach (var system in systems)
            {

                //job.jobName = string.Format("Request FTP", system.name);

                job.AddLog("FTP", string.Format("Working on system {0}", system.name), LogResult.INFO);
                if (system._id == "Arcade")
                {
                    using (var ftp = misterftpService.CreateConnection())
                    {
                        job.AddLog("FTP", string.Format("Retrive rom from path {0}...", system.gamepath), LogResult.INFO);
                        job.UpdateDelay();
                        if (OnJobRomUpdate != null)
                            OnJobRomUpdate(job);

                        var roms = misterftpService.GetArcadeRoms(ftp);
                        job.AddLog("FTP", string.Format("Found {0} rom{1}", roms.Count, roms.Count >1 ? "s" : ""), LogResult.INFO);

                        job.result.Initialize(roms.Count);
                        foreach (var r in roms)
                        {
                            job.AddLog("MongoDb", string.Format("Update rom {0} in database", r._id), LogResult.INFO);

                            job.result += await gamedbService.UpdateRom(r);
                            job.result.Newiteration();
                            job.UpdateDelay();
                            if (OnJobRomUpdate != null)
                                OnJobRomUpdate(job);

                        }
                    }
                }
                else
                {
                    job.AddLog("FTP", string.Format("Retrive rom from path {0}...", system.gamepath), LogResult.INFO);
                    job.UpdateDelay();
                    if (OnJobRomUpdate != null)
                        OnJobRomUpdate(job);

                    var roms = ExtractConsoleRom(system);
                    job.AddLog("FTP", string.Format("Found {0} rom{1}", roms.Count, roms.Count > 1 ? "s" : ""), LogResult.INFO);

                    job.result.Initialize(roms.Count);
                    foreach (var r in roms)
                    {
                        job.AddLog("MongoDb", string.Format("Update rom {0} in database", r._id), LogResult.INFO);
                        job.result += await gamedbService.UpdateRom(r);
                        job.result.Newiteration();
                        job.UpdateDelay();
                        if (OnJobRomUpdate != null)
                            OnJobRomUpdate(job);
                    }
                }                
            }

            

            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);
            _currentJob = null;
            UpdateStatistics();
            return job;
        }

        public async Task<GbdRomUpdateResult> ScanConsoleRom(string? systemid = null)
        {
            
            if (systemid != null)
            {
                var roms = ExtractConsoleRom(_cache.GetSystem(systemid));
                return await gamedbService.UpdateRom(roms);
            }
            var result = new GbdRomUpdateResult();
            foreach (var system in _cache.GetSystems())
            {
                if (system.category == "Console")
                {
                    var roms = ExtractConsoleRom(system);
                    result += await gamedbService.UpdateRom(roms);
                }
            }

            return result;

        }


       
        //public bool LaunchLinkRomToArcadeVideoGame()
        //{
        //    if (_currentJob != null)
        //        return false;
           
        //    var j = LinkRomToArcadeVideoGame();
        //    return true;
        //}
        //internal async Task<JobMister> LinkRomToArcadeVideoGame()
        //{
        //    var job = new JobMister();
        //    job.jobName = "Matching Arcade Rom";
        //    job.jobType = JobType.MATCHINGROM;
        //    _currentJob = job;
        //    if (OnJobRomUpdate != null)
        //        OnJobRomUpdate(job);

        //    var roms = await gamedbService.SelectUnmatchedRom("", "Arcade", -1, 0);
        //    job.result.Initialize(roms.Count);
        //    foreach (var r in roms)
        //    {
        //        LinkRomToArcadeVideogame(r, job.result);
        //        job.result.Newiteration();
        //        job.UpdateDelay();
        //        if (OnJobRomUpdate != null)
        //            OnJobRomUpdate(job);
        //    }
        //    job.state = JobState.DONE;
        //    if (OnJobRomUpdate != null)
        //        OnJobRomUpdate(job);
        //    _currentJob = null;


        //    UpdateStatistics();

        //    return job;
        //}

        //public async Task<GbdRomUpdateResult> ScanArcadeRom()
        //{
        //    var result = new GbdRomUpdateResult();
        //    using (var ftp = misterftpService.CreateConnection())
        //    {
        //        var roms = misterftpService.GetArcadeRoms(ftp);
        //        result += await gamedbService.UpdateRom(roms);                
        //    }

        //    return result;
        //}

        internal List<Rom> ExtractConsoleRom(SystemDb system)
        {
            var result = new List<Rom>();
            using (var ftp = misterftpService.CreateConnection())
            {
                var availablepath = misterftpService.GetAvailableRomPath(ftp);
                result.AddRange(misterftpService.GetConsoleRoms(system, availablepath, ftp));
            }
            return result;
        }
        //internal List<Rom> ExtractConsoleRom()
        //{
        //    var result = new List<Rom>();
        //    using (var ftp = misterftpService.CreateConnection())
        //    {
        //        var availablepath = misterftpService.GetAvailableRomPath(ftp);
        //        foreach (var system in _cache.GetSystems())
        //        {
        //            result.AddRange(misterftpService.GetConsoleRoms(system, availablepath, ftp));
        //        }
        //    }
        //    return result;
        //}

        
        #endregion

        #region Media

        internal async Task<string> InsertMedia (MediaRequest request)
        {
            // Check if exist
            var media = await gamedbService.GetMediaDbBySource(request.url);
            if (media != null)
                return media._id;

            media = new MediaDb()
            {
                _id = Guid.NewGuid().ToString(),
                source = request.url,
                path = string.Format("target:{0}", request.targetpath),
                targetpath = request.targetpath,
                filename = "",
                type = request.type,
                infos = request.infos,
            };
            media.url =string.Format("https://nodered.mattab.synology.me/multimedia/{0}",media._id);

            var insertresult = await gamedbService.InserMedia(media);
            if (insertresult)
            {
                if (!request.downloadondemand)
                {
                    var downloadmedia = GetMedia(media._id);
                };                 
                return media._id;
            }

            return "";
        }

        public async Task<MediaHttp> GetMedia(string id)
        {
            var result = new MediaHttp();
            // Check if exist
            var media = await gamedbService.GetMediaDbById(id);
            if (media == null)
                return result;
            // retrocompatbilité Nodered
            media.checktargetPath();

            // Load from disk if exist
            if (media.IsDownloaded())
            {
                result.content = await mistermediaService.Load(media.GetFullpath());
                result.contentType = media.contenttype;
                result.filename = media.filename;
                if (result.content!=null)
                    return result;
            }

            // Download from source en stored on the disk
            var t = await mistermediaService.Download(media.source, media.targetpath, media.GetGuid());

            if (!t.IsSucceed)
                return result;
            media.extension = t.extension;
            media.filename = t.filename;
            media.contenttype = t.contenttype;
            media.size = t.size;
            
            var isUpdated = await gamedbService.UpdateMedia(media);

            if (isUpdated)
            {
                result.content = await mistermediaService.Load(media.GetFullpath());
                result.filename = media.filename;
                result.contentType = media.contenttype;
            }
            return result;

        }


        internal async Task<bool> DeleteMediaWithFile (string id)
        {
            if (string.IsNullOrEmpty(id))
                return true;
            var media = await gamedbService.GetMediaDbById(id);
            if (media == null)
                return true;
            if (media.IsDownloaded())
            {
                mistermediaService.DeleteFile(media.GetFullpath());
            }
            return await gamedbService.DeleteMedia(id);
        }

        #endregion

        #region System

        public bool AutomaticScanSystems()
        {
            if (_currentJob != null)
                return false;
            var j = ScanSystems();
            return true;
        }

        internal async Task<JobMister> ScanSystems()
        {
            var job = new JobMister();
            job.jobName = string.Format("SCAN Systems");
            job.jobType = JobType.SCANSYSTEM;
            job.state = JobState.RUNNING;
            _currentJob = job;
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            if (!misterremoteService.isOk)
            {
                job.AddLog("Initialize", "Mister Remote is not reachable", LogResult.FAILED);
                job.state = JobState.DONE;               
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }
            if (!scrapperService.isOk)
            {
                job.AddLog("Initialize", "Screen Scrapper is not setup", LogResult.FAILED);
                job.state = JobState.DONE;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }

            job.AddLog("MR", "Get all Console System", LogResult.INFO);
            var dbsystem = (await misterremoteService.GetSystems()).Where(s => s.category == "Console").ToList();
            var consolecount = dbsystem != null ? dbsystem.Count : 0;
            job.AddLog("MR", string.Format("{0} Console{1} found", consolecount, consolecount > 1 ? "s" : ""), LogResult.INFO);
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            job.AddLog("SC", "Get all Console System", LogResult.INFO);
            var jsonScraperConsoleSystems = scrapperService.GetAllConsoleSystemJsonMatch().Result;
            job.AddLog("SC", string.Format("{0} Console{1} found", jsonScraperConsoleSystems.Count, jsonScraperConsoleSystems.Count > 1 ? "s" : ""), LogResult.INFO);
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            job.AddLog("SC", "Get all Arcade System", LogResult.INFO);
            var jsonScraperArcadeSystems = await scrapperService.GetAllArcadeSystemJsonMatch();
            job.AddLog("SC", string.Format("{0} Arcade{1} found", jsonScraperArcadeSystems.Count, jsonScraperArcadeSystems.Count > 1 ? "s" : ""), LogResult.INFO);
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            job.UpdateDelay();
            job.result.Initialize(consolecount + jsonScraperArcadeSystems.Count);
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);

            if (job.result.Income == 0)
            {
                job.state = JobState.DONE;
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
                _currentJob = null;
                return job;
            }

            job.AddLog("Console", "Matching Console", LogResult.INFO);
            foreach (var rsSys in dbsystem)
            {
                var systemexit = gamedbService.GetSystemDb(rsSys.id).Result;
                if (systemexit == null)
                {

                    var matchsystem = scrapperService.GetConsoleSystemJsonMatch(rsSys, jsonScraperConsoleSystems);
                    if (matchsystem == null)
                    {
                        job.AddLog("Console", string.Format("[MISTER] {0}-{1} == NOMATCH", rsSys.id, rsSys.name), LogResult.FAILED);                        
                    }
                    else
                    {
                        var newsystem = scrapperService.ConvertSystemDbFromJson(rsSys, matchsystem);
                        var pathtostoremedia = string.Format("{0}/{1}/_system/", newsystem.category, newsystem._id);
                        newsystem.media_BoitierConsole3D = GetMediaReference(newsystem.media_BoitierConsole3D, "ConsoleBox", pathtostoremedia);
                        newsystem.media_controller = GetMediaReference(newsystem.media_controller, "Controller", pathtostoremedia);
                        newsystem.media_illustration = GetMediaReference(newsystem.media_illustration, "Illustration", pathtostoremedia);
                        newsystem.media_logomonochrome = GetMediaReference(newsystem.media_logomonochrome, "LogoMonochrome", pathtostoremedia);
                        newsystem.media_photo = GetMediaReference(newsystem.media_photo, "Photo", pathtostoremedia);
                        newsystem.media_video = GetMediaReference(newsystem.media_video, "Video", pathtostoremedia);
                        newsystem.media_wheel = GetMediaReference(newsystem.media_wheel, "Wheel", pathtostoremedia);

                        var insert = gamedbService.UpdateSystemDbFull(newsystem).Result;
                        job.AddLog("Console", string.Format("[MISTER] {0}-{1} == MATCH", rsSys.id, rsSys.name), LogResult.SUCCEED);                        
                    }
                }
                else
                {
                    job.AddLog("Console", string.Format("System {1} already exist with id {0}", systemexit._id, rsSys.name), LogResult.SUCCEED);
                }
                job.UpdateDelay();
                job.result.Newiteration();
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);

            }

            job.AddLog("Arcade", "Matching Arcade", LogResult.INFO);

            foreach (var system in jsonScraperArcadeSystems)
            {
                var rsSys = new RemoteServiceSystem() { category = system.Type, id = "Arcade-" + system.Names[0], name = system.Names[0] };
                var newsystem = scrapperService.ConvertSystemDbFromJson(rsSys, system);

                var pathtostoremedia = string.Format("{0}/{1}/_system/", newsystem.category, newsystem._id);
                newsystem.media_BoitierConsole3D = GetMediaReference(newsystem.media_BoitierConsole3D, "ConsoleBox", pathtostoremedia);
                newsystem.media_controller = GetMediaReference(newsystem.media_controller, "Controller", pathtostoremedia);
                newsystem.media_illustration = GetMediaReference(newsystem.media_illustration, "Illustration", pathtostoremedia);
                newsystem.media_logomonochrome = GetMediaReference(newsystem.media_logomonochrome, "LogoMonochrome", pathtostoremedia);
                newsystem.media_photo = GetMediaReference(newsystem.media_photo, "Photo", pathtostoremedia);
                newsystem.media_video = GetMediaReference(newsystem.media_video, "Video", pathtostoremedia);
                newsystem.media_wheel = GetMediaReference(newsystem.media_wheel, "Wheel", pathtostoremedia);

                var insert = gamedbService.UpdateSystemDbFull(newsystem).Result;
                job.AddLog("Arcade", string.Format("Upsert System {1} with {1}", newsystem._id, rsSys.name), insert ? LogResult.SUCCEED : LogResult.FAILED);

                job.UpdateDelay();
                job.result.Newiteration();
                if (OnJobRomUpdate != null)
                    OnJobRomUpdate(job);
            }

            UpdateStatistics();

            job.state = JobState.DONE;
            job.UpdateDelay();
            if (OnJobRomUpdate != null)
                OnJobRomUpdate(job);
            _currentJob = null;
            return job;
        }

        public void MatchAllConsoleSystemWithScrapper ()
        {
            var dbsystem = this.misterremoteService.GetSystems().Result.Where(s => s.category == "Console");
            var oklist = new StringBuilder();
            var kolist = new StringBuilder();

            var jsonScraperSystems = scrapperService.GetAllConsoleSystemJsonMatch().Result;

            foreach (var rsSys in dbsystem)
            {                

                var systemexit = gamedbService.GetSystemDb(rsSys.id).Result;
                if (systemexit == null)
                {

                    var matchsystem = scrapperService.GetConsoleSystemJsonMatch(rsSys, jsonScraperSystems);
                    if (matchsystem == null)
                    {
                        
                        kolist.AppendLine(string.Format("[MISTER] {0}-{1} == NOMATCH", rsSys.id, rsSys.name));
                    }
                    else
                    {
                        var newsystem = scrapperService.ConvertSystemDbFromJson(rsSys, matchsystem);
                        var pathtostoremedia = string.Format("{0}/{1}/_system/", newsystem.category, newsystem._id);
                        newsystem.media_BoitierConsole3D = GetMediaReference(newsystem.media_BoitierConsole3D, "ConsoleBox", pathtostoremedia);
                        newsystem.media_controller = GetMediaReference(newsystem.media_controller, "Controller", pathtostoremedia);
                        newsystem.media_illustration = GetMediaReference(newsystem.media_illustration, "Illustration", pathtostoremedia);
                        newsystem.media_logomonochrome = GetMediaReference(newsystem.media_logomonochrome, "LogoMonochrome", pathtostoremedia);
                        newsystem.media_photo = GetMediaReference(newsystem.media_photo, "Photo", pathtostoremedia);
                        newsystem.media_video = GetMediaReference(newsystem.media_video, "Video", pathtostoremedia);
                        newsystem.media_wheel = GetMediaReference(newsystem.media_wheel, "Wheel", pathtostoremedia);



                        var insert = gamedbService.UpdateSystemDbFull(newsystem).Result;
                        oklist.AppendLine(string.Format("[MISTER] {0}-{1} == [SCRAPPER] {2}", rsSys.id, rsSys.name, string.Join(", ", matchsystem.Names.Take(3))));
                    }
                }
            }
           

            return;


        }

        public void MatchAllArcadeSystemWithScrapper()
        {
            var jsonScraperSystems = scrapperService.GetAllArcadeSystemJsonMatch().Result;
            foreach (var system in jsonScraperSystems)
            {
                var rsSys = new RemoteServiceSystem() { category = system.Type, id = "Arcade-" + system.Names[0], name = system.Names[0] };
                var newsystem = scrapperService.ConvertSystemDbFromJson(rsSys, system);

                var pathtostoremedia = string.Format("{0}/{1}/_system/", newsystem.category, newsystem._id);
                newsystem.media_BoitierConsole3D = GetMediaReference(newsystem.media_BoitierConsole3D, "ConsoleBox", pathtostoremedia);
                newsystem.media_controller = GetMediaReference(newsystem.media_controller, "Controller", pathtostoremedia);
                newsystem.media_illustration = GetMediaReference(newsystem.media_illustration, "Illustration", pathtostoremedia);
                newsystem.media_logomonochrome = GetMediaReference(newsystem.media_logomonochrome, "LogoMonochrome", pathtostoremedia);
                newsystem.media_photo = GetMediaReference(newsystem.media_photo, "Photo", pathtostoremedia);
                newsystem.media_video = GetMediaReference(newsystem.media_video, "Video", pathtostoremedia);
                newsystem.media_wheel = GetMediaReference(newsystem.media_wheel, "Wheel", pathtostoremedia);



                var insert = gamedbService.UpdateSystemDbFull(newsystem).Result;
            }

        }

        public async Task<SystemDb?> UpdateSettingsSystem (SystemDb system)
        {
            var result = await gamedbService.UpdateSettingsSystem(system);
            // refreshcache
            var s = await gamedbService.GetAllSystems();
            foreach (var m in s) { _cache.StoreSystems(m); };
            _cache.LastUpdate = DateTime.Now;

            return result;
        }

        public async Task<List<ItemCount>> GetSystemsFilter (string filter)
        {
            var result = new List<ItemCount>();
            switch (filter)
            {
                case "unmatchrom":
                    result = await gamedbService.GetSystemsWithUnmatchRoms();
                    break;
                case "allrom":
                    result = await gamedbService.GetSystemsWithTotalRoms();
                    break;
            }           
            return result;           
        }


        #endregion

        #region Core

        public async Task<bool> SendKeyboardCmd(List<string> cmds, bool raw, int delay)
        {

            var result = await misterremoteService.KeyboardCommand(cmds, raw, delay);
            return result;
        }

        public async Task<List<CoreSaveState>> GetSaveAllState (string videogameid, string romid)
        {
            var list = await gamedbService.GetCoreSaveState(videogameid, romid);
            var result = new List<CoreSaveState>();
            foreach (var item in list)
            {
                while (item.slot > result.Count+1)
                {
                    var emptysavestate = new CoreSaveState { videogameid = videogameid, romid = romid, slot = result.Count + 1 };
                    emptysavestate.setId();
                    result.Add(emptysavestate);
                }
                result.Add(item);
            }
           while (result.Count < 4)
           {
                var emptysavestate = new CoreSaveState { videogameid = videogameid, romid = romid, slot = result.Count + 1 };
                emptysavestate.setId();
                result.Add(emptysavestate);
           }

            return result;

        }

        public async Task<List<CoreSaveState>?> SaveStateCmdSave(string videogameid, string romid, int slot)
        {
            var currentgame = await CurrentVideogame();

            // Check videogame is still running
            if (!currentgame.IsPlaying || currentgame.currentVideogame == null || currentgame.playingRomdb == null || currentgame.currentVideogame._id != videogameid || currentgame.playingRomdb.romid != romid)
                return null;

           
            // Take screeenshoot
            var screenshootok = await misterremoteService.TakeScreenshot();
            if (!screenshootok)
                return await GetSaveAllState(videogameid, romid);

            var statescreenshots = (await misterremoteService.GetAllScreenshots()).OrderByDescending(s => s.modified).FirstOrDefault();

            if (statescreenshots == null)
                return await GetSaveAllState(videogameid, romid);

            // Launch command
            Task.Run(() => misterremoteService.CmdSaveState(slot));

            var idmedia = await InsertMedia(new MediaRequest()
            {
                format = "screenshot",
                targetpath = string.Format("savestate/{0}/{1}/{2}", currentgame.systemDb._id, videogameid, romid),
                type = "image",
                url = misterremoteService.ScreenshootUrl(statescreenshots),
                downloadondemand = false

            });

            var savestate = new CoreSaveState()
            {
                videogameid = videogameid,
                romid = romid,
                slot = slot
            };
            savestate.setId();

            var existsavestate = await gamedbService.GetCoreSaveState(savestate._id);
            if (existsavestate == null)
            {
                savestate.Modified = statescreenshots.modified;
                savestate.filename = statescreenshots.filename;
                savestate.core = statescreenshots.core;
                savestate.systemid = currentgame.currentVideogame.systemid;
                savestate.mediaId = idmedia;
                var updateresult = await gamedbService.UpdateCoreSaveState(savestate);
                if (updateresult == null || updateresult == false)
                    return null;                
            }
            else
            {
                // delete old screenshot
                var deleteresult = await misterremoteService.DeleteScreenshot(new RemoteScreenshotsInfo() { core = existsavestate.core, filename = existsavestate.filename });
                if (!deleteresult)
                    return null;

                existsavestate.mediaId = idmedia;
                existsavestate.filename = statescreenshots.filename;
                existsavestate.Modified = statescreenshots.modified;
                var updateresult = await gamedbService.UpdateCoreSaveState(existsavestate);
                if (updateresult == null || updateresult == false)
                    return null;                
            }


            // return all savestates
            return await GetSaveAllState(videogameid, romid);

        }

        public async Task<List<CoreSaveState>?> SaveStateCmdLoad(string videogameid, string romid, int slot)
        {
            if (slot < 0 || slot > 4)
                return null;

            var currentgame = await CurrentVideogame();

            if (!currentgame.IsPlaying)
                return null;

            if (currentgame.currentVideogame._id != videogameid || currentgame.playingRomdb.romid != romid)
            {
                return null;
            }

            var savestate = new CoreSaveState()
            {
                videogameid = currentgame.currentVideogame._id,
                romid = currentgame.playingRomdb.romid,
                slot = slot
            };
            savestate.setId();

            var existsavestate = await gamedbService.GetCoreSaveState(savestate._id);

            if (existsavestate == null)
                return null;

            misterremoteService.CmdLoadState(existsavestate.slot);
            return await GetSaveAllState(videogameid, romid);
        }

        public async Task<List<ModuleSetting>?> GetModuleSettings (string modulename)
        {
            var settings = await gamedbService.GetModuleSettings(modulename);
            if (settings == null || settings.Count==0)
            {
                switch (modulename)
                {
                    case "MisterFtp":
                        var fs = new FtpServiceSettings();
                        settings = fs.GetModuleSettings();
                        break;
                    case "MisterMedia":
                        var ms = new MediaServiceSettings();
                        settings = ms.GetModuleSettings();
                        break;
                    case "MisterRemote":
                        var rs = new RemoteServiceSettings();
                        settings = rs.GetModuleSettings();
                        break;
                    case "ScreenScrapper":
                        var ss = new ScrapperScServiceSettings();
                        settings = ss.GetModuleSettings();
                        break;

                }
            }
            return settings;

        }

        public async Task<bool> SetModuleSettings(List<ModuleSetting> settings)
        {
            if (settings == null || settings.Count == 0)
            { return false; }

            var result = await gamedbService.SetModuleSettings(settings);
            if (result)
            { // Reload module
                var modulename = settings[0].moduleName;
                switch (modulename)
                {
                    case "MisterFtp":
                        loadModule<MisterFtpService, FtpServiceSettings>();
                        break;
                    case "MisterMedia":
                        loadModule<MisterMediaService, MediaServiceSettings>();
                        break;
                        
                    case "MisterRemote":
                        loadModule<MisterRemoteService, RemoteServiceSettings>();
                        break;
                    case "ScreenScrapper":
                        loadModule<ScrapperScService, ScrapperScServiceSettings>();
                        break;
                    case "MisterAuth":
                        loadModule<MisterAuthService, AuthServiceSettings>();
                        break;
                }
                RefreshCache();
                if (OnCacheUpdated != null)
                    OnCacheUpdated();

            }
            return result;
        }

        

        public async Task<MisterModuleHealthCheck?> CheckModuleSettings (List<ModuleSetting> settings)
        {
            if (settings == null || settings.Count==0)
                { return null; }

            var modulename = settings[0].moduleName;
            switch (modulename)
            {
                case "MisterFtp":
                    var fs = new FtpServiceSettings();
                    fs.LoadModuleSettings(settings);
                    var fsmod = new MisterFtpService(fs);
                    return fsmod.CheckHealth();                   
                case "MisterMedia":
                    var ms = new MediaServiceSettings();
                    ms.LoadModuleSettings(settings);
                    var msmod = new MisterMediaService(ms);
                    return msmod.CheckHealth();
                case "MisterRemote":
                    var rs = new RemoteServiceSettings();
                    rs.LoadModuleSettings(settings);
                    var rsmod = new MisterRemoteService(rs);
                    return rsmod.CheckHealth();
                case "ScreenScrapper":
                    var ss = new ScrapperScServiceSettings();
                    ss.LoadModuleSettings(settings);
                    var ssmod = new ScrapperScService(ss);
                    return ssmod.CheckHealth();
                case "MisterAuth":
                    var os = new AuthServiceSettings();
                    os.LoadModuleSettings(settings);
                    var osmod = new MisterAuthService(os);
                    return osmod.CheckHealth();
            }
            return null;
        }

        internal void UpdateStatistics()
        {
            Task.Run(() =>
            {

                gamedbService.videogamestatistics();
                RefreshCache();
                if (OnCacheUpdated != null)
                    OnCacheUpdated();
            });
        }

        public JobMister? CurrentJob ()
        {
            return _currentJob;
        }

        public async Task<RemoteServiceScriptResult> GetScripts()
        {
            return await misterremoteService.GetScript();
        }

        public async Task<Boolean> ExecuteScript(string name, bool force=false)
        {
            var scripts = await misterremoteService.GetScript();
            
            if (!scripts.canLaunch && !force)
            {
                return false;
            }
            var script = scripts.scripts.Where(g => g.Compare(name)).FirstOrDefault();
            if (script.isEmpty())
                return false;

            if (scripts.canLaunch)
            {
                return await misterremoteService.ExecuteScript(script.filename);
            }

            else
            {
                Task.Run( () =>
                {
                    this.SendKeyboardCmd(new List<string>() { "reset" }, false, 0);
                    Thread.Sleep(2000);
                    var t = misterremoteService.ExecuteScript(script.filename).Result;
                });
                
            }
            return true;


        }


        #endregion

        #region Auth
        public bool AuthorisationIsrequired()
        {
            return authService != null && authService.Health.MisterState == MisterStateEnum.OK;
        }

        public GuestAccess? GenerateGuessAccess (string signature)
        {
            return authService.GenerateGuestAccess(signature);
        }

        public List<GuestAccess> GetCurrentGuestAccess()
        {
            return authService.GetCurrentGuestAccess();
        }

        public bool ApprouvedGuestAccess (bool approuved, string code)
        {
            return authService.GuessAccessAdminApprouve(approuved, code);
        }

        public GuestAccessState GuestAccessState(string code)
        {
            return authService.GuestAccessState(code);
        }

        public bool ConsumedGuestAccess(string code, string key)
        {
            return authService.GuestAccessClientConsumed(code, key);
        }

        public bool CheckAdminPassword (string password)
        {
            return authService.CheckAdminPassword(password);
        }

        public int GetTokenDelay(TokenType type)
        {
            return authService.GetTokenDelay(type);
        }
    


        #endregion

    }


}
