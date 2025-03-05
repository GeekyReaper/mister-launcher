using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    [BsonIgnoreExtraElements]
    public class Game
    {
        public string _id { get; set; }
        public bool matchscreenscraper { get; set; }
        public string name { get; set; }
        public string name_region { get; set; }
        public int year { get; set; }
        public int romsize { get; set; }
        public string romname { get; set; }
        public string fullpath { get; set; }
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
        public DateTime romdate { get; set; }
        public DateTime gamedate { get; set; }
        public List<string> gametype { get; set; }
        public List<string>? playlist { get; set; }
        public int? rating { get; set; }

        public int screenscraperId { get; set; }

        public List<RegionAttributes>? names { get; set; }
        public List<RegionAttributes>? dates { get; set; }

        public string collection { get; set; } = "";
        public int collectionId { get; set; } = 0;

        public string core { get; set; } = "";
    }

    

    [BsonIgnoreExtraElements]
    public class GameFull : Game
    {
        [BsonElement("unused")]
        public Details details { get; set; }
    }
    [BsonIgnoreExtraElements]
    
    public class Details
    {
        public List<Media> medias { get; set; } = new List<Media>();
    }
    [BsonIgnoreExtraElements]

    public class RegionAttributes
    {
        public List<string> regions { get; set; } = new List<string>();
        public string value { get; set; } = "";
    }
    public class Media
    {
        public string type { get; set; }
        public string parent { get; set; }
        public string url { get; set; }
        public string region { get; set; }
        public string crc { get; set; }
        public string md5 { get; set; }
        public string sha1 { get; set; }
        public string size { get; set; }
        public string format { get; set; }
        public string id { get; set; }
        public string subparent { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SystemDb
    {
        public string _id { get; set; }
        public string name { get; set; }
        public string gamepath { get; set; }
        public string category { get; set; }
        public int startyear { get; set; }
        public int endyear { get; set; }
        public int generation { get; set; }
        public string abreviation { get; set; }
        public string alternative_name { get; set; }
        public string family { get; set; }
        public string extensions { get; set; } = "";

        public string excluderompaterns { get; set; } = "^boot\\.,^mister-boot\\.,^mister-demo\\.";
        public string unofficalpathrompaterns { get; set; } = "(hacks|homebrew|demo|_alternatives|unlicensed)";
        public string company { get; set; }
        [BsonElement("type")]
        public string systemtype { get; set; }
        public string supporttype { get; set; }
        public string romtype { get; set; }
        public string name_eu { get; set; }
        public string name_jp { get; set; }
        public string name_us { get; set; }

        public bool match { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }

        public string media_logomonochrome { get; set; }
        public string media_photo { get; set; }
        public string media_illustration { get; set; }
        public string media_controller { get; set; }
        public string media_wheel { get; set; }
        public string media_video { get; set; }
        public string media_BoitierConsole3D { get; set; }

        public int screenscraperId { get; set; }

        public bool allowSaveStates { get; set; } = false;
        public bool allowSaveMemory { get; set; } = false;

        public string core { get; set; } = "";



        public int statvideogame { get; set; }
        public int statromfound{ get; set; }
        public int statrommatch { get; set; }

        public List<string> GetAllowExtensions()
        {
            return extensions.Split(",").Select( s => s.Trim()).ToList();
        }

        public List<string> GetExcludeRomPaterns()
        {
            if (string.IsNullOrEmpty(excluderompaterns))
                return new List<string>();
            return excluderompaterns.Split(",").Select(s => s.Trim()).ToList();
        }

        public List<string> GetUnofficialPathRomPaterns()
        {
            if (string.IsNullOrEmpty(unofficalpathrompaterns))
                return new List<string>();
            return unofficalpathrompaterns.Split(",").Select(s => s.Trim()).ToList();
        }
    }

    public class SystemDbFull : SystemDb
    {
        public BsonDocument unused { get; set; }

        public void LoadUnusedJson(string json)
        {
            unused = BsonDocument.Parse(json);
        }
    }


    [BsonIgnoreExtraElements]
    public class GameEnum
    {
        [BsonId]
        public Guid _id { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
    /*
    [BsonIgnoreExtraElements]
    public class Game2
    {
        public string _id { get; set; }
        public bool matchscreenscraper { get; set; }
        public string name { get; set; }
        public string name_region { get; set; }
        public int size { get; set; }
        public string date { get; set; }
        public string name { get; set; }
        public string fullpath { get; set; }
        public string date { get; set; }
        public string date_region { get; set; }
        public string desc { get; set; }
        public string desc_lang { get; set; }
        public string systemid { get; set; }
        public int systemid_sc { get; set; }
        public string systemname_sc { get; set; }
        public string editor_sc { get; set; }
        public string editorname_sc { get; set; }
        public string develop_sc { get; set; }
        public string developname_sc { get; set; }
        public string nbplayers { get; set; }
        public int id_sc { get; set; }
        public string media_fanart { get; set; }
        public string media_video { get; set; }
        public string media_manuel { get; set; }
        public string media_screenshot { get; set; }
        public string media_title { get; set; }
        //public List<Media> medias { get; set; }
        //public List<RomFile> roms { get; set; }
        public List<BsonDocument> medias { get; set; }
        public List<BsonDocument> roms { get; set; }
        public List<ScGameType> gametype_details { get; set; }
        public List<TextByRegion> dates { get; set; }
        public List<TextByLanguage> description_details { get; set; }
        public List<TextByRegion> name_details { get; set; }
        public List<string> gametype { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class Media
    {
        public string type { get; set; }
        public string parent { get; set; }
        public string url { get; set; }
        public string region { get; set; }
        public string crc { get; set; }
        public string md5 { get; set; }
        public string sha1 { get; set; }
        public string size { get; set; }
        public string format { get; set; }
        public string posx { get; set; }
        public string posy { get; set; }
        public string posw { get; set; }
        public string posh { get; set; }
        public string subparent { get; set; }
        [BsonElement ("romid")]
        public string? mediaid { get; set; }

        public string? support { get; set; }
        public string? version { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class LanguageDescription
    {
        public List<string> langues_id { get; set; }
        public List<string> langues_shortname { get; set; }
        public List<string> langues_en { get; set; }
        public List<string> langues_fr { get; set; }
        public List<string> langues_de { get; set; }
        public List<string> langues_es { get; set; }
        public List<string> langues_it { get; set; }
        public List<string> langues_pt { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class RegionDescription
    {
        public List<string> regions_id { get; set; }
        public List<string> regions_shortname { get; set; }
        public List<string> regions_en { get; set; }
        public List<string> regions_fr { get; set; }
        public List<string> regions_de { get; set; }
        public List<string> regions_es { get; set; }
        public List<string> regions_pt { get; set; }
        public List<string> regions_it { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class RomFile
    {
        [BsonElement ("romid")]
        public string romid { get; set; }
        public string size { get; set; }
        public string romfilename { get; set; }
        public string romnumsupport { get; set; }
        public string romtotalsupport { get; set; }
        public string romcloneof { get; set; }
        public string romcrc { get; set; }
        public string rommd5 { get; set; }
        public string romsha1 { get; set; }
        public string beta { get; set; }
        public string demo { get; set; }
        public string proto { get; set; }
        public string trad { get; set; }
        public string hack { get; set; }
        public string unl { get; set; }
        public string alt { get; set; }
        public string best { get; set; }
        public string netplay { get; set; }
        public LanguageDescription langues { get; set; }
        public RegionDescription regions { get; set; }
        public string clonetypes { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class TextByLanguage
    {
        public string langue { get; set; }
        public string value { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class ScGameType
    {
        [BsonElement("romid")]
        public string typeid { get; set; }
        public string nomcourt { get; set; }
        public string principale { get; set; }
        public string parentid { get; set; }
        public List<TextByLanguage> noms { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class TextByRegion
    {
        public string region { get; set; }
        public string value { get; set; }
    }

    //}
    */
}
