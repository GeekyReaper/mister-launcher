using Amazon.SecurityToken.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    [BsonIgnoreExtraElements]
    public class VideoGameDb
    {
        public string _id { get; set; }
        public int screenscraperId { get; set; }
        public string name { get; set; }
        public string name_region { get; set; }

        public string namesearch { get; set; }
        public int year { get; set; }
        public string systemcategory { get; set; }
        public string date_region { get; set; }
        public string desc { get; set; }
        public string desc_lang { get; set; }
        public string systemid { get; set; }
        public string systemname { get; set; }
        public string editorname { get; set; }
        public string developname { get; set; }
        public string nbplayers { get; set; }
        public string? media_fanart { get; set; }
        public string? media_video { get; set; }      
        public string? media_manuel { get; set; }
        public string? media_screenshot { get; set; }
        public string? media_title { get; set; }
        public DateTime gamedate { get; set; }
        public List<string> gametype { get; set; }
        public List<string>? playlist { get; set; }
        public int? rating { get; set; }        

        public List<RegionAttributes>? names { get; set; }
        public List<RegionAttributes>? dates { get; set; }

        public string collection { get; set; } = "";
        public int collectionId { get; set; } = 0;

        public string core { get; set; } = "";

        public List<RomDb>? roms { get; set; }

        public int romscount { get; set; } = 0;

        public int playedhit { get; set; } = 0;
        public DateTime playedlast { get; set; } = DateTime.MinValue;

    }

    public class VideoGameFullDb : VideoGameDb
    {
        public BsonDocument unused { get; set; }

        public void LoadUnusedJson (string json)
        {
            unused = BsonDocument.Parse(json);
        }

        public bool setId ()
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            var value = string.Format("{0}-{1}", systemcategory == "Arcade" ? "Arcade" : systemid, name);
            
            var t = Regex.Replace(value, "[^A-Za-z0-9]+", "-");
            while (t.Length > 0 && t.EndsWith("-"))
            {
                t = t.Substring(0, t.Length - 1);
            }
            _id = t.ToLower();
            return true;
        }


        public VideoGameDb CastToVideoGame ()
        {
            return new VideoGameDb()
            {
                _id = this._id,
                screenscraperId = this.screenscraperId,
                name = this.name,
                name_region = this.name_region,
                namesearch = this.namesearch,
                year = this.year,
                systemcategory = this.systemcategory,
                date_region = this.date_region,
                desc = this.desc,
                desc_lang = this.desc_lang,
                systemid = this.systemid,
                systemname = this.systemname,
                editorname = this.editorname,
                developname = this.developname,
                nbplayers = this.nbplayers,
                media_fanart = this.media_fanart,
                media_video = this.media_video,
                media_manuel = this.media_manuel,
                media_screenshot = this.media_screenshot,
                media_title = this.media_title,
                gamedate = this.gamedate,
                gametype = this.gametype,
                rating = this.rating,
                names = this.names,
                dates = this.dates,
                collection = this.collection,
                collectionId = this.collectionId,
                core = this.core,
                roms = this.roms,
                romscount = this.romscount
            };
     
        }

    }

    [BsonIgnoreExtraElements]
    public class RomDb
    {
        public string romid { get; set; }
        public string fullpath { get; set; }
        public string name { get; set; }
        public long size { get; set; }
        public DateTime date { get; set; }

        public MraInfo? mrainfo { get; set; }
        public string region { get; set; }
        public string language { get; set; }
        public string supportType { get; set; }
        public int screenscrapperId { get;set;}

        public string core { get; set; } = "";
        
    }

    public class VideoGameSearchRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Regex { get; set; } = string.Empty;
        public List<string> Systems { get; set; } = new List<string>();

        public string NbPlayers { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public int YearMax { get; set; } = 0;
        public int YearMin { get; set; } = 0;
        public bool AllowUnknowYear { get; set; } = false;
        public bool AllowUnRated { get; set; } = false;
        public int MinRating { get; set; } = 0;
        public string SystemCategory { get; set; } = string.Empty;
        public int MaxRating { get; set; } = 0;
        public string Editor { get; set; } = string.Empty;
        public string Developname { get; set; } = string.Empty;

        public string Playlist { get; set; } = string.Empty;

        public List<string> GameType { get; set; } = new List<string>();
        public List<string> GameTypeExcluded { get; set; } = new List<string>();

        public string Collection { get; set; } = "";

        public string Core { get; set; } = string.Empty;

        public List<string> GamesExcluded { get; set; } = new List<string>();
        public List<string> SystemsExcluded { get; set; } = new List<string>();


        public int? limit { get; set; } = 0;

        public int? pagesize { get; set; } = 20;

        public int? page { get; set; } = 1;

        public List<SortDb> SortFields { get; set; } = new List<SortDb>();

        public int playedhitMin { get; set; } = -1;
        public int playedhitMax { get; set; } = -1;

        public DateTime playedlastMin { get; set; } = DateTime.MinValue;
        public DateTime playedlastMax { get; set; } = DateTime.MinValue;


        public VideoGameSearchRequest Clone()
        {
            var copy = new VideoGameSearchRequest();
            copy.Name = Name;
            copy.Regex = Regex;
            copy.NbPlayers = NbPlayers;
            copy.Year = Year;
            copy.YearMax = YearMax;
            copy.YearMin = YearMin;
            copy.AllowUnknowYear = AllowUnknowYear;
            copy.AllowUnRated = AllowUnRated;
            copy.MinRating = MinRating;
            copy.SystemCategory = SystemCategory;            
            copy.MaxRating = MaxRating;
            copy.Editor = Editor;
            copy.Developname = Developname;

            foreach( var s in Systems)
            {
                copy.Systems.Add(s);
            }

            foreach (var s in GameType)
            {
                copy.GameType.Add(s);
            }
            foreach (var s in SortFields)
            {
                copy.SortFields.Add(new SortDb(s.Field, s.IsAscending));
            }


            return copy;
        }

    }

    public class VideoGameSearchResult
    {
        public List<VideoGameDb> videogames { get; set; } = new List<VideoGameDb>();
        public FilterOption? filterOption { get; set; }

        public int count { get; set; } = 0;
        public int page { get; set; } = 1;

        public int pagesize { get; set; } = 20;
       
    }
    public enum EFilterOptionCategory {  GameType, SystemCategory , NbPlayers, Collections, Editors, Developers }
    
    public class FilterOption
    {
        

        internal Dictionary<EFilterOptionCategory, List<string>> filterItems = new Dictionary<EFilterOptionCategory, List<string>>();

        public List<string> gameTypes { get { return filterItems[EFilterOptionCategory.GameType]; } }
        public List<string> nbPlayers { get { return filterItems[EFilterOptionCategory.NbPlayers]; } }        
        public List<string> collections { get { return filterItems[EFilterOptionCategory.Collections]; } }
        public List<string> editors { get { return filterItems[EFilterOptionCategory.Editors]; } }
        public List<string> developers { get { return filterItems[EFilterOptionCategory.Developers]; } }
        public List<string> SystemCategory { get { return filterItems[EFilterOptionCategory.SystemCategory]; } }

        public List<SystemDb> systemDbs { get; } = new List<SystemDb>();

        public int MinYear { get; set; } = int.MaxValue;
        public int MaxYear { get; set; } = int.MinValue;
       


        public FilterOption()
        {
            foreach (var e in Enum.GetValues(typeof(EFilterOptionCategory)).Cast<EFilterOptionCategory>())
            {
                filterItems.Add(e, new List<string>());
            }
        }

        public void SetYearInterval (int year)
        {
            MinYear = Math.Min(year, MinYear);
            MaxYear = Math.Max(year, MaxYear);
        }
        public void AddValueToCategory(EFilterOptionCategory cat, string value)
        {
            if (!string.IsNullOrEmpty(value) && !filterItems[cat].Contains(value))
                filterItems[cat].Add(value);
        }

        public void SetSort ()
        {
            foreach (var e in Enum.GetValues(typeof(EFilterOptionCategory)).Cast<EFilterOptionCategory>())
            {
                filterItems[e] = filterItems[e].OrderBy(e => e).ToList();
            }
        }

    
        //public void AddGameType(string gametype)
        //{
        //    if (!gameTypes.Contains(gametype))
        //    {
        //        gameTypes.Add(gametype);
        //    }
        //}

        //public void AddEditors(string value)
        //{
        //    if (!editors.Contains(value))
        //    {
        //        editors.Add(value);
        //    }
        //}
        //public void AddDeveloper(string value)
        //{
        //    if (!developers.Contains(value))
        //    {
        //        developers.Add(value);
        //    }
        //}

        //public void AddCollection (string value)
        //{
        //    if (!collections.Contains(value))
        //    {
        //        collections.Add(value);
        //    }
        //}

        //public void AddNbPlayers(string nbplayers)
        //{
        //    if (!nbPlayers.Contains(nbplayers))
        //    {
        //        nbPlayers.Add(nbplayers);
        //    }
        //}

        //public void AddSystemCategory(string category)
        //{
        //    if (!SystemCategory.Contains(category))
        //    {
        //        SystemCategory.Add(category);
        //    }
        //}

        public bool GameSystemsExist(string systemid)
        {
            return systemDbs.Where(s => s._id == systemid).Any();
        }

        public void AddGameSystem(SystemDb? gameSystem)
        {
            if (gameSystem != null)
            {
                systemDbs.Add(gameSystem);
            }
        }
    }

    public class FilterItem
    {
        public string label { get; set; } = "";
        public string value { get; set; } = "";
    }

    public class PlayingGame
    {
        public VideoGameDb? currentVideogame { get; set; }
        public RomDb? playingRomdb { get; set; }

        public SystemDb? systemDb { get; set; }

        public bool IsPlaying {  get { return currentVideogame != null && playingRomdb != null; } }

        public void SetVideoGame(VideoGameDb? vgdb, string core)
        {

            if (vgdb != null && vgdb.roms != null)
            {
                currentVideogame = vgdb;
                foreach (var r in currentVideogame.roms)
                {
                    if (
                        (currentVideogame.systemcategory=="Console" && r.name.ToLower() == core.ToLower())
                        || (currentVideogame.systemcategory == "Arcade" && r.core.ToLower() == core.ToLower())
                        )
                    {
                        playingRomdb = r;
                    }
                }
            }
        }

        public string getId()
        {
            if (!IsPlaying)
                return "";
            return string.Format(string.Format("{0}/{1}", currentVideogame._id, playingRomdb.romid));
        }

    }
}
