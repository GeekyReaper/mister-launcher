using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using Amazon.Util;

namespace libMisterLauncher.Service
{
    public class ScrapperScServiceSettings : IMisterSettings
    {
        const string _moduleName = "ScreenScrapper";
        public string ModuleName { get { return _moduleName; } }
        public string host { get; set; } = "https://api.screenscraper.fr/api2/";
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public string softname { get; set; } = "MiSTerLauncher";

        public List<string> _preferedRegion = new List<string> { "fr", "eu", "ss", "wor", "us", "jp" };
        public List<string> _preferedLanguage = new List<string> { "fr", "en", "ja" };
        public List<string> systemidAllowSaveState = new List<string> { "NES", "AtariLynx", "Gameboy", "GBA", "PSX" };
        public List<string> systemidAllowSaveMemory = new List<string> { "MegaDrive", "Gameboy", "SNES", "N64", "Saturn", "SuperGrafx", "TurboGrafx16", "TurboGrafx16CD" };


        public bool isValid()
        {
            return !string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

       
        public void LoadModuleSettings(List<ModuleSetting> moduleSettings)
        {
            foreach (var moduleSetting in moduleSettings)
            {
                switch (moduleSetting.name)
                {
                    case "host":
                        host = moduleSetting.value;
                        break;
                    case "username":
                        username = moduleSetting.value;
                        break;
                    case "password":
                        password = moduleSetting.value;
                        break;
                    case "softname":
                        softname = moduleSetting.value;
                        break;
                    case "preferedRegion":
                        _preferedRegion = moduleSetting.value.Split(',').ToList();
                        break;
                    case "systemidallowsavestate":
                        systemidAllowSaveState = moduleSetting.value.Split(',').ToList();
                        break;
                    case "systemidallowsavememory":
                        systemidAllowSaveMemory = moduleSetting.value.Split(',').ToList();
                        break;
                }


            }
        }        

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "host",
                    value = host,
                    valueType = "url",
                    description = "ScreenScrapper API. Default 'https://api.screenscraper.fr/api2/'",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "username",
                    value = username,
                    description = "ScreenScraper Dev Account. Visit https://screenscraper.fr to create an account.",
                    valueType = "text",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "password",
                    value = password,
                    description = "ScreenScraper Dev Account. Visit https://screenscraper.fr to create an account.",
                    valueType = "password",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "softname",
                    value = softname,
                    description = "Value used to identify screenscraper request from. Default value 'MiSTerLauncher'",
                    valueType = "text",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "preferedRegion",
                    value = string.Join(',',_preferedRegion),
                    description = "Enter value separated by ',' with no space. This value used to extract your prefered content from ScreenScraper database",
                    valueType = "text",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "preferedLanguage",
                    value = string.Join(',',_preferedLanguage),
                    description = "Enter value separated by ',' with no space. This value used to extract your prefered content from ScreenScraper database",
                    valueType = "text",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "systemidallowsavestate",
                    value = string.Join(',',systemidAllowSaveState),
                    description = "Enter value separated by ',' with no space. List of system name which allow savestate. It is used during system extract.",
                    valueType = "text",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "systemidallowsavememory",
                    value = string.Join(',',systemidAllowSaveMemory),
                    description = "Enter value separated by ',' with no space. List of system name which allow memorysave¨. Please check you activate 'autosaving'. It is used during system extract.",
                    valueType = "text",
                    update = DateTime.Now
                }
            };
            foreach (var item in result)
                item.SetId();
            return result;
        }
    }


    public class ScrapperScService : BaseModule<ScrapperScServiceSettings>
    {
        public List<ScGameType> GameTypes { get; set; } = new List<ScGameType>();

        public ScrapperScService() : base()
        {
        }
        public ScrapperScService(ScrapperScServiceSettings? settings=null) : base(settings)
        {
            Task.Run(() =>
            {
                GameTypes = GetGameTypeFromScrapper().OrderBy(i => i.Name).ToList();                
            });
        }

        public override bool CheckConnection()
        {
            var hoststate = GetRequest("ssinfraInfos.php").Result;
            return hoststate != null && hoststate.IsValid();         
        }

        public override MisterModuleHealthCheck CheckHealth()
        {
            // Not necessary to check at every refresh
            if (_health.MisterState == MisterStateEnum.OK)
                return _health;
            
            return base.CheckHealth();
        }

        public bool GameTypeLoaded ()
        {
            return GameTypes.Count > 0;
        }
        
        public List<string> GetGameType(bool parentOnly)
        {
            return (from i in GameTypes where i.Parent == 0 || !parentOnly select i.Name).ToList();
        }

        internal string GetUrlParameters()
        {
            return string.Format("devid={0}&devpassword={1}&softname={2}&output=json", _settings.username, _settings.password, _settings.softname);
        }

        internal async Task<JsonRequestResult> GetRequest(string action, string parameters = "")
        {
            var result = new JsonRequestResult();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_settings.host);
                client.DefaultRequestHeaders.Add("User-Agent", "MisterLauncher");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Stopwatch watch = new Stopwatch();
                watch.Start();
                var response = await client.GetAsync(action + "?" + GetUrlParameters() + (string.IsNullOrEmpty(parameters) ? "" : "&" + parameters));
                watch.Stop();
                result.responsetime = (int)watch.Elapsed.TotalMilliseconds;
                result.HttpCode = (int)response.StatusCode;
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return result;                               
                var jsonData = await response.Content.ReadAsStringAsync();
                try
                {
                    result.json = JsonNode.Parse(jsonData);
                }
                catch (Exception ex)
                {
                    result.parsingException = ex.Message;
                }
                return result;

            }
        }

        public async Task<JsonRequestResult?> GetSystems()
        {
            var result =  await GetRequest("systemesListe.php");
            result.MoveTo("response.systemes");
            return result;
        }


        public async Task<List<RemoteScvSystemMatch>> GetAllConsoleSystemJsonMatch ()
        {
            var result = new List<RemoteScvSystemMatch>();
            var systemsresult = await GetSystems();
            if (systemsresult == null || !systemsresult.IsValid())
                return result;
                       
            foreach (JsonObject jsys in systemsresult.json.AsArray())
            {
                var type = getJsonValue(jsys, "type", "", s => s);

                if (!string.IsNullOrEmpty(type) && (type == "Console" || type == "Console Portable" || type== "Console & Arcade" || type=="Accessoire"))
                {
                    
                    var systemitem = new RemoteScvSystemMatch();
                    systemitem.Type = type;

                    var attributesname = new List<string>() { "noms.nom_eu", "noms.nom_jp", "noms.nom_us", "noms.nom_recalbox", "noms.nom_retropie", "noms.nom_launchbox", "noms.nom_hyperspin" };

                    foreach (var attribute in attributesname)
                    {
                        var n = getJsonValue(jsys, attribute, "", s => s);
                        if (!string.IsNullOrEmpty(n))
                            systemitem.Names.Add(Tools.RemoveSpecialCaracteres(n));
                    }
                    var noms_commun = getJsonValue(jsys, "noms.noms_commun", "", s => s);
                    if (!string.IsNullOrEmpty(noms_commun))
                    {
                        systemitem.Names.AddRange(noms_commun.Split(',').Select(s => Tools.RemoveSpecialCaracteres(s)));
                    }

                    systemitem.json = jsys;
                    result.Add(systemitem);
                }

            }
            return result;
        }

        public async Task<List<RemoteScvSystemMatch>> GetAllArcadeSystemJsonMatch()
        {
            var result = new List<RemoteScvSystemMatch>();
            var systemsresult = await GetSystems();
            if (systemsresult == null || !systemsresult.IsValid())
                return result;

            foreach (JsonObject jsys in systemsresult.json.AsArray())
            {
                var supporttype = getJsonValue(jsys, "supporttype", "", s => s);

                if (!string.IsNullOrEmpty(supporttype) && (supporttype == "pcb"))
                {

                    var systemitem = new RemoteScvSystemMatch();
                    systemitem.Type = "Arcade";

                    var attributesname = new List<string>() { "noms.nom_eu", "noms.nom_jp", "noms.nom_us", "noms.nom_recalbox", "noms.nom_retropie", "noms.nom_launchbox", "noms.nom_hyperspin" };

                    foreach (var attribute in attributesname)
                    {
                        var n = getJsonValue(jsys, attribute, "", s => s);
                        if (!string.IsNullOrEmpty(n))
                            systemitem.Names.Add(n);
                    }
                    var noms_commun = getJsonValue(jsys, "noms.noms_commun", "", s => s);
                    if (!string.IsNullOrEmpty(noms_commun))
                    {
                        systemitem.Names.AddRange(noms_commun.Split(',').Select(s => s));
                    }

                    systemitem.json = jsys;
                    result.Add(systemitem);
                }

            }
            return result;
        }

        public RemoteScvSystemMatch? GetConsoleSystemJsonMatch(RemoteServiceSystem rsSys, List<RemoteScvSystemMatch> systems)
        {
            var names = new List<string>
            {
                rsSys.id,
                rsSys.name
            };

            if (names.Contains("TurboGrafx-16 CD"))
                names.Add("PC Engine CD-Rom");
            if (names.Contains("SuperVision"))
                names.Add("Watara Supervision");


            foreach (var s in systems)
            {
                foreach (var systemname in s.Names)
                {
                    if (names.Exists(s => Tools.RemoveSpecialCaracteres(s) == systemname))
                        return s;
                }
            }
            return null;
        }

        public SystemDbFull ConvertSystemDbFromJson (RemoteServiceSystem rmsystem, RemoteScvSystemMatch jsonSystem)
        {
            var result = new SystemDbFull();
            result._id = rmsystem.id;
            result.name = rmsystem.name;
            result.category = rmsystem.category;
            result.gamepath = rmsystem.id;

            // FIX
            if (result.name == "Neo Geo CD")
            {
                result._id = "NeoGeoCD";
            }
            if (result.name == "Genesis")
            {
                result.name = "MegaDrive";
                result._id = "MegaDrive";
                result.gamepath = rmsystem.id;
            }
            switch (result.gamepath)
            {
                case "Nintendo64":
                    result.gamepath = "N64";
                    break;
                case "SuperGrafx":
                    result.gamepath = "TGFX16/SuperGrafx";
                    break;
                case "TurboGrafx16":
                    result.gamepath = "TGFX16/PC Engine";
                    break;
                case "TurboGrafx16CD":
                    result.gamepath = "TGFX16-CD";
                    break;
            }

            // PARSE

            //        "extensions" : "{{{system.sc.extensions}}}",
            //"company" : "{{{system.sc.compagnie}}}",
            //"type" : "{{{system.sc.type}}}",
            //"supporttype" : "{{{system.sc.supporttype}}}",
            //"romtype" : "{{{system.sc.romtype}}}",
            //"name_eu" : "{{{system.sc.noms.nom_eu}}}",
            //"name_jp" : "{{{system.sc.noms.nom_jp}}}",
            //"name_us" : "{{{system.sc.noms.nom_us}}}",
            
            result.startdate = Tools.ParseDate(getJsonValue(jsonSystem.json, "datedebut", "", s => s));
            result.startyear = result.startdate.Year;
            result.enddate = Tools.ParseDate(getJsonValue(jsonSystem.json, "datefin", "", s => s));
            result.endyear = result.enddate.Year;

            
            result.generation = 0;
            result.abreviation = "";
            result.alternative_name = "";
            result.family = "";

            result.extensions = getJsonValue(jsonSystem.json, "extensions", "", s => s);
            result.company = getJsonValue(jsonSystem.json, "compagnie", "", s => s);
            result.systemtype = getJsonValue(jsonSystem.json, "type", "", s => s);
            result.supporttype = getJsonValue(jsonSystem.json, "supporttype", "", s => s);
            result.romtype = getJsonValue(jsonSystem.json, "romtype", "", s => s);
            result.name_eu = getJsonValue(jsonSystem.json, "noms.nom_eu", "", s => s);
            result.name_jp = getJsonValue(jsonSystem.json, "noms.nom_jp", "", s => s);
            result.name_us = getJsonValue(jsonSystem.json, "noms.nom_us", "", s => s);
            result.screenscraperId = getJsonValue(jsonSystem.json, "id", 0, s => int.Parse(s));


            result.media_BoitierConsole3D = getJsonValue(GetMedia(jsonSystem.json, "BoitierConsole3D"), "url", "", s => s);
            result.media_controller = getJsonValue(GetMedia(jsonSystem.json, "controller"), "url", "", s => s);
            result.media_illustration = getJsonValue(GetMedia(jsonSystem.json, "illustration"), "url", "", s => s);
            result.media_logomonochrome = getJsonValue(GetMedia(jsonSystem.json, "logo-monochrome"), "url", "", s => s);
            result.media_photo = getJsonValue(GetMedia(jsonSystem.json, "photo"), "url", "", s => s);
            result.media_video = getJsonValue(GetMedia(jsonSystem.json, "video"), "url", "", s => s);
            result.media_wheel = getJsonValue(GetMedia(jsonSystem.json, "wheel"), "url", "", s => s);



            result.match = true;

            if (result.supporttype == "cd")
            {
                result.extensions = string.Join(',', result.extensions.Split(',').Where(e => e.ToLower() != "bin").ToList());
            }
            if (result._id == "NeoGeo")
            {
                result.extensions = "neo,chd,zip";
            }
            if (result._id == "SuperGrafx")
            {
                result.extensions = "sgx," + string.Join(',', result.extensions.Split(',').Where(e => e.ToLower() != "sgx").ToList());               
            }

            result.allowSaveMemory = _settings.systemidAllowSaveMemory.Contains(result._id);
            result.allowSaveStates = _settings.systemidAllowSaveState.Contains(result._id);


            result.unused = UnusedSystemJson(jsonSystem.json);


           


            return result;
        }

        public int GetSystemScrapperIdFromJsonVideogame(JsonNode json)
        {
            return getJsonValue(json, "systeme.id", 0, s => int.Parse(s));
        }

        internal List<ScGameType> GetGameTypeFromScrapper ()
        {
            var result = new List<ScGameType>();
            var json = GetAllGameType().Result;
            if (json.IsValid())
            {
                result.Clear();
                foreach (var item in json.json.AsObject())
                {
                    var t = new ScGameType()
                    {
                        Id = getJsonValue(item.Value, "id", 0, s => int.Parse(s)),
                        Name = getJsonValue(item.Value, "nom_fr", "", s => s),
                        Parent = getJsonValue(item.Value, "parent", 0, s => int.Parse(s))
                    };
                    result.Add(t);
                }
                    
            }
            return result;

        }

        internal async Task<JsonRequestResult> GetAllGameType()
        {
           
            var result = await GetRequest("genresListe.php");
            result.MoveTo("response.genres");
            return result;
        }

        public async Task<JsonRequestResult> GetVideoGameFromRom(string romname, long size, int scSystem_id=0)
        {
           
            var p = string.Format("romnom={0}&romtaille={1}", System.Web.HttpUtility.UrlEncode(romname), size);
            if (scSystem_id != 0)
            {
                p += string.Format("&systemeid={0}", scSystem_id);
            }

            var result = await GetRequest("jeuInfos.php", p);
            result.MoveTo("response.jeu");
            return result;

        }

        public async Task<JsonRequestResult> GetVideoGameFromSearchName (string searchname, int scSystem_id)
        {
            var p = string.Format("recherche={0}&systemeid={1}", System.Web.HttpUtility.UrlEncode(searchname), scSystem_id);
            var result = await GetRequest("jeuRecherche.php", p);
            result.MoveTo("response.jeux");
            return result;
        }
        public async Task<JsonRequestResult> GetVideoGameFromId(int screenscraperid)
        {

            var p = string.Format("gameid={0}", screenscraperid);           

            var result = await GetRequest("jeuInfos.php", p);
            result.MoveTo("response.jeu");
            return result;

        }

        public int getSystemScrapperId (JsonNode json)
        {
            return getJsonValue(json, "systeme.id", 0, s => int.Parse(s));
        }

        internal VideoGameFullDb ConvertVideogameFromJson(JsonNode json)
        {
            var name = GetPreferedRegionValue(_settings._preferedRegion, json!["noms"]!, "region", "text");
            var desc = GetPreferedLanguageValue(_settings._preferedLanguage, json!["synopsis"]!, "langue", "text");
            DateTime gamedate = DateTime.MinValue;
            string dateregion = "";
            if (json!["dates"] != null)
            {
                var date = GetPreferedRegionValue(_settings._preferedRegion, json!["dates"]!, "region", "text");
                dateregion = date.region;
                gamedate = Tools.ParseDate(date.value);
            }

            
            var json_media_sstitle = GetMedia(json, "sstitle");
            var json_media_ss = GetMedia(json, "ss");
            var json_media_fanart = GetMedia(json, "fanart");
            var json_media_video = GetMedia(json, "video");
            var json_media_manuel = GetMedia(json, "manuel");



            var gametype = new List<string>();
            if (json["genres"] != null)
            {

                foreach (var genre in json["genres"].AsArray())
                {
                    var r = GetPreferedLanguageValue(_settings._preferedLanguage, genre["noms"], "langue", "text");
                    gametype.Add(r.value);
                }
            }

            var famille = json!["familles"]?[0];

            var collectionname = "";
            if (famille != null && famille["noms"] != null)
                collectionname = GetPreferedLanguageValue(_settings._preferedLanguage, famille["noms"], "langue", "text").value;


            var result = new VideoGameFullDb
            {
                screenscraperId = getJsonValue(json, "id", 0, s => int.Parse(s)),
                name = name.value,
                name_region = name.region,
                editorname = getJsonValue(json, "editeur.text", "", s => s),
                developname = getJsonValue(json, "developpeur.text", "", s => s),
                nbplayers = getJsonValue(json, "joueurs.text", "", s => s),
                rating = getJsonValue(json, "note.text", 0, s => int.Parse(s)),
                desc = desc.value,
                desc_lang = desc.language,
                gamedate = gamedate,
                date_region = dateregion,
                year = gamedate.Year,
                gametype = gametype,
                collectionId = famille != null ? getJsonValue(json, "id", 0, s => int.Parse(s)) : 0,
                collection = collectionname,
                names = TransformRegion(json!["noms"], "region", "text"),
                dates = TransformRegion(json!["dates"], "region", "text"),
                romscount = 0,
                namesearch = aggregateValueforSearch(json!["noms"], "text"),
                unused = UnusedJson(json),
                media_title = getJsonValue(json_media_sstitle, "url", "", s => s),
                media_screenshot = getJsonValue(json_media_ss, "url", "", s => s),
                media_video = getJsonValue(json_media_video, "url", "", s => s),
                media_fanart = getJsonValue(json_media_fanart, "url", "", s => s),
                media_manuel = getJsonValue(json_media_manuel, "url", "", s => s)
            };

        
            return result;
        }

        internal RomDb ConvertRomFromJson(JsonNode json)
        {
            var rom = new RomDb()
            {
                region = getJsonValue(json, "rom.romregions", "", s => s),
                language = getJsonValue(json, "rom.romlangues", "", s => s),
                supportType = getJsonValue(json, "rom.romsupporttype", "", s => s),
                screenscrapperId = getJsonValue(json, "rom.id", 0, s => int.Parse(s)),
                size = getJsonValue(json, "rom.size", 0, s => long.Parse(s))
            };
            return rom;
        }

        internal T getJsonValue<T>(JsonNode? json, string attributes, T defaultvalue, Func<string, T> castfromString)
        {
            if (json == null)
                return defaultvalue;
            var att = attributes.Split('.');
            var j = json;
            foreach (var a in att)
            {
                if (j[a] == null)
                    return defaultvalue;
                j = j[a];
            }

            try
            {
                return castfromString(j.AsValue().ToString());
            }
            catch (Exception ex)
            {
                return defaultvalue;
            }
        }

        internal RegionInformation GetPreferedRegionValue(List<string> preferedRegion, JsonNode json, string regionAttribute, string valueAttribute)
        {
            var result = new RegionInformation();
            if (json == null || json.AsArray() == null)
                return result;
            var array = json.AsArray();
            foreach (var region in preferedRegion)
            {
                int i = 0;
                while (result.isEmpty && i < array.Count)
                {
                    if (getJsonValue(json[i], regionAttribute, "", s => s) == region)
                    {
                        result.value = getJsonValue(json[i], valueAttribute, "", s => s);
                        result.region = getJsonValue(json[i], regionAttribute, "", s => s);
                    }
                    i++;
                }
                if (!result.isEmpty)
                    return result;
            }

            result.value = getJsonValue(json[0], valueAttribute, "", s => s);
            result.region = getJsonValue(json[0], regionAttribute, "", s => s);

            return result;
        }

        internal JsonObject? GetPreferedRegionObject(List<string> preferedRegion, JsonArray? json, string regionAttribute)
        {
            if (json == null || json.Count == 0)
                return null;
           
            foreach (var region in preferedRegion)
            {
                int i = 0;
                while (i < json.Count)
                {
                    if (getJsonValue(json[i], regionAttribute, "", s => s) == region)
                    {
                        return json[i]?.AsObject();
                    }
                    i++;
                }               
            }

            return json[0]?.AsObject();
        }


        internal LanguageInformation GetPreferedLanguageValue(List<string> preferedLanguage, JsonNode json, string languageAttribute, string valueAttribute)
        {
            var result = new LanguageInformation();
            if (json == null)
                return result;
            var array = json.AsArray();
            foreach (var lang in preferedLanguage)
            {
                int i = 0;
                while (result.isEmpty && i < array.Count)
                {
                    if (getJsonValue(json[i], languageAttribute, "", s => s) == lang)
                    {
                        result.value = getJsonValue(json[i], valueAttribute, "", s => s);
                        result.language = getJsonValue(json[i], languageAttribute, "", s => s);
                    }
                    i++;
                }
                if (!result.isEmpty)
                    return result;
            }

            result.value = getJsonValue(json[0], valueAttribute, "", s => s);
            result.language = getJsonValue(json[0], languageAttribute, "", s => s);

            return result;
        }

        internal List<RegionAttributes> TransformRegion(JsonNode? json, string regionAttribute, string valueAttribute)
        {
            var result = new List<RegionAttributes>();
            if (json == null)
                return result;
            foreach (var region in json.AsArray())
            {
                if (region![regionAttribute] != null && region![valueAttribute] != null)
                {
                    var r = region[regionAttribute].GetValue<string>();
                    var v = region[valueAttribute].GetValue<string>();

                    bool find = false;

                    foreach (var item in result)
                    {
                        if (item.value == v)
                        {
                            find = true;
                            if (!item.regions.Contains(r))
                                item.regions.Add(r);
                        }
                    }
                    if (!find)
                    {
                        result.Add(new RegionAttributes() { regions = new List<string> { r }, value = v });
                    }
                }
            }

            return result;
        }

        internal string aggregateValueforSearch(JsonNode? json, string valueAttribute)
        {
            if (json == null)
                return "";
            var result = "";
            foreach (var item in json.AsArray())
            {
                var add = Regex.Replace(item![valueAttribute]!.GetValue<string>(), "[^A-Za-z0-9]+", "").ToLower();
                if (!result.Contains(add))
                    result += add;                
            }

            return result;

        }

        

        internal BsonDocument UnusedJson(JsonNode json)
        {
            var unused = new JsonObject();
            if (json!["medias"]!=null)
                unused.Add("medias", JsonArray.Parse(json!["medias"].ToString()));
            if (json!["noms"] != null)
                unused.Add("noms", JsonArray.Parse(json!["noms"].ToString()));
            if (json!["synopsis"] != null)
                unused.Add("synopsis", JsonArray.Parse(json!["synopsis"].ToString()));
            if (json!["dates"] != null)
                unused.Add("dates", JsonArray.Parse(json!["dates"].ToString()));
            if (json!["genres"] != null)
                unused.Add("genres", JsonArray.Parse(json!["genres"].ToString()));
            if (json!["classifications"] != null)
                unused.Add("classifications", JsonArray.Parse(json!["classifications"].ToString()));
            if (json!["roms"] != null)
                unused.Add("roms", JsonArray.Parse(json!["roms"].ToString()));
            if (json!["rom"] != null)
                unused.Add("rom", JsonObject.Parse(json!["rom"].ToString()));

            return BsonDocument.Parse(unused.ToString());

        }

        internal BsonDocument UnusedSystemJson(JsonNode json)
        {
            var unused = new JsonObject();
            unused.Add("sc", JsonObject.Parse(json.ToString()));

            return BsonDocument.Parse(unused.ToString());

        }

        internal JsonArray? FilterNodes(JsonArray? node, string filterAttribute, string filterValue)
        {
            if (node == null)
                return null;
            var result = new JsonArray();
            foreach (var childNode in node)
            {
                if (childNode[filterAttribute].GetValue<string>() == filterValue)
                {
                    result.Add(JsonObject.Parse(childNode.ToString()));
                }
            }
            return result;

        }

        internal JsonObject? GetMedia (JsonNode json, string mediaType)
        {
            return GetPreferedRegionObject(_settings._preferedRegion, FilterNodes(json["medias"]?.AsArray(), "type", mediaType), "region");
        }

        //public SystemDb? MatchSystem(JsonObject scrapperSystem, List<SystemDb> systems)
        //{
        //    SystemDb? result = null;

        //    if (scrapperSystem == null && scrapperSystem["noms"]!=null)
        //        return result;

        //    var type = getJsonValue(scrapperSystem, "type", "", s => s);

           

        //    var attributesname = new List<string>() { "noms.nom_eu", "noms.nom_jp", "noms.nom_us", "noms.nom_recalbox", "noms.nom_retropie", "noms.nom_launchbox", "noms.nom_hyperspin" };

        //    foreach (var attribute in attributesname)
        //    {
        //        var n = getJsonValue(scrapperSystem, attribute, "", s => s);
        //        if (!string.IsNullOrEmpty(n))
        //            systemitem.Names.Add(Tools.RemoveSpecialCaracteres(n));
        //    }           
        //    var noms_commun = getJsonValue(scrapperSystem, "noms.noms_commun", "", s => s);
        //    if (!string.IsNullOrEmpty(noms_commun))
        //    {
        //        systemitem.Names.AddRange(noms_commun.Split(',').Select(s => Tools.RemoveSpecialCaracteres(s)));
        //    }

            

        //    foreach (var s in systems.Where(s => s.category.ToLower() == type.ToLower()))
        //    {
        //        if (namelist.Exists (n => n == Tools.RemoveSpecialCaracteres(s.name) || n==Tools.RemoveSpecialCaracteres(s._id)))
        //        {
        //            return s;
        //        }
        //    }

        //    return result;
        //}
    }

}
 